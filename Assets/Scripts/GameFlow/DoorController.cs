using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Collider blockingCollider;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField, Min(0f)] private float openDuration = 1f;

    private Coroutine openRoutine;
    private int openTriggerHash;

    public event Action Opened;

    public bool IsOpen { get; private set; }
    public bool IsOpening => openRoutine != null;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (blockingCollider == null)
        {
            blockingCollider = GetComponentInChildren<Collider>();
        }

        openTriggerHash = string.IsNullOrWhiteSpace(openTriggerName)
            ? 0
            : Animator.StringToHash(openTriggerName);
    }

    public void Open()
    {
        if (IsOpen || IsOpening)
        {
            return;
        }

        if (animator != null && HasOpenTrigger())
        {
            animator.SetTrigger(openTriggerHash);
        }
        else if (animator != null)
        {
            Debug.LogWarning(
                $"Animator does not contain the '{openTriggerName}' trigger.",
                this);
        }

        if (openDuration <= 0f)
        {
            CompleteOpen();
            return;
        }

        openRoutine = StartCoroutine(CompleteAfterDelay());
    }

    public void NotifyOpenComplete()
    {
        if (IsOpen)
        {
            return;
        }

        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        CompleteOpen();
    }

    private IEnumerator CompleteAfterDelay()
    {
        yield return new WaitForSeconds(openDuration);
        openRoutine = null;
        CompleteOpen();
    }

    private void CompleteOpen()
    {
        if (IsOpen)
        {
            return;
        }

        IsOpen = true;

        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        Opened?.Invoke();
    }

    private bool HasOpenTrigger()
    {
        if (string.IsNullOrWhiteSpace(openTriggerName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == openTriggerHash &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }
}
