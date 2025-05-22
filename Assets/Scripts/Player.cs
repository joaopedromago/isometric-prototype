using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttributes : MonoBehaviour
{
    public Collider2D Collider { get; set; }
    public float MoveSpeed { get; set; } = 10f;

    public Vector3 MoveDir { get; set; }
    public Vector3 PreviousPos { get; set; }
    public Vector3 TargetPos { get; set; }

    public bool IsMoving { get; set; } = false;
    public bool OnCollision { get; set; } = false;

    public TileBase CurrentTile { get; set; }
    public List<Tilemap> CurrentTilemap { get; set; }


    void Start()
    {
        Collider = GetComponent<Collider2D>();
        TargetPos = transform.position;
    }
}
