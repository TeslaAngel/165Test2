using UnityEngine;
using UnityEngine.AI;

public class EmbodiedAgentNavigator : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform target;
    [SerializeField] private float sampleRadius = 2.0f;

    // gesture system call agentNavigator.MoveToWorldPosition(gestureSelectedPoint);

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    public void MoveToWorldPosition(Vector3 worldPosition)
    {
        if (agent == null)
        {
            Debug.LogError("[EmbodiedAgentNavigator] Missing NavMeshAgent.");
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning("[EmbodiedAgentNavigator] Agent is not ready or not on NavMesh yet.");
            return;
        }

        if (NavMesh.SamplePosition(worldPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("[EmbodiedAgentNavigator] Target is not near the NavMesh.");
        }
    }

    public void MoveToTarget()
    {
        if (target == null)
        {
            return;
        }

        MoveToWorldPosition(target.position);
    }

    private void Update()
    {
        // Optional test behavior.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveToTarget();
        }
    }
}