using UnityEngine;

[RequireComponent(typeof(ActorMotor))]
[RequireComponent(typeof(ActorFacing))]
public class PlayerController : MonoBehaviour
{
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

    [SerializeField] private Animator animator;

    private ActorMotor motor;
    private ActorFacing facing;

    private void Awake()
    {
        motor = GetComponent<ActorMotor>();
        facing = GetComponent<ActorFacing>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        Vector2 input = GetMovementInput();
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        motor.SetMoveDirection(moveDirection);
        facing.FaceDirection(moveDirection);

        if (animator != null)
        {
            animator.SetBool(IsRunningHash, input.sqrMagnitude > 0.01f);
        }
    }

    private Vector2 GetMovementInput()
    {
        GameInput source = GameInput.Instance;
        return source != null ? source.GetMovementInputVector2() : Vector2.zero;
    }
}
