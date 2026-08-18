using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActorMotor))]
[RequireComponent(typeof(ActorFacing))]
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int DieStateHash = Animator.StringToHash("Base Layer.Die");

    [SerializeField] private Animator animator;

    public static event Action PlayerDied;
    public static bool IsPlayerDead { get; private set; }

    private ActorMotor motor;
    private ActorFacing facing;
    private Health health;
    private bool isDead;
    private bool walkingSoundPlaying;

    private void Awake()
    {
        IsPlayerDead = false;
        motor = GetComponent<ActorMotor>();
        facing = GetComponent<ActorFacing>();
        health = GetComponent<Health>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        health.Died += HandleDied;
    }

    private void OnDisable()
    {
        health.Died -= HandleDied;
        StopWalkingSound();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        Vector2 input = GetMovementInput();
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
        bool isMoving = input.sqrMagnitude > 0.01f;

        motor.SetMoveDirection(moveDirection);
        facing.FaceDirection(moveDirection);
        UpdateWalkingSound(isMoving);

        if (animator != null)
        {
            animator.SetBool(IsRunningHash, isMoving);
        }
    }

    private Vector2 GetMovementInput()
    {
        GameInput source = GameInput.Instance;
        return source != null ? source.GetMovementInputVector2() : Vector2.zero;
    }

    private void HandleDied()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        IsPlayerDead = true;
        motor.SetMoveDirection(Vector3.zero);
        StopWalkingSound();
        PlayerDied?.Invoke();
        GameResultUI.Instance?.Show(GameResult.Defeat);

        if (animator != null)
        {
            animator.SetBool(IsRunningHash, false);
            animator.CrossFadeInFixedTime(DieStateHash, 0.08f);
            StartCoroutine(DestroyAfterDeathAnimation());
            return;
        }

        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDeathAnimation()
    {
        yield return null;

        const float timeout = 5f;
        float elapsed = 0f;
        bool enteredDeathState = false;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (state.fullPathHash == DieStateHash)
            {
                enteredDeathState = true;

                if (!animator.IsInTransition(0) && state.normalizedTime >= 1f)
                {
                    break;
                }
            }
            else if (enteredDeathState)
            {
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void UpdateWalkingSound(bool isMoving)
    {
        if (isMoving)
        {
            if (!walkingSoundPlaying)
            {
                walkingSoundPlaying = SoundManager.StartLoop(SoundId.Walking);
            }

            return;
        }

        StopWalkingSound();
    }

    private void StopWalkingSound()
    {
        if (!walkingSoundPlaying)
        {
            return;
        }

        SoundManager.StopLoop(SoundId.Walking);
        walkingSoundPlaying = false;
    }
}
