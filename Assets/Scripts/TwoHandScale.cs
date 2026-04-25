using UnityEngine;

public class TwoHandScale : MonoBehaviour
{
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [SerializeField] private float minScale = 0.25f;
    [SerializeField] private float maxScale = 3.0f;
    [SerializeField] private float gripThreshold = 0.5f;
    [SerializeField] private bool debugLogs = false;

    private bool isSelected;
    private bool isScaling;

    private float startDistance;
    private Vector3 startScale;
    private Vector3 committedScale;

    public Transform target;

    private void Awake()
    {
        committedScale = target.localScale;
    }

    public void HandleSelect()
    {
        isSelected = true;
        if (debugLogs) Debug.Log($"{name}: TwoHandScale SELECTED");
    }

    public void HandleUnselect()
    {
        isSelected = false;
        isScaling = false;
        StopScalingAndCommit();
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

        // we scale object based on distance between hands
        float currentDistance = Vector3.Distance(leftHand.position, rightHand.position);

        if (!isScaling)
        {
            startDistance = currentDistance;
            startScale = target.localScale;
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

        target.localScale = targetScale;
        committedScale = targetScale;

        if (debugLogs)
            Debug.Log($"{name}: Scaling. Current distance = {currentDistance}, ratio = {ratio}, scale = {targetScale}");
    }

    private void StopScalingAndCommit()
    {
        if (!isScaling)
            return;

        isScaling = false;
        target.localScale = committedScale;
    }
}