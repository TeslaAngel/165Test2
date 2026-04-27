using UnityEngine;

public class CustomRadiusGrabber : MonoBehaviour
{
    [Header("Radius")]
    [SerializeField] private float grabRadius = 0.35f;
    [SerializeField] private LayerMask grabbableLayers;

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

    private readonly Collider[] hits = new Collider[32];

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
            UpdateRadiusHover();
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

    private void UpdateRadiusHover()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            grabRadius,
            hits,
            grabbableLayers
        );

        MyGrabbable nearest = null;
        float nearestDistSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider hit = hits[i];
            if (hit == null) continue;

            MyGrabbable candidate =
                hit.GetComponentInParent<MyGrabbable>();

            if (candidate == null) continue;

            float d = (candidate.transform.position - transform.position).sqrMagnitude;

            if (d < nearestDistSqr)
            {
                nearest = candidate;
                nearestDistSqr = d;
            }
        }

        if (nearest != hovered)
        {
            if (hovered != null)
                hovered.HoverExit();

            hovered = nearest;

            if (hovered != null)
                hovered.HoverEnter();
        }
    }

    private void TryGrab()
    {
        if (hovered == null) return;

        grabbed = hovered;
        grabbed.SelectEnter();

        if (keepGrabOffset)
        {
            localGrabOffset =
                Quaternion.Inverse(transform.rotation) *
                (grabbed.transform.position - transform.position);

            localRotationOffset =
                Quaternion.Inverse(transform.rotation) *
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
        Vector3 targetPosition =
            transform.position + transform.rotation * localGrabOffset;

        Quaternion targetRotation =
            transform.rotation * localRotationOffset;

        grabbed.transform.position = targetPosition;
        grabbed.transform.rotation = targetRotation;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}