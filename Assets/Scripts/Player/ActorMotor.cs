using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActorMotor : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float moveSpeedMultiplier = 1f;

    public float MoveSpeed => moveSpeed * moveSpeedMultiplier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 velocity = moveDirection * MoveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    public void SetMoveDirection(Vector3 direction)
    {
        direction.y = 0f;
        moveDirection = direction.sqrMagnitude > 1f ? direction.normalized : direction;
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    public void AddMoveSpeedMultiplier(float amount)
    {
        moveSpeedMultiplier = Mathf.Max(0.1f, moveSpeedMultiplier + amount);
    }
}
