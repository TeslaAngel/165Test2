using UnityEngine;
using UnityEngine.AI;

public class EmbodiedAgentNavigator : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float sampleRadius = 2.0f;

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
            Debug.LogWarning("[EmbodiedAgentNavigator] Agent is not ready or not on NavMesh.");
            return;
        }

        if (NavMesh.SamplePosition(worldPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);

            Debug.Log($"[EmbodiedAgentNavigator] Moving agent to {hit.position}");
        }
        else
        {
            Debug.LogWarning("[EmbodiedAgentNavigator] Target is not close enough to the NavMesh.");
        }
    }
}