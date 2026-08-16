using UnityEngine;

[RequireComponent(typeof(EnemyMotor))]
public class EnemyController : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    [SerializeField] private Transform target;
    [SerializeField, Min(0.05f)] private float repathInterval = 0.2f;
    [SerializeField] private Animator animator;

    private EnemyMotor motor;
    private float repathTimer;

    private void Awake()
    {
        motor = GetComponent<EnemyMotor>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (target != null && !target.IsChildOf(transform))
        {
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void Update()
    {
        if (animator != null)
        {
            animator.SetBool(IsMovingHash, motor.IsMoving);
        }

        if (target == null)
        {
            motor.Stop();
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f)
        {
            return;
        }

        repathTimer = repathInterval;
        motor.SetDestination(target.position);
    }
}
