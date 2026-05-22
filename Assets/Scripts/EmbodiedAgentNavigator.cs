using UnityEngine;
using UnityEngine.AI;

public class EmbodiedAgentNavigator : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent agent;
    public Transform target;
    public float sampleRadius = 2.0f;

    [Header("Animation")]
    public Animator anim;
    public string walkingBoolName = "Walking";
    public float walkingVelocityThreshold = 0.05f;

    [Header("Behavior")]
    public bool followTargetContinuously = true;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        UpdateTargetFollowing();
        UpdateAnimation();
    }

    private void UpdateTargetFollowing()
    {
        if (!followTargetContinuously)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        if (!IsAgentReady())
        {
            return;
        }

        agent.SetDestination(target.position);
    }

    private void UpdateAnimation()
    {
        if (anim == null || agent == null)
        {
            return;
        }

        bool isWalking = agent.enabled &&
                         agent.isOnNavMesh &&
                         agent.velocity.magnitude > walkingVelocityThreshold;

        anim.SetBool(walkingBoolName, isWalking);
    }

    public void MoveToWorldPosition(Vector3 worldPosition)
    {
        if (!IsAgentReady())
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

    public void MoveToTarget()
    {
        if (target == null)
        {
            Debug.LogWarning("[EmbodiedAgentNavigator] No target assigned.");
            return;
        }

        MoveToWorldPosition(target.position);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void StopMoving()
    {
        if (!IsAgentReady())
        {
            return;
        }

        agent.ResetPath();
        UpdateAnimation();
    }

    private bool IsAgentReady()
    {
        return agent != null &&
               agent.enabled &&
               agent.isOnNavMesh;
    }
}