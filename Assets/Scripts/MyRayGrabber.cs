using UnityEngine;

public class MyRayGrabber : MonoBehaviour
{
    [Header("Ray")]
    [SerializeField] private float rayLength = 8f;
    [SerializeField] private LayerMask grabbableLayers;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Input")]
    [SerializeField] private OVRInput.Controller controller;
    [SerializeField] private float gripThreshold = 0.6f;

    [Header("Grab")]
    [SerializeField] private bool keepGrabOffset = true;
    [SerializeField] private float throwMultiplier = 1.2f;

    private MyGrabbable hovered;
    private MyGrabbable grabbed;

    private Vector3 localGrabOffset;
    private Quaternion localRotationOffset;

    private Vector3 lastHandPosition;
    private Vector3 handVelocity;

    private bool wasGripHeld;

    private void Start()
    {
        lastHandPosition = transform.position;
    }

    private void Update()
    {
        UpdateHandVelocity();

        bool gripHeld =
            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller) > gripThreshold;

        if (grabbed == null)
        {
            UpdateRayHover();
        }
        else
        {
            HoldGrabbedObject();
        }

        if (gripHeld && !wasGripHeld)
        {
            TryGrab();
        }

        if (!gripHeld && wasGripHeld)
        {
            Release();
        }

        wasGripHeld = gripHeld;
    }

    private void UpdateHandVelocity()
    {
        handVelocity = (transform.position - lastHandPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastHandPosition = transform.position;
    }

    private void UpdateRayHover()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        bool hitSomething = Physics.Raycast(
            ray,
            out RaycastHit hit,
            rayLength,
            grabbableLayers
        );

        MyGrabbable newHover = null;

        if (hitSomething)
        {
            newHover = hit.collider.GetComponentInParent<MyGrabbable>();
        }

        if (newHover != hovered)
        {
            if (hovered != null)
                hovered.HoverExit();

            hovered = newHover;

            if (hovered != null)
                hovered.HoverEnter();
        }

        UpdateRayVisual(hitSomething, hit);
    }

    private void TryGrab()
    {
        if (hovered == null) return;

        grabbed = hovered;
        grabbed.SelectEnter();

        if (keepGrabOffset)
        {
            localGrabOffset = Quaternion.Inverse(transform.rotation) *
                              (grabbed.transform.position - transform.position);

            localRotationOffset = Quaternion.Inverse(transform.rotation) *
                                  grabbed.transform.rotation;
        }
        else
        {
            localGrabOffset = Vector3.zero;
            localRotationOffset = Quaternion.identity;
        }

        Rigidbody rb = grabbed.Rigidbody;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void HoldGrabbedObject()
    {
        if (grabbed == null) return;

        Vector3 targetPosition =
            transform.position + transform.rotation * localGrabOffset;

        Quaternion targetRotation =
            transform.rotation * localRotationOffset;

        grabbed.transform.position = targetPosition;
        grabbed.transform.rotation = targetRotation;

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void Release()
    {
        if (grabbed == null) return;

        Rigidbody rb = grabbed.Rigidbody;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = handVelocity * throwMultiplier;
        }

        grabbed.SelectExit();
        grabbed = null;
    }

    private void UpdateRayVisual(bool hitSomething, RaycastHit hit)
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = grabbed == null;

        Vector3 start = transform.position;
        Vector3 end = hitSomething
            ? hit.point
            : transform.position + transform.forward * rayLength;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}