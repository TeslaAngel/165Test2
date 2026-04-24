using UnityEngine;

public class MetaPushPullObject : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform distanceReference;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float minDistance = 0.25f;
    [SerializeField] private float maxDistance = 6.0f;

    [Header("Optional")]
    [SerializeField] private bool requireSelectedState = true;

    private bool _isSelected;

    private void Start()
    {
        if (distanceReference == null && Camera.main != null)
        {
            distanceReference = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (requireSelectedState && !_isSelected) return;
        if (distanceReference == null) return;

        float leftTrigger =
            OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        float rightTrigger =
            OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        float delta = (rightTrigger - leftTrigger) * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(delta) < 0.0001f) return;

        Vector3 fromReference = transform.position - distanceReference.position;
        float currentDistance = fromReference.magnitude;
        if (currentDistance < 0.0001f) return;

        Vector3 dir = fromReference / currentDistance;
        float nextDistance = Mathf.Clamp(currentDistance + delta, minDistance, maxDistance);

        transform.position = distanceReference.position + dir * nextDistance;
    }

    public void HandleSelect()
    {
        _isSelected = true;
    }

    public void HandleUnselect()
    {
        _isSelected = false;
    }
}