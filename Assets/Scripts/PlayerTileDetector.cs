using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTileDetector : MonoBehaviour
{

    public Transform chunksRoot;
    public List<TileBase> stairUpTiles;

    [SerializeField] private Vector2 worldOrigin = new Vector2(-9, -4);
    [SerializeField] private int chunkSize = 32;

    private Vector3? previousPosition = null;

    private PlayerAttributes playerAttributes;

    void Awake()
    {
        playerAttributes = GetComponent<PlayerAttributes>();
    }
    void Update()
    {
        if (playerAttributes.IsMoving) return;
        if (previousPosition == transform.position) return;
        previousPosition = transform.position;

        ProcessTileTrigger();
    }

    Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        Vector2 adjusted = new Vector2(worldPos.x - worldOrigin.x, worldPos.y - worldOrigin.y);
        int x = Mathf.FloorToInt(adjusted.x / chunkSize);
        int y = Mathf.FloorToInt(adjusted.y / chunkSize);
        return new Vector2Int(x, y);
    }


    List<Tilemap> GetTilemapByPosition(int x, int y, int height)
    {
        string heightName = $"Chunks_{height}";
        string tilemapName = $"Chunk_{x}_{y}";

        foreach (Transform chunkGroup in chunksRoot)
        {
            if (chunkGroup.name != heightName) continue;
            foreach (Transform tilemapTransform in chunkGroup)
            {
                if (tilemapTransform.name == tilemapName)
                {
                    List<Tilemap> tilemaps = new List<Tilemap>();

                    foreach (Transform child in tilemapTransform)
                    {
                        Tilemap tilemap = child.GetComponent<Tilemap>();
                        if (tilemap != null)
                        {
                            tilemaps.Add(tilemap);
                        }
                    }

                    return tilemaps;
                }
            }
        }

        return null;
    }

    void ProcessTileTrigger()
    {
        Vector2Int playerChunkCoord = GetChunkCoord(transform.position);

        var tilemaps = GetTilemapByPosition(playerChunkCoord.x, playerChunkCoord.y, (int)transform.position.z);

        if (tilemaps == null || tilemaps.Count == 0) return;

        playerAttributes.CurrentTilemap = tilemaps;

        foreach (var tilemap in tilemaps)
        {
            if (!tilemap) continue;

            Vector3Int tilePos = tilemap.WorldToCell(transform.position);
            playerAttributes.CurrentTile = tilemap.GetTile(tilePos);

            if (playerAttributes.CurrentTile != null && stairUpTiles.Contains(playerAttributes.CurrentTile))
            {
                PerformEnterStair();
            }
        }
    }

    void PerformEnterStair()
    {
        Vector3 pos = transform.position;
        pos.y += 1.0f;
        pos.z += 1.0f;
        transform.position = pos;
    }
}
