using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileNode
{
    public Vector3Int position;
    public Vector3 globalPosition;
    public List<(Tilemap tilemap, TileBase tile)> tiles;
    public bool walkable;

    public TileNode(Vector3Int position, Vector3 globalPosition, List<(Tilemap, TileBase)> tiles, bool walkable)
    {
        this.position = position;
        this.globalPosition = globalPosition;
        this.tiles = tiles;
        this.walkable = walkable;
    }


    public override string ToString()
    {
        return $"TileNode [Pos: {position}, WorldPos: {globalPosition}, Tiles: {tiles.Count}, Walkable: {walkable}]";
    }
}

