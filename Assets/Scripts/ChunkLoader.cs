using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChunkLoader : MonoBehaviour
{
    public Transform player;
    public int chunkSize = 32;
    public int viewDistance = 1; // 1 = 3x3 chunks

    private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();
    private Transform chunksRoot;
    private Vector2Int? previousChunk = null;
    [SerializeField] private Vector2 worldOrigin = new Vector2(-9, -4);


    void Start()
    {
        chunksRoot = GameObject.Find("Chunks").transform;
        CacheAllChunks();
    }

    void Update()
    {
        Vector2Int currentChunk = GetChunkCoord(player.position);
        UpdateChunks(currentChunk);
    }

    void CacheAllChunks()
    {
        foreach (Transform child in chunksRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("Chunk_"))
            {
                Vector2Int coord = ParseChunkName(child.name);
                loadedChunks[coord] = child.gameObject;
            }
        }
    }

    Vector2Int ParseChunkName(string name)
    {
        // Ex: "Chunk_0_1"
        string[] parts = name.Split('_');
        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);
        return new Vector2Int(x, y);
    }

    Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        Vector2 adjusted = new Vector2(worldPos.x - worldOrigin.x, worldPos.y - worldOrigin.y);
        int x = Mathf.FloorToInt(adjusted.x / chunkSize);
        int y = Mathf.FloorToInt(adjusted.y / chunkSize);
        return new Vector2Int(x, y);
    }


    void UpdateChunks(Vector2Int center)
    {
        var hasChange = previousChunk != center;
        if (!hasChange) return;

        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        for (int dx = -viewDistance; dx <= viewDistance; dx++)
        {
            for (int dy = -viewDistance; dy <= viewDistance; dy++)
            {
                Vector2Int pos = new Vector2Int(center.y + dy, center.x + dx);
                if (loadedChunks.TryGetValue(pos, out GameObject chunk))
                {
                    chunk.SetActive(true);
                    chunksToKeep.Add(pos);
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

        if (center != previousChunk)
        {
            previousChunk = center;
        }
    }

}
