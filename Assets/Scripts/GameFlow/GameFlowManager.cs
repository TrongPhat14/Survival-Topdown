using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GameFlowManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private WaveController waveController;
    [SerializeField] private PlayerController player;
    [SerializeField] private DoorController exitDoor;
    [SerializeField] private Transform doorWaitPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameResultUI resultUI;

    [Header("Victory Sequence")]
    [SerializeField, Min(0.05f)] private float arrivalDistance = 0.25f;
    [SerializeField, Min(0f)] private float victoryDelay = 0.4f;
    [SerializeField, Min(0.1f)] private float doorTimeout = 5f;
    [SerializeField, Min(0.1f)] private float movementTimeout = 12f;

    private Coroutine victoryRoutine;
    private bool victoryStarted;

    private void Awake()
    {
        if (waveController == null)
        {
            waveController = FindFirstObjectByType<WaveController>();
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (resultUI == null)
        {
            resultUI = GameResultUI.Instance;
        }
    }

    private void OnEnable()
    {
        if (waveController != null)
        {
            waveController.AllWavesCompleted += HandleAllWavesCompleted;
        }

        PlayerController.PlayerDied += HandlePlayerDied;
    }

    private void Start()
    {
        if (waveController == null)
        {
            Debug.LogError("GameFlowManager is missing WaveController.", this);
        }

        if (player == null ||
            exitDoor == null ||
            doorWaitPoint == null ||
            exitPoint == null)
        {
            Debug.LogWarning(
                "Victory sequence is missing Player, Exit Door, Door Wait Point, or Exit Point. " +
                "Victory UI will use its fallback flow.",
                this);
        }
    }

    private void OnDisable()
    {
        if (waveController != null)
        {
            waveController.AllWavesCompleted -= HandleAllWavesCompleted;
        }

        PlayerController.PlayerDied -= HandlePlayerDied;

        if (victoryRoutine != null)
        {
            StopCoroutine(victoryRoutine);
            victoryRoutine = null;
        }

        if (player != null)
        {
            player.StopAutoMove();
        }
    }

    private void HandleAllWavesCompleted()
    {
        if (victoryStarted || PlayerController.IsPlayerDead)
        {
            return;
        }

        victoryStarted = true;
        victoryRoutine = StartCoroutine(RunVictorySequence());
    }

    private IEnumerator RunVictorySequence()
    {
        while (Time.timeScale <= 0f && !PlayerController.IsPlayerDead)
        {
            yield return null;
        }

        if (PlayerController.IsPlayerDead)
        {
            yield break;
        }

        GameInput.Instance?.DisableGameplayInput();

        if (exitDoor != null)
        {
            exitDoor.Open();
        }

        if (player != null &&
            doorWaitPoint != null &&
            !PlayerController.IsPlayerDead)
        {
            yield return MovePlayerTo(doorWaitPoint, "door wait point");
        }

        if (exitDoor != null)
        {
            float elapsed = 0f;
            while (!exitDoor.IsOpen &&
                   elapsed < doorTimeout &&
                   !PlayerController.IsPlayerDead)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!exitDoor.IsOpen && !PlayerController.IsPlayerDead)
            {
                Debug.LogWarning(
                    "Door opening timed out. Completing the open state to avoid a soft lock.",
                    exitDoor);
                exitDoor.NotifyOpenComplete();
            }
        }

        if (player != null &&
            exitPoint != null &&
            !PlayerController.IsPlayerDead)
        {
            yield return MovePlayerTo(exitPoint, "exit point");
        }

        if (PlayerController.IsPlayerDead)
        {
            yield break;
        }

        if (victoryDelay > 0f)
        {
            yield return new WaitForSeconds(victoryDelay);
        }

        victoryRoutine = null;
        GameResultUI victoryUI = ResolveResultUI();

        if (victoryUI != null)
        {
            victoryUI.Show(GameResult.Victory);
        }
        else
        {
            Debug.LogError("GameFlowManager could not find GameResultUI.", this);
        }
    }

    private IEnumerator MovePlayerTo(Transform target, string targetName)
    {
        if (player == null ||
            target == null ||
            !player.BeginAutoMove(target, arrivalDistance))
        {
            yield break;
        }

        float elapsed = 0f;
        while (player.IsAutoMoving &&
               elapsed < movementTimeout &&
               !PlayerController.IsPlayerDead)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (player.IsAutoMoving && !PlayerController.IsPlayerDead)
        {
            Debug.LogWarning(
                $"Player auto movement timed out before reaching the {targetName}.",
                player);
        }

        player.StopAutoMove();
    }

    private void HandlePlayerDied()
    {
        if (victoryRoutine != null)
        {
            StopCoroutine(victoryRoutine);
            victoryRoutine = null;
        }

        if (player != null)
        {
            player.StopAutoMove();
        }
    }

    private GameResultUI ResolveResultUI()
    {
        if (resultUI == null)
        {
            resultUI = GameResultUI.Instance;
        }

        return resultUI;
    }
}
