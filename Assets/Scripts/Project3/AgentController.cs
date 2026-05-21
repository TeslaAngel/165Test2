using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class AgentController : MonoBehaviour
{
    public NavMeshSurface surface;
    public NavMeshAgent agent;
    public Animator anim;
    public Transform target;

    public void RebakeAfterAnchorAdded()
  {
    surface.BuildNavMesh();
  }
    void Update()
    {
        if (target != null && agent.isOnNavMesh) agent.SetDestination(target.position);
        anim.SetBool("Walking", agent.velocity.magnitude > 0.05f);
    }
}
