using UnityEngine;

public class PushPullObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform moveTarget;        // VisualRoot child
    [SerializeField] private Transform distanceReference; // CenterEyeAnchor / Main Camera

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float minDistance = 0.35f;
    [SerializeField] private float maxDistance = 8.0f;
    [SerializeField] private float triggerDeadzone = 0.1f;

    private bool isSelected;

    private void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        if (distanceReference == null && Camera.main != null)
            distanceReference = Camera.main.transform;
    }

    public void HandleSelect()
    {
        isSelected = true;
    }

    public void HandleUnselect()
    {
        isSelected = false;
    }

    private void LateUpdate()
    {
        if (!isSelected) return;
        if (moveTarget == null || distanceReference == null) return;

        float rt = OVRInput.Get(
            OVRInput.Axis1D.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );

        float lt = OVRInput.Get(
            OVRInput.Axis1D.PrimaryIndexTrigger,
            OVRInput.Controller.LTouch
        );

        if (rt < triggerDeadzone) rt = 0f;
        if (lt < triggerDeadzone) lt = 0f;

        float input = rt - lt;
        if (Mathf.Abs(input) < 0.001f) return;

        Vector3 fromPlayer = moveTarget.position - distanceReference.position;
        float currentDistance = fromPlayer.magnitude;

        if (currentDistance < 0.0001f) return;

        Vector3 directionFromPlayer = fromPlayer.normalized;

        float nextDistance = Mathf.Clamp(
            currentDistance + input * moveSpeed * Time.deltaTime,
            minDistance,
            maxDistance
        );

        Vector3 nextWorldPosition =
            distanceReference.position + directionFromPlayer * nextDistance;

        moveTarget.position = nextWorldPosition;
    }
}