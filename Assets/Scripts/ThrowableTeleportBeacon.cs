using UnityEngine;

public class ThrowableTeleportBeacon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;      // OVRCameraRig root
    [SerializeField] private Transform beaconVisual;    // VisualRoot child
    [SerializeField] private Rigidbody rb;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask validGroundLayers;
    [SerializeField] private float raycastHeight = 0.5f;
    [SerializeField] private float raycastDistance = 3.0f;

    [Header("Throw Detection")]
    [SerializeField] private float minThrowSpeed = 0.75f;
    [SerializeField] private float settleSpeed = 0.2f;
    [SerializeField] private float settleDelay = 0.25f;

    [Header("Behavior")]
    [SerializeField] private bool resetBeaconAfterTeleport = true;
    [SerializeField] private float playerHeightOffset = 0f;

    private Vector3 startRootPosition;
    private Quaternion startRootRotation;
    private Vector3 startVisualLocalPosition;
    private Quaternion startVisualLocalRotation;

    private bool isSelected;
    private bool armed;
    private bool teleported;
    private float settleTimer;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (beaconVisual == null)
            beaconVisual = transform;

        startRootPosition = transform.position;
        startRootRotation = transform.rotation;
        startVisualLocalPosition = beaconVisual.localPosition;
        startVisualLocalRotation = beaconVisual.localRotation;
    }

    public void HandleSelect()
    {
        isSelected = true;
        armed = false;
        teleported = false;
        settleTimer = 0f;
    }

    public void HandleUnselect()
    {
        isSelected = false;

        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude;

        if (speed >= minThrowSpeed)
        {
            armed = true;
            teleported = false;
            settleTimer = 0f;
        }
    }

    private void Update()
    {
        if (!armed || teleported || rb == null || playerRoot == null)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (speed <= settleSpeed)
            settleTimer += Time.deltaTime;
        else
            settleTimer = 0f;

        if (settleTimer < settleDelay)
            return;

        Vector3 rayOrigin = beaconVisual.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            validGroundLayers
        ))
        {
            TeleportPlayer(hit.point);
        }
    }

    private void TeleportPlayer(Vector3 groundPoint)
    {
        teleported = true;
        armed = false;

        Vector3 newPlayerPosition = new Vector3(
            groundPoint.x,
            playerRoot.position.y + playerHeightOffset,
            groundPoint.z
        );

        playerRoot.position = newPlayerPosition;

        if (resetBeaconAfterTeleport)
            ResetBeacon();
    }

    private void ResetBeacon()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = startRootPosition;
        transform.rotation = startRootRotation;

        beaconVisual.localPosition = startVisualLocalPosition;
        beaconVisual.localRotation = startVisualLocalRotation;
    }
}