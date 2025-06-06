using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float tileSize = 1f;
    public Transform chunkManager;
    private PlayerAttributes playerAttributes;
    private PlayerTileDetector playerTileDetector;
    private TileGrid tileGrid;

    void Awake()
    {
        playerTileDetector = GetComponent<PlayerTileDetector>();
        playerAttributes = GetComponent<PlayerAttributes>();
        tileGrid = chunkManager.GetComponent<TileGrid>();
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0)) // 0 = Left click
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            MoveToPosition(new Vector3(mousePos.x, mousePos.y, transform.position.z));
        }
        if (!playerAttributes.IsMoving)
        {
            bool topLeft = Input.GetKey(KeyCode.Q);
            bool topRight = Input.GetKey(KeyCode.E);
            bool bottonLeft = Input.GetKey(KeyCode.Z);
            bool bottonRight = Input.GetKey(KeyCode.C);

            if (topLeft)
            {
                UpdateDesiredPosition(new Vector3(-1, 1, 0));
            }
            else if (topRight)
            {
                UpdateDesiredPosition(new Vector3(1, 1, 0));
            }
            else if (bottonLeft)
            {
                UpdateDesiredPosition(new Vector3(-1, -1, 0));
            }
            else if (bottonRight)
            {
                UpdateDesiredPosition(new Vector3(1, -1, 0));
            }
            else
            {
                bool usingArrowKeys =
                    Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                    Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
                bool dpadPressed =
                    Input.GetKey(KeyCode.JoystickButton6) ||
                    Input.GetKey(KeyCode.JoystickButton7) ||
                    Input.GetKey(KeyCode.JoystickButton8) ||
                    Input.GetKey(KeyCode.JoystickButton9);

                float x = Input.GetAxisRaw("Horizontal");
                float y = Input.GetAxisRaw("Vertical");

                if (Input.GetAxisRaw("DPadHorizontal") != 0 || Input.GetAxisRaw("DPadVertical") != 0)
                {
                    x = Input.GetAxisRaw("DPadHorizontal");
                    y = Input.GetAxisRaw("DPadVertical");
                }

                int xDir = x < 0 ? (int)Math.Floor(x) :
                           x > 0 ? (int)Math.Ceiling(x) : 0;

                int yDir = y < 0 ? (int)Math.Floor(y) :
                           y > 0 ? (int)Math.Ceiling(y) : 0;
                if (xDir != 0 || yDir != 0)
                {
                    playerAttributes.MoveDir = new Vector3(xDir, yDir, 0);

                    var positionDirection = new Vector3(xDir, yDir, 0);
                    if (usingArrowKeys || dpadPressed)
                    {
                        UpdateDesiredPosition(positionDirection);
                        return;
                    }
                    Vector3[] moveAttempts = new Vector3[]
                    {
                       positionDirection ,
                        new Vector3(0, yDir, 0),
                        new Vector3(xDir, 0, 0)
                    };
                    foreach (Vector3 attempt in moveAttempts)
                    {
                        if (UpdateDesiredPosition(attempt))
                        {
                            break;
                        }
                    }
                }
            }
        }
        if (playerAttributes.IsMoving)
        {
            UpdateColliderOffset();
            if (playerAttributes.OnCollision)
            {
                playerAttributes.TargetPos = playerAttributes.PreviousPos;
                playerAttributes.OnCollision = false;
            }
            transform.position = Vector3.MoveTowards(transform.position, playerAttributes.TargetPos, playerAttributes.MoveSpeed * Time.deltaTime);

            if (transform.position == playerAttributes.TargetPos)
            {
                playerAttributes.IsMoving = false;
            }
        }
    }

    bool UpdateDesiredPosition(Vector3 pos)
    {
        if (pos == Vector3.zero) return false;

        Vector3 desiredPos = transform.position + pos * tileSize;

        Collider2D[] targetColliders = Physics2D.OverlapCircleAll(desiredPos, 0.2f);
        bool haveCollision = targetColliders.Any(c => c != null && ShouldCollide(c));

        if (!haveCollision && HasTile(desiredPos))
        {
            playerAttributes.MoveDir = pos;
            playerAttributes.PreviousPos = transform.position;
            playerAttributes.TargetPos = desiredPos;
            playerAttributes.IsMoving = true;
            UpdateColliderOffset();
            // playerAttributes.Collider.enabled = false;
            return true;
        }

        return false;
    }

    void UpdateColliderOffset()
    {
        GetComponent<BoxCollider2D>().offset = new Vector2((playerAttributes.TargetPos.x - transform.position.x) / 2, (playerAttributes.TargetPos.y - transform.position.y) / 2);
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

    void MoveToPosition(Vector3 position)
    {
        var precisePosition = new Vector3(
            (float)Math.Floor(position.x) + 0.5f,
            (float)Math.Floor(position.y) + 0.5f,
            position.z
        );
        print($"{precisePosition}, {transform.position}");
        var path = tileGrid.GetWalkPath(transform.position, precisePosition);
        print($"move to: {string.Join(", ", path)}, size: {path.Count()}");
    }
}
