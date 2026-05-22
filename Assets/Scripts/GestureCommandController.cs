using UnityEngine;
using UnityEngine.AI;

public class GestureCommandController : MonoBehaviour
{
    public enum CommandMode
    {
        MoveAgent,
        PlaceObstacle
    }

    [Header("Current Mode")]
    [SerializeField] private CommandMode currentMode = CommandMode.MoveAgent;

    [Header("Gesture Ray")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask floorRaycastMask = ~0;

    [Header("Agent")]
    public EmbodiedAgentNavigator agentNavigator;

    [Header("Obstacle Placement")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private MRUKNavMeshBuilder navMeshBuilder;
    [SerializeField] private float obstacleYOffset = 0.05f;
    [SerializeField] private float navMeshSampleRadius = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    private bool pinchHeld;
    private bool fistHeld;

    public CommandMode CurrentMode => currentMode;

    private void Update()
    {
        if (drawDebugRay && rayOrigin != null)
        {
            Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance, Color.green);
        }
    }

    // Call this from your fist gesture event.
    public void OnFistStarted()
    {
        Debug.Log("[GestureCommandController] Fist started.");
        if (fistHeld)
        {
            return;
        }

        fistHeld = true;
        SwitchMode();
    }

    // Call this from your fist released event.
    public void OnFistEnded()
    {
        Debug.Log("[GestureCommandController] Fist ended.");
        fistHeld = false;
    }

    // Call this from your pinch gesture started event.
    public void OnPinchStarted()
    {
        Debug.Log("[GestureCommandController] Pinch started.");
        if (pinchHeld)
        {
            return;
        }

        pinchHeld = true;
        ExecutePinchCommand();
    }

    // Call this from your pinch released event.
    public void OnPinchEnded()
    {
        Debug.Log("[GestureCommandController] Pinch ended.");
        pinchHeld = false;
    }

    private void SwitchMode()
    {
        if (currentMode == CommandMode.MoveAgent)
        {
            currentMode = CommandMode.PlaceObstacle;
        }
        else
        {
            currentMode = CommandMode.MoveAgent;
        }

        Debug.Log($"[GestureCommandController] Switched mode to: {currentMode}");
    }

    private void ExecutePinchCommand()
    {
        if (rayOrigin == null)
        {
            Debug.LogError("[GestureCommandController] Missing ray origin.");
            return;
        }

        if (!TryGetFloorPointFromRay(out Vector3 hitPoint))
        {
            Debug.LogWarning("[GestureCommandController] Pinch ray did not hit a valid floor target.");
            return;
        }

        switch (currentMode)
        {
            case CommandMode.MoveAgent:
                CommandMoveAgent(hitPoint);
                break;

            case CommandMode.PlaceObstacle:
                CommandPlaceObstacle(hitPoint);
                break;
        }
    }

    private bool TryGetFloorPointFromRay(out Vector3 hitPoint)
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, floorRaycastMask))
        {
            hitPoint = hit.point;
            return true;
        }

        hitPoint = Vector3.zero;
        return false;
    }

    private void CommandMoveAgent(Vector3 rawPoint)
    {
        if (agentNavigator == null)
        {
            Debug.LogError("[GestureCommandController] Missing agent navigator.");
            return;
        }

        if (NavMesh.SamplePosition(rawPoint, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            agentNavigator.MoveToWorldPosition(navHit.position);
        }
        else
        {
            Debug.LogWarning("[GestureCommandController] Move target is not on the NavMesh.");
        }
    }

    private void CommandPlaceObstacle(Vector3 rawPoint)
    {
        if (obstaclePrefab == null)
        {
            Debug.LogError("[GestureCommandController] Missing obstacle prefab.");
            return;
        }

        if (navMeshBuilder == null)
        {
            Debug.LogError("[GestureCommandController] Missing MRUKNavMeshBuilder.");
            return;
        }

        if (!NavMesh.SamplePosition(rawPoint, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning("[GestureCommandController] Obstacle target is not close enough to the NavMesh.");
            return;
        }

        Vector3 spawnPosition = navHit.position + Vector3.up * obstacleYOffset;

        GameObject obstacle = Instantiate(
            obstaclePrefab,
            spawnPosition,
            Quaternion.identity,
            obstacleParent != null ? obstacleParent : navMeshBuilder.transform
        );

        EnsureObstacleHasCollider(obstacle);

        navMeshBuilder.RebuildNavMesh();

        Debug.Log($"[GestureCommandController] Placed obstacle at {spawnPosition} and rebuilt NavMesh.");
    }

    private void EnsureObstacleHasCollider(GameObject obstacle)
    {
        Collider existingCollider = obstacle.GetComponentInChildren<Collider>();

        if (existingCollider != null)
        {
            return;
        }

        BoxCollider boxCollider = obstacle.AddComponent<BoxCollider>();
        boxCollider.size = Vector3.one;

        Debug.LogWarning("[GestureCommandController] Obstacle prefab had no collider, so a BoxCollider was added automatically.");
    }
}