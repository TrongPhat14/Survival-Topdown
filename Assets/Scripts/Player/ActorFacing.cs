using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActorFacing : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float movementTurnSmoothTime = 0.15f;

    private Rigidbody rb;
    private Vector3 facingDirection;
    private float turnVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FaceMovementDirection(Vector3 direction)
    {
        SetFacingDirection(direction);
    }

    public void FaceAttackDirection(Vector3 direction)
    {
        if (!SetFacingDirection(direction))
        {
            return;
        }

        turnVelocity = 0f;
        rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.LookRotation(facingDirection, Vector3.up);
    }

    private bool SetFacingDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return false;
        }

        facingDirection = direction.normalized;
        return true;
    }

    private void FixedUpdate()
    {
        if (facingDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        float targetYaw = Mathf.Atan2(
            facingDirection.x,
            facingDirection.z) * Mathf.Rad2Deg;
        float smoothedYaw = Mathf.SmoothDampAngle(
            rb.rotation.eulerAngles.y,
            targetYaw,
            ref turnVelocity,
            movementTurnSmoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime);

        rb.angularVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.Euler(0f, smoothedYaw, 0f));
    }
}
