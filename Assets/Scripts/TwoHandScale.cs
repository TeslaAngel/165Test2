using UnityEngine;

public class TwoHandScale : MonoBehaviour
{
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [SerializeField] private float minScale = 0.25f;
    [SerializeField] private float maxScale = 3.0f;
    [SerializeField] private float gripThreshold = 0.5f;
    [SerializeField] private bool debugLogs = true;

    private bool isSelected;
    private bool isScaling;

    private float startDistance;
    private Vector3 startScale;

    public void HandleSelect()
    {
        isSelected = true;
        if (debugLogs) Debug.Log($"{name}: TwoHandScale SELECTED");
    }

    public void HandleUnselect()
    {
        isSelected = false;
        isScaling = false;
        if (debugLogs) Debug.Log($"{name}: TwoHandScale UNSELECTED");
    }

    private void LateUpdate()
    {
        if (!isSelected)
            return;

        if (leftHand == null || rightHand == null)
        {
            Debug.LogWarning($"{name}: Missing leftHand or rightHand reference.");
            return;
        }

        bool leftGrip =
            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > gripThreshold;

        bool rightGrip =
            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > gripThreshold;

        bool bothGrips = leftGrip && rightGrip;

        if (!bothGrips)
        {
            isScaling = false;
            return;
        }

        float currentDistance = Vector3.Distance(leftHand.position, rightHand.position);

        if (!isScaling)
        {
            startDistance = currentDistance;
            startScale = transform.localScale;
            isScaling = startDistance > 0.0001f;

            if (debugLogs)
                Debug.Log($"{name}: Scaling started. Start distance = {startDistance}, start scale = {startScale}");

            return;
        }

        float ratio = currentDistance / startDistance;
        Vector3 targetScale = startScale * ratio;

        targetScale.x = Mathf.Clamp(targetScale.x, minScale, maxScale);
        targetScale.y = Mathf.Clamp(targetScale.y, minScale, maxScale);
        targetScale.z = Mathf.Clamp(targetScale.z, minScale, maxScale);

        transform.localScale = targetScale;

        if (debugLogs)
            Debug.Log($"{name}: Scaling. Current distance = {currentDistance}, ratio = {ratio}, scale = {targetScale}");
    }
}