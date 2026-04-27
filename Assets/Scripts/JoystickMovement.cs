using UnityEngine;

public class JoystickMovement : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headReference;

    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float deadzone = 0.15f;
    [SerializeField] private bool useHeadDirection = true;

    [SerializeField] private bool lockYAxis = true;

    private void Awake()
    {
        if (playerRoot == null)
            playerRoot = transform;

        if (headReference == null && Camera.main != null)
            headReference = Camera.main.transform;
    }

    private void Update()
    {
        // input here
        Vector2 stick = OVRInput.Get(
            OVRInput.Axis2D.PrimaryThumbstick,
            OVRInput.Controller.LTouch
        );

        // to sensitive is bad
        if (stick.magnitude < deadzone)
            return;

        Vector3 forward;
        Vector3 right;

        if (useHeadDirection && headReference != null)
        {
            forward = headReference.forward;
            right = headReference.right;
        }
        else
        {
            forward = playerRoot.forward;
            right = playerRoot.right;
        }

        if (lockYAxis)
        {
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 moveDirection =
            forward * stick.y +
            right * stick.x;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        Vector3 delta = moveDirection * moveSpeed * Time.deltaTime;

        playerRoot.position += delta;
    }
}