using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGrid : MonoBehaviour
{
    public GameObject chunksRoot;

    public List<TileNode> Tiles { get; set; }

    void Start()
    {
        ScanTilemapArea();
    }
    void ScanTilemapArea()
    {
        Tiles = new List<TileNode>();

        Tilemap[] tilemaps = chunksRoot.GetComponentsInChildren<Tilemap>(true);

        Dictionary<(int x, int y, int height), List<(Tilemap tilemap, TileBase tile)>> nodeMap = new();

        foreach (var tilemap in tilemaps)
        {
            Transform parent = tilemap.transform.parent?.parent;
            if (parent == null)
            {
                Debug.LogWarning($"Tilemap {tilemap.name} has no grandparent, skipping.");
                continue;
            }

            string parentName = parent.name;
            int height = 0;
            if (!int.TryParse(parentName.Split('_').Last(), out height))
            {
                Debug.LogWarning($"Failed to parse height from {parentName}, defaulting to 0.");
            }

            var bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(pos);

                    if (tile != null)
                    {
                        var key = (x, y, height);
                        if (!nodeMap.ContainsKey(key))
                            nodeMap[key] = new List<(Tilemap, TileBase)>();

                        nodeMap[key].Add((tilemap, tile));
                    }
                }
            }
        }

        foreach (var kvp in nodeMap)
        {
            var (x, y, height) = kvp.Key;
            var tileList = kvp.Value;

            var pos = new Vector3Int(x, y, height);
            var worldPos = tileList[0].tilemap.CellToWorld(new Vector3Int(x, y, 0));

            bool walkable = DetermineWalkable(tileList);

            TileNode node = new TileNode(pos, worldPos, tileList, walkable);
            Tiles.Add(node);

            Debug.Log(node.ToString());
        }
    }



    bool DetermineWalkable(List<(Tilemap tilemap, TileBase tile)> tileList)
    {
        foreach (var tileData in tileList)
        {
            var tile = tileData.tile;
            var tilemap = tileData.tilemap;

            if (tilemap.GetComponent<TilemapCollider2D>() != null)
            {
                return false;
            }

            if (tile != null && tile is CustomTile customTile)
            {
                if (customTile.isStair || customTile.isStairDown)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public Vector2Int[] GetWalkPath(Vector3 origin, Vector3 destination)
    {
        TileNode startNode = PathFinder.FindClosestNode(origin, Tiles);
        TileNode endNode = PathFinder.FindClosestNode(destination, Tiles);

        if (startNode == null || endNode == null || !startNode.walkable || !endNode.walkable)
            return Array.Empty<Vector2Int>();

        var openSet = new PriorityQueue<TileNode>();
        var cameFrom = new Dictionary<TileNode, TileNode>();
        var gScore = new Dictionary<TileNode, float>();
        var fScore = new Dictionary<TileNode, float>();

        foreach (var node in Tiles)
        {
            gScore[node] = float.MaxValue;
            fScore[node] = float.MaxValue;
        }

        gScore[startNode] = 0;
        fScore[startNode] = PathFinder.Heuristic(startNode, endNode);

        openSet.Enqueue(startNode, fScore[startNode]);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == endNode)
                return PathFinder.ReconstructPath(cameFrom, current);

            foreach (var neighbor in PathFinder.GetNeighbors(current, Tiles))
            {
                if (!neighbor.walkable)
                    continue;

                float tentativeG = gScore[current] + 1;

                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + PathFinder.Heuristic(neighbor, endNode);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return Array.Empty<Vector2Int>();
    }
}
