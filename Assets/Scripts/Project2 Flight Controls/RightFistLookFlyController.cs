using UnityEngine;

public class RightFistLookFlyController : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The XR Origin / player rig root. This is the object that moves.")]
    [SerializeField] private Transform playerRig;

    [Tooltip("Main Camera / Center Eye Camera. This decides the flying direction.")]
    [SerializeField] private Transform headCamera;

    [Header("Movement")]
    [SerializeField] private float flySpeed = 3.0f;

    [Tooltip("If true, looking up/down also moves vertically. If false, movement stays horizontal.")]
    [SerializeField] private bool allowVerticalFlight = true;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    private bool rightFistHeld;

    private void Awake()
    {
        if (playerRig == null)
            playerRig = transform;

        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (!rightFistHeld)
            return;

        if (headCamera == null || playerRig == null)
            return;

        Vector3 flyDirection = headCamera.forward;

        if (!allowVerticalFlight)
            flyDirection = Vector3.ProjectOnPlane(flyDirection, Vector3.up);

        if (flyDirection.sqrMagnitude < 0.0001f)
            return;

        flyDirection.Normalize();

        playerRig.position += flyDirection * flySpeed * Time.deltaTime;
    }

    // Connect this to Right Fist Gesture > Gesture Performed
    public void OnRightFistPerformed()
    {
        rightFistHeld = true;

        if (logStateChanges)
            Debug.Log("[Right Fist Look Fly] Right fist held. Flying started.");
    }

    // Connect this to Right Fist Gesture > Gesture Ended
    public void OnRightFistEnded()
    {
        rightFistHeld = false;

        if (logStateChanges)
            Debug.Log("[Right Fist Look Fly] Right fist released. Flying stopped.");
    }
}