using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttributes : MonoBehaviour
{
    public Collider2D Collider { get; set; }
    public float MoveSpeed { get; set; } = 4f;

    public Vector3 MoveDir { get; set; }
    public Vector3 PreviousPos { get; set; }
    public Vector3 TargetPos { get; set; }

    public bool IsMoving { get; set; } = false;
    public bool OnCollision { get; set; } = false;

    public List<Tilemap> CurrentTilemap { get; set; }
    public Tilemap? CurrentGroundTilemap
    {
        get => CurrentTilemap?.Find(tilemap => tilemap != null && tilemap.name == "Ground");
    }

    public Vector3Int? CurrentTile
    {
        get => CurrentGroundTilemap?.WorldToCell(transform.position);
    }
    public Vector3? WorldPosition
    {
        get => CurrentTile != null
            ? CurrentGroundTilemap?.GetCellCenterWorld(CurrentTile.Value)
            : null;
    }

    void Start()
    {
        Collider = GetComponent<Collider2D>();
        TargetPos = transform.position;
    }
}
