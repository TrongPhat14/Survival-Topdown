using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputActions inputActions;
    private InputAction movementAction;

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
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
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
}
