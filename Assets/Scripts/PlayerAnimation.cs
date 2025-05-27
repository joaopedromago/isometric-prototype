using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public bool? lastState = null;
    private PlayerAttributes playerAttributes;

    void Awake()
    {
        playerAttributes = GetComponent<PlayerAttributes>();
    }

    void Update()
    {
        HandleWalkingAnimation();
    }

    void HandleWalkingAnimation()
    {
        if (playerAttributes.IsMoving == lastState) return;
        lastState = playerAttributes.IsMoving;

        Animator animator = GetComponent<Animator>();

        if (playerAttributes.IsMoving)
        {
            animator.Play("PlayerFrontWalk");
        }
        else
        {
            animator.Play("Idle");
        }
    }
}
