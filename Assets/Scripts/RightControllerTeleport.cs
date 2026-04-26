using UnityEngine;

public class RightControllerTeleport : MonoBehaviour
{
    [Header("Ray Settings")]
    public float maxDistance = 10f;
    public LayerMask groundLayer;

    [Header("Teleport")]
    public GameObject teleportRingPrefab;

    private GameObject ringInstance;
    private Transform rigRoot;

    void Start()
    {
        // The OVRCameraRig root should move, not the camera
        rigRoot = GetComponentInParent<OVRCameraRig>().transform;

        ringInstance = Instantiate(teleportRingPrefab);
        ringInstance.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool hitGround = Physics.Raycast(ray, out hit, maxDistance, groundLayer);

        // Update ring position
        if (hitGround)
        {
            ringInstance.SetActive(true);

            ringInstance.transform.position = hit.point;
            ringInstance.transform.up = hit.normal;
        }
        else
        {
            ringInstance.SetActive(false);
        }

        // Teleport on B press
        if (OVRInput.GetDown(OVRInput.Button.Two)) // B button
        {
            if (hitGround)
            {
                TeleportTo(hit.point);
            }
        }
    }

    void TeleportTo(Vector3 targetPosition)
    {
        // Keep height offset so player doesn't sink into ground
        Vector3 offset = rigRoot.position - Camera.main.transform.position;

        rigRoot.position = targetPosition + offset;
    }
}