using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class PathFinder
{
    public static TileNode FindClosestNode(Vector3 worldPos, List<TileNode> tiles)
    {
        return tiles.OrderBy(t => Vector3.Distance(t.globalPosition, worldPos)).FirstOrDefault();
    }

    public static float Heuristic(TileNode a, TileNode b)
    {
        return Mathf.Abs(a.position.x - b.position.x) + Mathf.Abs(a.position.y - b.position.y);
    }

    public static List<TileNode> GetNeighbors(TileNode node, List<TileNode> tiles)
    {
        List<TileNode> neighbors = new();
        Vector3Int[] directions = new Vector3Int[]
        {
        new(1, 0, 0),   // East
        new(-1, 0, 0),  // West
        new(0, 1, 0),   // North
        new(0, -1, 0)   // South
        };

        foreach (var dir in directions)
        {
            var neighborPos = node.position + dir;
            var neighbor = tiles.FirstOrDefault(t => t.position == neighborPos);
            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    public static Vector2Int[] ReconstructPath(Dictionary<TileNode, TileNode> cameFrom, TileNode current)
    {
        List<Vector2Int> totalPath = new();

        while (cameFrom.ContainsKey(current))
        {
            var previous = cameFrom[current];
            var dir = new Vector2Int(
                current.position.x - previous.position.x,
                current.position.y - previous.position.y
            );
            totalPath.Insert(0, dir);
            current = previous;
        }

        return totalPath.ToArray();
    }

}
