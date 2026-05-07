using UnityEngine;

public class TeleportBallSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private GameObject teleportBallPrefab;

    [Header("Input")]
    [SerializeField] private float gripThreshold = 0.6f;

    [Header("Throw")]
    [SerializeField] private float throwVelocityMultiplier = 1.4f;

    private GameObject currentBall;
    private Rigidbody currentRb;

    private bool wasHoldingBall;

    private Vector3 lastHandPosition;
    private Vector3 handVelocity;

    private void Start()
    {
        if (rightHandAnchor != null)
            lastHandPosition = rightHandAnchor.position;
    }

    private void Update()
    {
        if (rightHandAnchor == null || teleportBallPrefab == null)
            return;

        UpdateHandVelocity();

        bool bPressed = OVRInput.Get(OVRInput.Button.Two); // B button (right controller)

        bool rightGripHeld =
            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > gripThreshold;

        bool shouldHoldBall = bPressed && rightGripHeld;

        if (shouldHoldBall && currentBall == null)
        {
            SpawnBall();
        }

        if (currentBall != null && rightGripHeld)
        {
            HoldBallAtHand();
            wasHoldingBall = true;
        }

        if (currentBall != null && wasHoldingBall && !rightGripHeld)
        {
            ReleaseBall();
        }
    }

    private void UpdateHandVelocity()
    {
        Vector3 currentHandPosition = rightHandAnchor.position;
        handVelocity = (currentHandPosition - lastHandPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastHandPosition = currentHandPosition;
    }

    private void SpawnBall()
    {
        currentBall = Instantiate(
            teleportBallPrefab,
            rightHandAnchor.position,
            rightHandAnchor.rotation
        );

        TeleportBallProjectile teleportBall = currentBall.GetComponent<TeleportBallProjectile>();
        if (teleportBall != null)
        {
            teleportBall.SetPlayerRoot(playerRoot);
        }

        currentRb = currentBall.GetComponent<Rigidbody>();

        if (currentRb != null)
        {
            currentRb.isKinematic = true;
            currentRb.useGravity = false;
            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
        }

        currentBall.transform.SetParent(rightHandAnchor, true);
        currentBall.transform.localPosition = Vector3.zero;
        currentBall.transform.localRotation = Quaternion.identity;

        wasHoldingBall = true;
    }

    private void HoldBallAtHand()
    {
        currentBall.transform.position = rightHandAnchor.position;
        currentBall.transform.rotation = rightHandAnchor.rotation;
    }

    private void ReleaseBall()
    {
        currentBall.transform.SetParent(null, true);

        if (currentRb != null)
        {
            currentRb.isKinematic = false;
            currentRb.useGravity = true;
            currentRb.linearVelocity = handVelocity * throwVelocityMultiplier;
            currentRb.angularVelocity = Vector3.zero;
        }

        currentBall = null;
        currentRb = null;
        wasHoldingBall = false;
    }
}