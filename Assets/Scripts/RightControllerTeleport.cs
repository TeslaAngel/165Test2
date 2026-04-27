using UnityEngine;

public class RightControllerTeleport : MonoBehaviour
{
    [Header("Ray Settings")]
    public float maxDistance = 10f;
    public LayerMask groundLayer;
    public float heightOffset = 0.1f;

    [Header("Teleport")]
    public GameObject teleportRingPrefab;
    [SerializeField] private float playerYOffset = 0f;
    [SerializeField] private bool preservePlayerY = true;

    [Header("References")]
    [SerializeField] private Transform playerRoot;

    private GameObject ringInstance;

    public void SetPlayerRoot(Transform root)
    {
        playerRoot = root;
    }
    void Start()
    {
        ringInstance = Instantiate(teleportRingPrefab);
        ringInstance.SetActive(false);
    }
    void TeleportTo(Vector3 contactPoint)
    {
        float targetY = preservePlayerY
            ? playerRoot.position.y
            : contactPoint.y + playerYOffset;

        playerRoot.position = new Vector3(
            contactPoint.x,
            targetY,
            contactPoint.z
        );
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool hasHit = Physics.Raycast(ray, out hit, maxDistance);

        bool hitGround = hasHit &&
            ((1 << hit.collider.gameObject.layer) & groundLayer) != 0;

        // Update ring position
        if (hitGround)
        {
            ringInstance.SetActive(true);

            Vector3 adjustedPosition = hit.point + Vector3.up * heightOffset;

            ringInstance.transform.position = adjustedPosition;

            ringInstance.transform.rotation = Quaternion.LookRotation(Vector3.up);
        }
        else
        {
            ringInstance.SetActive(false);
        }

        // Teleport on B press
        if (OVRInput.GetDown(OVRInput.Button.Two) && hitGround) // B button
        {
            TeleportTo(hit.point);
        }
    }
}