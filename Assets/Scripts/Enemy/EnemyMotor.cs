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

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
}
