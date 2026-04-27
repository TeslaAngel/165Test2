using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VR3DSpawnButton : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private Vector3 localSpawnOffset = Vector3.zero;

    [Header("Visual")]
    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.green;
    [SerializeField] private Color pressedColor = Color.yellow;

    private bool isHovered;

    private void Awake()
    {
        if (buttonRenderer == null)
            buttonRenderer = GetComponentInChildren<Renderer>();

        ApplyColor(normalColor);
    }

    public void HoverEnter()
    {
        isHovered = true;
        ApplyColor(hoverColor);
    }

    public void HoverExit()
    {
        isHovered = false;
        ApplyColor(normalColor);
    }

    public void Press()
    {
        ApplyColor(pressedColor);
        SpawnPrefab();
    }

    public void Release()
    {
        ApplyColor(isHovered ? hoverColor : normalColor);
    }

    private void SpawnPrefab()
    {
        if (prefabToSpawn == null || rightHandAnchor == null)
        {
            Debug.LogWarning($"{name}: missing prefabToSpawn or rightHandAnchor.");
            return;
        }

        Vector3 spawnPos = rightHandAnchor.TransformPoint(localSpawnOffset);
        Quaternion spawnRot = rightHandAnchor.rotation;

        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, spawnRot);

        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void ApplyColor(Color c)
    {
        if (buttonRenderer == null) return;

        foreach (Material mat in buttonRenderer.materials)
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", c);
            else if (mat.HasProperty("_Color"))
                mat.color = c;
        }
    }
}