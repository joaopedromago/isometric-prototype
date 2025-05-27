using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingEnemy : MonoBehaviour
{
    public float tileSize = 1f;
    public Transform player;
    private float MoveSpeed = 4f;
    private bool IsMoving = false;
    public Vector3 TargetPos;
    private PlayerTileDetector playerTileDetector;

    static readonly Vector2[] Movements = new Vector2[]
    {
        new Vector2( 1,  0),
        new Vector2(-1,  0),
    };

    private int MovesMade = 0;

    void Awake()
    {
        playerTileDetector = player?.GetComponent<PlayerTileDetector>();
    }
    void Start()
    {
        TargetPos = transform.position;
    }

    void LateUpdate()
    {
        if (!IsMoving)
        {
            var move = Movements[MovesMade];
            var xDir = move.x;
            var yDir = move.y;

            Vector3[] moveAttempts = new Vector3[]
            {
                new Vector3(xDir, yDir, 0),
                new Vector3(0, yDir, 0),
                new Vector3(xDir, 0, 0)
            };
            foreach (Vector3 attempt in moveAttempts)
            {
                if (attempt == Vector3.zero) continue;

                Vector3 desiredPos = transform.position + attempt * tileSize;

                Collider2D[] targetColliders = Physics2D.OverlapCircleAll(desiredPos, 0.2f);
                bool haveCollision = targetColliders.Any(c => c != null && ShouldCollide(c));

                if (!haveCollision && HasTile(desiredPos))
                {
                    GetComponent<BoxCollider2D>().offset = new Vector2(attempt.x, attempt.y);
                    TargetPos = desiredPos;
                    IsMoving = true;

                    if (MovesMade == (Movements.Length - 1))
                    {
                        MovesMade = 0;
                    }
                    else
                    {
                        MovesMade++;
                    }
                    break;
                }
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, TargetPos, MoveSpeed * Time.deltaTime);

            GetComponent<BoxCollider2D>().offset = new Vector2(TargetPos.x - transform.position.x, TargetPos.y - transform.position.y);

            if (transform.position == TargetPos)
            {
                IsMoving = false;
            }
        }
    }

    bool ShouldCollide(Collider2D targetCollider)
    {
        Transform parent = targetCollider?.transform?.parent?.parent;
        if (parent == null)
        {
            return true;
        }

        return IsSameChunk(parent);
    }

    bool IsSameChunk(Transform chunk)
    {
        if (chunk != null)
        {
            string[] parts = chunk.name.Split('_');

            if (parts.Length > 1 && int.TryParse(parts[1], out int number))
            {
                if (number != (int)transform.position.z)
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool HasTile(Vector3 position)
    {
        Vector2Int chunkChoord = playerTileDetector.GetChunkCoord(position);

        var tilemaps = playerTileDetector.GetTilemapByPosition(chunkChoord.x, chunkChoord.y, (int)position.z);

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

        return false;
    }
}
