using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float tileSize = 1f;
    private PlayerAttributes playerAttributes;
    private PlayerTileDetector playerTileDetector;

    void Awake()
    {
        playerTileDetector = GetComponent<PlayerTileDetector>();
        playerAttributes = GetComponent<PlayerAttributes>();
    }

    void Update()
    {
        if (!playerAttributes.IsMoving)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            int xDir = x < 0 ? (int)Math.Floor(x) :
                       x > 0 ? (int)Math.Ceiling(x) : 0;

            int yDir = y < 0 ? (int)Math.Floor(y) :
                       y > 0 ? (int)Math.Ceiling(y) : 0;

            playerAttributes.MoveDir = new Vector3(xDir, yDir, 0);

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

                Collider2D[] targetColliders = Physics2D.OverlapCircleAll(desiredPos, 0.1f);
                bool haveCollision = targetColliders.Any(c => c != null && ShouldCollide(c));

                if (!haveCollision && HasTile(desiredPos))
                {
                    playerAttributes.MoveDir = attempt;
                    playerAttributes.PreviousPos = transform.position;
                    playerAttributes.TargetPos = desiredPos;
                    playerAttributes.IsMoving = true;
                    playerAttributes.Collider.enabled = false;
                    break;
                }
            }
        }
        if (playerAttributes.IsMoving)
        {
            if (playerAttributes.OnCollision)
            {
                playerAttributes.TargetPos = playerAttributes.PreviousPos;
                playerAttributes.OnCollision = false;
            }
            transform.position = Vector3.MoveTowards(transform.position, playerAttributes.TargetPos, playerAttributes.MoveSpeed * Time.deltaTime);

            if (transform.position == playerAttributes.TargetPos)
            {
                playerAttributes.IsMoving = false;
                playerAttributes.Collider.enabled = true;
            }
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        playerAttributes.OnCollision = true;
    }

    bool ShouldCollide(Collider2D targetCollider)
    {
        Transform parent = targetCollider?.transform?.parent?.parent;
        if (parent == null)
        {
            return false;
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
