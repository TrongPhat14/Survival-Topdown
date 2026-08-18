using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event Action AttackStarted;
    public event Action AttackPressed;
    public event Action AttackReleased;
    public event Action BlastPressed;
    public event Action RepulsePressed;

    private InputActions inputActions;
    private InputAction movementAction;
    private InputAction attackAction;
    private InputAction blastAction;
    private InputAction repulseAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        inputActions = new InputActions();
        movementAction = inputActions.Player.Movement;
        attackAction = inputActions.Player.Attack;
        blastAction = inputActions.Player.Skill1Blast;
        repulseAction = inputActions.Player.Skill2Repulse;
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            return;
        }

        attackAction.started += HandleAttackStarted;
        attackAction.performed += HandleAttackPerformed;
        attackAction.canceled += HandleAttackCanceled;
        blastAction.performed += HandleBlastPerformed;
        repulseAction.performed += HandleRepulsePerformed;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions == null)
        {
            return;
        }

        AttackReleased?.Invoke();
        attackAction.started -= HandleAttackStarted;
        attackAction.performed -= HandleAttackPerformed;
        attackAction.canceled -= HandleAttackCanceled;
        blastAction.performed -= HandleBlastPerformed;
        repulseAction.performed -= HandleRepulsePerformed;
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Vector2 GetMovementInputVector2()
    {
        return movementAction != null
            ? movementAction.ReadValue<Vector2>()
            : Vector2.zero;
    }

    public void DisableGameplayInput()
    {
        if (!enabled)
        {
            return;
        }

        enabled = false;
    }

    private void HandleAttackStarted(InputAction.CallbackContext context)
    {
        AttackStarted?.Invoke();
    }

    private void HandleAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPressed?.Invoke();
    }

    private void HandleAttackCanceled(InputAction.CallbackContext context)
    {
        AttackReleased?.Invoke();
    }

    private void HandleBlastPerformed(InputAction.CallbackContext context)
    {
        BlastPressed?.Invoke();
    }

    private void HandleRepulsePerformed(InputAction.CallbackContext context)
    {
        RepulsePressed?.Invoke();
    }
}
