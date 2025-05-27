using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;

public class ChunkLoader : MonoBehaviour
{
    public Transform point;
    public int chunkSize = 32;
    public int viewDistance = 1; // 1 = 3x3 chunks

    private Dictionary<Vector3Int, GameObject> loadedChunks = new Dictionary<Vector3Int, GameObject>();
    public Transform chunksRoot;
    private Vector3Int? previousPosition = null;
    private Vector3? previousPointPosition = null;
    [SerializeField] private Vector2 worldOrigin = new Vector2(-9, -4);

    private PlayerTileDetector playerTileDetector;
    private PlayerAttributes playerAttributes;

    static readonly Vector3[] neighborOffsets = new Vector3[]
    {
        new Vector3( 1,  0, 0),
        new Vector3(-1,  0, 0),
        new Vector3( 0,  1, 0),
        new Vector3( 0, -1, 0),
    };


    void Awake()
    {
        playerTileDetector = point?.GetComponent<PlayerTileDetector>();
        playerAttributes = point?.GetComponent<PlayerAttributes>();
    }

    void Start()
    {
        chunksRoot = GameObject.Find("Chunks").transform;
        CacheAllChunks();
    }

    void Update()
    {
        Vector3Int currentChunk = GetChunkCoord(point.position);
        UpdateChunks(currentChunk);
        UpdateChunksDisplay(currentChunk);
    }

    void CacheAllChunks()
    {
        foreach (Transform child in chunksRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("Chunk_"))
            {
                Vector3Int coord = ParseChunkName(child.name, child.transform.parent.name);
                loadedChunks[coord] = child.gameObject;
            }
        }
    }

    Vector3Int ParseChunkName(string name, string parentName)
    {
        // Ex: "Chunk_0_1"
        string[] parts = name.Split('_');
        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);
        int z = int.Parse(parentName.Split('_')[1]);
        return new Vector3Int(x, y, z);
    }

    public Vector3Int GetChunkCoord(Vector3 worldPos)
    {
        Vector2 adjusted = new Vector2(worldPos.x - worldOrigin.x, worldPos.y - worldOrigin.y);
        int x = Mathf.FloorToInt(adjusted.x / chunkSize);
        int y = Mathf.FloorToInt(adjusted.y / chunkSize);
        return new Vector3Int(x, y, (int)worldPos.z);
    }


    void UpdateChunks(Vector3Int position)
    {
        var hasChange = previousPosition != position;
        if (!hasChange) return;

        int height = (int)point.transform.position.z;

        HashSet<Vector3Int> chunksToKeep = new HashSet<Vector3Int>();

        for (int dx = -viewDistance; dx <= viewDistance; dx++)
        {
            for (int dy = -viewDistance; dy <= viewDistance; dy++)
            {
                foreach (Transform child in chunksRoot.transform)
                {
                    Vector3Int pos = new Vector3Int(position.y + dy, position.x + dx, int.Parse(child.name.Split('_')[1]));
                    if (loadedChunks.TryGetValue(pos, out GameObject chunk))
                    {
                        chunk.SetActive(true);
                        chunksToKeep.Add(pos);

                        if (chunk.transform?.parent?.name == "Chunks_" + height)
                        {
                            UpdateCollider(chunk, false);
                        }
                        else
                        {
                            UpdateCollider(chunk, true);
                        }
                    }
                }
            }
        }

        foreach (var pair in loadedChunks)
        {
            if (!chunksToKeep.Contains(pair.Key))
            {
                pair.Value.SetActive(false);
            }
        }

        if (position != previousPosition)
        {
            previousPosition = position;
        }
    }

    void UpdateChunksDisplay(Vector3Int currentChunk)
    {
        if (playerAttributes.IsMoving) return;
        var hasChange = previousPointPosition != point.position;
        if (!hasChange) return;
        previousPointPosition = point.position;

        int? heightToHide = null;

        for (int dx = -viewDistance; dx <= viewDistance; dx++)
        {
            for (int dy = -viewDistance; dy <= viewDistance; dy++)
            {
                foreach (Transform child in chunksRoot.transform)
                {
                    Vector3Int pos = new Vector3Int(currentChunk.y + dy, currentChunk.x + dx, int.Parse(child.name.Split('_')[1]));
                    if (loadedChunks.TryGetValue(pos, out GameObject chunk))
                    {
                        var chunkHeight = int.Parse(child.name.Split('_')[1]);

                        if ((heightToHide != null && chunkHeight >= heightToHide) || ShouldHideChunk(chunkHeight, (int)point.position.z, point.position, true))
                        {
                            UpdateDisplay(chunk, false);
                            heightToHide = chunkHeight;
                        }
                        else
                        {
                            UpdateDisplay(chunk, true);
                        }
                    }
                }
            }
        }
    }

    void UpdateCollider(GameObject chunk, bool value)
    {
        foreach (Transform child in chunk.transform)
        {
            Collider2D collider = child.GetComponent<Collider2D>();

            if (collider != null)
            {
                Physics2D.IgnoreCollision(collider, point.GetComponent<Collider2D>(), value);
            }
        }
    }

    void UpdateDisplay(GameObject chunk, bool value)
    {
        foreach (Transform child in chunk.transform)
        {
            Tilemap tilemap = child?.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                Color color = tilemap.color;
                color.a = value ? 1f : 0f;
                tilemap.color = color;
            }
        }
    }

    bool ShouldHideChunk(int chunkHeight, int surfaceHeight, Vector3 position, bool validateParents = false)
    {
        if (chunkHeight <= surfaceHeight)
        {
            return false;
        }

        Vector2Int chunkChoord = playerTileDetector.GetChunkCoord(position);

        var tilemaps = playerTileDetector.GetTilemapByPosition(chunkChoord.x, chunkChoord.y, chunkHeight);
        if (tilemaps == null || tilemaps.Count == 0) return false;

        foreach (var tilemap in tilemaps)
        {
            if (!tilemap) continue;

            Vector3Int tilePos = tilemap.WorldToCell(position);
            TileBase tile = tilemap.GetTile(tilePos);

            if (tile != null)
            {
                return true;
            }
        }

        return validateParents && ValidateTilesAround(position, chunkHeight, surfaceHeight);
    }

    bool ValidateTilesAround(Vector3 position, int chunkHeight, int surfaceHeight)
    {
        foreach (var offset in neighborOffsets)
        {
            if (ValidateParentTile(position + offset, chunkHeight, surfaceHeight))
                return true;
        }
        return false;
    }


    bool ValidateParentTile(Vector3 position, int chunkHeight, int surfaceHeight)
    {
        Vector2Int chunkChoord = playerTileDetector.GetChunkCoord(position);

        var tilemaps = playerTileDetector.GetTilemapByPosition(chunkChoord.x, chunkChoord.y, surfaceHeight);

        if (tilemaps == null) return false;

        foreach (var tilemap in tilemaps)
        {
            if (!tilemap) continue;

            Vector3Int tilePos = tilemap.WorldToCell(position);
            TileBase tile = tilemap.GetTile(tilePos);
            if (tile != null)
            {
                if (tilemap.GetComponent<Collider2D>() != null)
                {
                    if (tile is CustomTile customTile)
                    {
                        if (!customTile.isWindow)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

            }
        }

        return ShouldHideChunk(chunkHeight, surfaceHeight, position);
    }
}
