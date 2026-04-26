using UnityEngine;

public class PushPullObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform moveTarget;        // VisualRoot child
    [SerializeField] private Transform distanceReference; // CenterEyeAnchor / Main Camera

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float minLocalZ = -3.0f;
    [SerializeField] private float maxLocalZ = 3.0f;
    [SerializeField] private float triggerDeadzone = 0.1f;

    private bool isSelected;
    private Vector3 startLocalPosition;

    private void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        if (distanceReference == null && Camera.main != null)
            distanceReference = Camera.main.transform;

        startLocalPosition = moveTarget.localPosition;
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
        if (moveTarget == null) return;

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

        Vector3 local = moveTarget.localPosition;

        // Right trigger pushes the visible/collider child forward.
        // Left trigger pulls it back.
        local.z += input * moveSpeed * Time.deltaTime;
        local.z = Mathf.Clamp(local.z, minLocalZ, maxLocalZ);

        moveTarget.localPosition = local;
    }

    public void ResetOffset()
    {
        if (moveTarget != null)
            moveTarget.localPosition = startLocalPosition;
    }
}