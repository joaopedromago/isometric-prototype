using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewCustomTile", menuName = "Tiles/Custom Tile")]
public class CustomTile : Tile
{
    public bool isStair = true;
    public bool isStairDown = false;

    public bool isWindow = false;
}
