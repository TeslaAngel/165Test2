using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshTargetUtility
{
    public static bool TryProjectToNavMesh(
        Vector3 rawWorldPoint,
        float maxDistance,
        out Vector3 navMeshPoint)
    {
        if (NavMesh.SamplePosition(rawWorldPoint, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            navMeshPoint = hit.position;
            return true;
        }

        navMeshPoint = rawWorldPoint;
        return false;
    }
}

public class MRUKNavMeshBuilder : MonoBehaviour
{
    [Header("Generated Geometry")]
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private bool showDebugGeometry = true;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private float wallObstacleHeight = 2.2f;
    [SerializeField] private float wallObstacleThickness = 0.08f;
    [SerializeField] private float navMeshBuildDelay = 0.5f;

    [Header("Agent Spawn")]
    [SerializeField] private GameObject agentPrefab;
    private GameObject spawnedAgent;
    //[SerializeField] private Transform agentToSpawnOrWarp;
    [SerializeField] private float spawnSampleRadius = 2.0f;

    private readonly List<GameObject> generatedObjects = new();

    private IEnumerator Start()
    {
        if (navMeshSurface == null)
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
        }

        if (navMeshSurface == null)
        {
            Debug.LogError("[MRUKNavMeshBuilder] Missing NavMeshSurface.");
            yield break;
        }

        // Give MRUK time to load scene data.
        yield return new WaitForSeconds(navMeshBuildDelay);

        MRUKRoom room = WaitForCurrentRoom();

        if (room == null)
        {
            Debug.LogError("[MRUKNavMeshBuilder] No MRUK room found. Make sure Space Setup is completed on Quest and MRUK is in the scene.");
            yield break;
        }

        ClearGeneratedGeometry();

        BuildFloorFromMRUK(room);
        BuildWallsFromMRUK(room);

        navMeshSurface.BuildNavMesh();
        SpawnAgentOnNavMesh(room);

        Debug.Log("[MRUKNavMeshBuilder] Runtime NavMesh built from MRUK scene data.");

        if (spawnedAgent != null)
        {
            PlaceAgentOnNavMesh(room);
        }
    }

    private MRUKRoom WaitForCurrentRoom()
    {
        if (MRUK.Instance == null)
        {
            Debug.LogError("[MRUKNavMeshBuilder] MRUK.Instance is null.");
            return null;
        }

        return MRUK.Instance.GetCurrentRoom();
    }

    private void BuildFloorFromMRUK(MRUKRoom room)
    {
        MRUKAnchor floorAnchor = room.FloorAnchor;

        if (floorAnchor == null)
        {
            Debug.LogError("[MRUKNavMeshBuilder] No floor anchor found.");
            return;
        }

        GameObject floorObject = CreatePlaneFromAnchor(
            floorAnchor,
            "Generated_MRUK_Floor",
            floorMaterial,
            isWalkable: true
        );

        if (floorObject != null)
        {
            generatedObjects.Add(floorObject);
        }
    }

    private void BuildWallsFromMRUK(MRUKRoom room)
    {
        if (room.WallAnchors == null || room.WallAnchors.Count == 0)
        {
            Debug.LogWarning("[MRUKNavMeshBuilder] No wall anchors found.");
            return;
        }

        foreach (MRUKAnchor wallAnchor in room.WallAnchors)
        {
            GameObject wallObject = CreateWallObstacleFromAnchor(wallAnchor);

            if (wallObject != null)
            {
                generatedObjects.Add(wallObject);
            }
        }
    }

    private GameObject CreatePlaneFromAnchor(
        MRUKAnchor anchor,
        string objectName,
        Material material,
        bool isWalkable)
    {
        if (anchor.PlaneRect == null)
        {
            Debug.LogWarning($"[MRUKNavMeshBuilder] {objectName} has no PlaneRect.");
            return null;
        }

        Rect rect = anchor.PlaneRect.Value;

        GameObject plane = new GameObject(objectName);
        plane.transform.SetParent(transform, false);

        // MRUK anchor transform already represents the real-world plane pose.
        plane.transform.position = anchor.transform.position;
        plane.transform.rotation = anchor.transform.rotation;

        Mesh mesh = BuildRectMesh(rect);
        MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshCollider meshCollider = plane.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();
        meshRenderer.enabled = showDebugGeometry;

        if (material != null)
        {
            meshRenderer.sharedMaterial = material;
        }

        // Mark generated geometry for NavMesh collection.
        plane.layer = gameObject.layer;

        if (isWalkable)
        {
            plane.AddComponent<NavMeshModifier>().overrideArea = false;
        }

        return plane;
    }

    private Mesh BuildRectMesh(Rect rect)
    {
        float xMin = rect.xMin;
        float xMax = rect.xMax;
        float yMin = rect.yMin;
        float yMax = rect.yMax;

        // MRUK PlaneRect is local 2D plane space.
        // For floor/ceiling/wall anchors, this maps into the anchor's local X/Y plane.
        Vector3[] vertices =
        {
            new Vector3(xMin, yMin, 0f),
            new Vector3(xMax, yMin, 0f),
            new Vector3(xMax, yMax, 0f),
            new Vector3(xMin, yMax, 0f)
        };

        int[] triangles =
        {
            0, 1, 2,
            0, 2, 3
        };

        Vector2[] uvs =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        Mesh mesh = new Mesh();
        mesh.name = "MRUK_RectMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private GameObject CreateWallObstacleFromAnchor(MRUKAnchor wallAnchor)
    {
        Vector3 center = wallAnchor.GetAnchorCenter();
        Vector3 size = wallAnchor.GetAnchorSize();

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Generated_MRUK_Wall_Obstacle";
        wall.transform.SetParent(transform, false);

        wall.transform.position = center;
        wall.transform.rotation = wallAnchor.transform.rotation;

        // For NavMesh, the exact visible wall mesh is less important than giving the bake
        // a physical obstacle volume.
        float width = Mathf.Max(size.x, 0.2f);
        float height = Mathf.Max(size.y, wallObstacleHeight);

        wall.transform.localScale = new Vector3(
            width,
            height,
            wallObstacleThickness
        );

        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        renderer.enabled = showDebugGeometry;

        if (wallMaterial != null)
        {
            renderer.sharedMaterial = wallMaterial;
        }

        return wall;
    }

    private void SpawnAgentOnNavMesh(MRUKRoom room)
    {
        Vector3 desiredPosition = room.FloorAnchor != null
            ? room.FloorAnchor.GetAnchorCenter()
            : transform.position;

        if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, spawnSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning("[MRUKNavMeshBuilder] Could not find spawn point on NavMesh.");
            return;
        }

        spawnedAgent = Instantiate(agentPrefab, hit.position, Quaternion.identity);

        NavMeshAgent agent = spawnedAgent.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
        {
            Debug.Log("[MRUKNavMeshBuilder] Spawned agent on generated NavMesh.");
        }
    }

    private void PlaceAgentOnNavMesh(MRUKRoom room)
    {
        Vector3 desiredPosition = room.FloorAnchor != null
            ? room.FloorAnchor.GetAnchorCenter()
            : transform.position;

        if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, spawnSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning("[MRUKNavMeshBuilder] Could not sample a valid NavMesh position for agent.");
            return;
        }

        NavMeshAgent agent = spawnedAgent.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = false;

            spawnedAgent.transform.position = hit.position;

            agent.enabled = true;

            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("[MRUKNavMeshBuilder] Agent enabled but still not on NavMesh.");
                return;
            }

            agent.Warp(hit.position);
        }
        else
        {
            spawnedAgent.transform.position = hit.position;
        }

        Debug.Log("[MRUKNavMeshBuilder] Agent placed on generated NavMesh.");
    }

    public void RebuildNavMesh()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("[MRUKNavMeshBuilder] Cannot rebuild NavMesh because NavMeshSurface is missing.");
            return;
        }

        navMeshSurface.BuildNavMesh();

        Debug.Log("[MRUKNavMeshBuilder] NavMesh rebuilt.");
    }

    private void ClearGeneratedGeometry()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        generatedObjects.Clear();
    }
}