using UnityEngine;

public class ThrowableTeleportBeacon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Rigidbody rb;

    [Header("Landing")]
    [SerializeField] private LayerMask validGroundLayers;
    [SerializeField] private float minThrowSpeed = 0.75f;
    [SerializeField] private float settleVelocityThreshold = 0.15f;
    [SerializeField] private float raycastHeight = 0.5f;
    [SerializeField] private float raycastDistance = 3.0f;

    [Header("Behavior")]
    [SerializeField] private bool resetToStartAfterTeleport = true;
    [SerializeField] private float teleportYOffset = 0f;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private bool _wasSelected;
    private bool _armedAfterRelease;
    private bool _teleportDone;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    public void HandleSelect()
    {
        _wasSelected = true;
        _armedAfterRelease = false;
        _teleportDone = false;
    }

    public void HandleUnselect()
    {
        if (!_wasSelected || rb == null) return;

        _wasSelected = false;

        if (rb.linearVelocity.magnitude >= minThrowSpeed)
        {
            _armedAfterRelease = true;
        }
    }

    private void Update()
    {
        if (!_armedAfterRelease || _teleportDone || rb == null || playerRoot == null)
            return;

        if (rb.linearVelocity.magnitude > settleVelocityThreshold)
            return;

        Vector3 origin = transform.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, validGroundLayers))
        {
            TeleportPlayer(hit.point);
        }
    }

    private void TeleportPlayer(Vector3 hitPoint)
    {
        _teleportDone = true;
        _armedAfterRelease = false;

        Vector3 target = new Vector3(
            hitPoint.x,
            playerRoot.position.y + teleportYOffset,
            hitPoint.z
        );

        playerRoot.position = target;

        if (resetToStartAfterTeleport)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = _startPosition;
            transform.rotation = _startRotation;
        }
    }
}