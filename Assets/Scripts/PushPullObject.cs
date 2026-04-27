using UnityEngine;

public class PushPullObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform moveTarget;        // VisualRoot
    [SerializeField] private Transform distanceReference; // camera

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float minDistance = 0.35f;
    [SerializeField] private float maxDistance = 8.0f;
    [SerializeField] private float triggerDeadzone = 0.1f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float rotateThreshold = 0.9f;

    private bool isSelected;

    private Vector3 lastHandPos;

    private void Start()
    {
        lastHandPos = transform.position;
    }

    public void HandleSelect(Transform disRef)
    {
        isSelected = true;
        distanceReference = disRef;
    }

    public void HandleUnselect()
    {
        isSelected = false;
    }

    private void LateUpdate()
    {
        if (!isSelected || moveTarget == null || distanceReference == null)
            return;

        float rt = OVRInput.Get(
            OVRInput.Axis1D.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );

        float lt = OVRInput.Get(
            OVRInput.Axis1D.PrimaryIndexTrigger,
            OVRInput.Controller.LTouch
        );

        bool rotateMode = rt > rotateThreshold && lt > rotateThreshold;

        // ===== ROTATION MODE =====
        if (rotateMode)
        {
            RotateObject();
            return;
        }

        // ===== NORMAL PUSH/PULL =====
        if (rt < triggerDeadzone) rt = 0f;
        if (lt < triggerDeadzone) lt = 0f;

        float input = rt - lt;
        if (Mathf.Abs(input) < 0.001f) return;

        Vector3 fromPlayer = moveTarget.position - distanceReference.position;
        float currentDistance = fromPlayer.magnitude;

        if (currentDistance < 0.0001f) return;

        Vector3 direction = fromPlayer.normalized;

        float nextDistance = Mathf.Clamp(
            currentDistance + input * moveSpeed * Time.deltaTime,
            minDistance,
            maxDistance
        );

        moveTarget.position =
            distanceReference.position + direction * nextDistance;
    }

    private void RotateObject()
    {
        float rotationPerSecond = rotationSpeed;

        moveTarget.Rotate(
            Vector3.up,
            rotationPerSecond * Time.deltaTime,
            Space.World
        );
    }
}