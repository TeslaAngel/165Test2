using UnityEngine;

public class VR3DMenuInteractor : MonoBehaviour
{
    [Header("Ray")]
    [SerializeField] private float rayLength = 5f;
    [SerializeField] private LayerMask buttonLayers;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Input")]
    [SerializeField] private OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField] private float triggerThreshold = 0.7f;

    private VR3DSpawnButton hoveredButton;
    private VR3DSpawnButton pressedButton;
    private bool wasTriggerHeld;

    private void Update()
    {
        bool triggerHeld =
            OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller) > triggerThreshold;

        UpdateHover();

        if (triggerHeld && !wasTriggerHeld)
        {
            PressCurrent();
        }

        if (!triggerHeld && wasTriggerHeld)
        {
            ReleaseCurrent();
        }

        wasTriggerHeld = triggerHeld;
    }

    private void UpdateHover()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        bool hitSomething = Physics.Raycast(
            ray,
            out RaycastHit hit,
            rayLength,
            buttonLayers
        );

        VR3DSpawnButton newHover = null;

        if (hitSomething)
            newHover = hit.collider.GetComponentInParent<VR3DSpawnButton>();

        if (newHover != hoveredButton)
        {
            if (hoveredButton != null)
                hoveredButton.HoverExit();

            hoveredButton = newHover;

            if (hoveredButton != null)
                hoveredButton.HoverEnter();
        }

        UpdateRayVisual(hitSomething, hit);
    }

    private void PressCurrent()
    {
        if (hoveredButton == null) return;

        pressedButton = hoveredButton;
        pressedButton.Press();
    }

    private void ReleaseCurrent()
    {
        if (pressedButton == null) return;

        pressedButton.Release();
        pressedButton = null;
    }

    private void UpdateRayVisual(bool hitSomething, RaycastHit hit)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);

        Vector3 end = hitSomething
            ? hit.point
            : transform.position + transform.forward * rayLength;

        lineRenderer.SetPosition(1, end);
    }
}