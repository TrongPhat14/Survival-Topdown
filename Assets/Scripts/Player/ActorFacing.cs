using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActorFacing : MonoBehaviour
{
    [SerializeField] private float turnSpeedDegrees = 180f;

    private Rigidbody rb;
    private Vector3 facingDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        facingDirection = direction.normalized;
    }

    private void FixedUpdate()
    {
        if (facingDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);

        rb.angularVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            turnSpeedDegrees * Time.fixedDeltaTime));
    }
}
