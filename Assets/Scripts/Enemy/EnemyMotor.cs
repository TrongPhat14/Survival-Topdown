using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMotor : MonoBehaviour
{
    private NavMeshAgent agent;

    public bool IsMoving =>
        agent != null &&
        agent.enabled &&
        agent.isOnNavMesh &&
        agent.velocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Configure(float moveSpeed, float attackRange)
    {
        if (agent != null)
        {
            agent.speed = Mathf.Max(0f, moveSpeed);
            agent.stoppingDistance = Mathf.Max(0f, attackRange * 0.9f);
        }
    }

    public bool SetDestination(Vector3 destination)
    {
        if (!CanUseAgent())
        {
            return false;
        }

        agent.isStopped = false;
        return agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (!CanUseAgent())
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void DisableAgent()
    {
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    public void EnableAgent()
    {
        if (agent == null)
        {
            return;
        }

        agent.enabled = true;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
}
