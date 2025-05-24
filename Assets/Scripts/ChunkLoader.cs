using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

public class ChunkLoader : MonoBehaviour
{
    public Transform point;
    public int chunkSize = 32;
    public int viewDistance = 1; // 1 = 3x3 chunks

    private Dictionary<Vector3Int, GameObject> loadedChunks = new Dictionary<Vector3Int, GameObject>();
    private Transform chunksRoot;
    private Vector3Int? previousPosition = null;
    [SerializeField] private Vector2 worldOrigin = new Vector2(-9, -4);


    void Start()
    {
        chunksRoot = GameObject.Find("Chunks").transform;
        CacheAllChunks();
    }

    void Update()
    {
        Vector3Int currentChunk = GetChunkCoord(point.position);
        UpdateChunks(currentChunk);
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

    Vector3Int GetChunkCoord(Vector3 worldPos)
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
        print("UPDATE CHUNKS!" + position);

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

            if (pair.Value.transform?.parent?.name == "Chunks_" + height)
            {
                UpdateCollider(pair.Value, false);
            }
            else
            {
                UpdateCollider(pair.Value, true);
            }
        }

        if (position != previousPosition)
        {
            previousPosition = position;
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
}
