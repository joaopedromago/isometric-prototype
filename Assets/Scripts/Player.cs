using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Collider2D col;

    public float moveSpeed = 250f;
    public float tileSize = 1f;
    private Vector3 moveDir;
    private Vector3 previousPos;
    private Vector3 targetPos;
    private bool isMoving = false;
    private bool onCollision = false;

    public bool IsMoving => isMoving;


    void Start()
    {
        col = GetComponent<Collider2D>();
        targetPos = transform.position;
    }

    void Update()
    {
        if (!isMoving)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            int xDir = x < 0 ? (int)Math.Floor(x) :
                       x > 0 ? (int)Math.Ceiling(x) : 0;

            int yDir = y < 0 ? (int)Math.Floor(y) :
                       y > 0 ? (int)Math.Ceiling(y) : 0;

            moveDir = new Vector3(xDir, yDir, 0);

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
                bool hasCollisionAtTarget = Physics2D.OverlapCircle(desiredPos, 0.1f) != null;

                if (!hasCollisionAtTarget)
                {
                    moveDir = attempt;
                    previousPos = transform.position;
                    targetPos = desiredPos;
                    isMoving = true;
                    col.enabled = false;
                    break;
                }
            }
        }
        if (isMoving)
        {
            if (onCollision)
            {
                targetPos = previousPos;
                onCollision = false;
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (transform.position == targetPos)
            {
                isMoving = false;
                col.enabled = true;
            }
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        onCollision = true;
    }
}
