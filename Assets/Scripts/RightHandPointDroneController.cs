using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class RightHandPointDroneController : MonoBehaviour
{
    private enum ViewMode
    {
        PilotNoVisuals,
        PilotWithCockpit,
        BehindDrone
    }

    [Header("XR Rig References")]
    [Tooltip("The XR Origin / player rig root. This object moves.")]
    [SerializeField] private Transform playerRig;

    [Tooltip("Usually the same XR Origin transform.")]
    [SerializeField] private Transform xrOrigin;

    [Tooltip("Usually Camera Offset under XR Origin.")]
    [SerializeField] private Transform cameraOffset;

    [Tooltip("Main Camera / Center Eye Camera.")]
    [SerializeField] private Transform headCamera;

    [Header("Flight")]
    [SerializeField] private float flySpeed = 3.0f;

    [Tooltip("If true, pointing up/down also moves vertically. If false, movement is projected horizontally.")]
    [SerializeField] private bool allowVerticalFlight = true;

    [Header("View Visuals")]
    [SerializeField] private GameObject virtualCockpitVisual;
    [SerializeField] private GameObject droneBodyVisual;

    [Header("View Positions")]
    [SerializeField] private Vector3 pilotCameraOffsetLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 pilotCameraOffsetLocalEuler = Vector3.zero;

    [SerializeField] private Vector3 behindDroneCameraOffsetLocalPosition = new Vector3(0f, 1.0f, -4.0f);
    [SerializeField] private Vector3 behindDroneCameraOffsetLocalEuler = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    [SerializeField] private bool logRuntimeValues = false;

    private XRHandSubsystem handSubsystem;

    private bool rightIndexPointHeld;
    private bool rightThumbUpHeld;

    private ViewMode currentViewMode = ViewMode.PilotNoVisuals;

    private void Awake()
    {
        if (playerRig == null)
            playerRig = transform;

        if (xrOrigin == null)
            xrOrigin = playerRig;

        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        if (cameraOffset == null && headCamera != null && headCamera.parent != null)
            cameraOffset = headCamera.parent;

        ApplyViewMode();
    }

    private void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        if (subsystems.Count == 0)
        {
            Debug.LogError("[Point Drone] No XRHandSubsystem found.");
            return;
        }

        handSubsystem = subsystems[0];

        if (logStateChanges)
            Debug.Log("[Point Drone] Ready. Point with right index finger to fly.");
    }

    private void Update()
    {
        if (!rightIndexPointHeld)
            return;

        if (!TryGetRightIndexPointDirection(out Vector3 pointDirection))
            return;

        if (!allowVerticalFlight)
            pointDirection = Vector3.ProjectOnPlane(pointDirection, Vector3.up);

        if (pointDirection.sqrMagnitude < 0.0001f)
            return;

        pointDirection.Normalize();

        playerRig.position += pointDirection * flySpeed * Time.deltaTime;

        if (logRuntimeValues && Time.frameCount % 30 == 0)
        {
            Debug.Log("[Point Drone] Flying direction: " + pointDirection);
        }
    }

    private bool TryGetRightIndexPointDirection(out Vector3 direction)
    {
        direction = Vector3.zero;

        if (handSubsystem == null)
            return false;

        XRHand rightHand = handSubsystem.rightHand;

        if (!rightHand.isTracked)
            return false;

        XRHandJoint indexProximal = rightHand.GetJoint(XRHandJointID.IndexProximal);
        XRHandJoint indexTip = rightHand.GetJoint(XRHandJointID.IndexTip);

        if (!indexProximal.TryGetPose(out Pose proximalPose))
            return false;

        if (!indexTip.TryGetPose(out Pose tipPose))
            return false;

        Pose originPose = new Pose(xrOrigin.position, xrOrigin.rotation);

        Pose proximalWorldPose = proximalPose.GetTransformedBy(originPose);
        Pose tipWorldPose = tipPose.GetTransformedBy(originPose);

        direction = tipWorldPose.position - proximalWorldPose.position;

        return direction.sqrMagnitude > 0.0001f;
    }

    // ------------------------------------------------------------------
    // Right index pointing gesture events
    // ------------------------------------------------------------------

    public void OnRightIndexPointPerformed()
    {
        rightIndexPointHeld = true;

        if (logStateChanges)
            Debug.Log("[Point Drone] Right index point detected. Flying started.");
    }

    public void OnRightIndexPointEnded()
    {
        rightIndexPointHeld = false;

        if (logStateChanges)
            Debug.Log("[Point Drone] Right index point ended. Flying stopped.");
    }

    // ------------------------------------------------------------------
    // Right thumb-up gesture events
    // ------------------------------------------------------------------

    public void OnRightThumbUpPerformed()
    {
        if (rightThumbUpHeld)
            return;

        rightThumbUpHeld = true;
        CycleViewMode();
    }

    public void OnRightThumbUpEnded()
    {
        rightThumbUpHeld = false;

        if (logStateChanges)
            Debug.Log("[Point Drone] Right thumb-up ended.");
    }

    // ------------------------------------------------------------------
    // View switching
    // ------------------------------------------------------------------

    private void CycleViewMode()
    {
        currentViewMode = currentViewMode switch
        {
            ViewMode.PilotNoVisuals => ViewMode.PilotWithCockpit,
            ViewMode.PilotWithCockpit => ViewMode.BehindDrone,
            ViewMode.BehindDrone => ViewMode.PilotNoVisuals,
            _ => ViewMode.PilotNoVisuals
        };

        ApplyViewMode();
    }

    private void ApplyViewMode()
    {
        switch (currentViewMode)
        {
            case ViewMode.PilotNoVisuals:
                SetCameraOffset(pilotCameraOffsetLocalPosition, pilotCameraOffsetLocalEuler);

                if (virtualCockpitVisual != null)
                    virtualCockpitVisual.SetActive(false);

                if (droneBodyVisual != null)
                    droneBodyVisual.SetActive(false);

                break;

            case ViewMode.PilotWithCockpit:
                SetCameraOffset(pilotCameraOffsetLocalPosition, pilotCameraOffsetLocalEuler);

                if (virtualCockpitVisual != null)
                    virtualCockpitVisual.SetActive(true);

                if (droneBodyVisual != null)
                    droneBodyVisual.SetActive(false);

                break;

            case ViewMode.BehindDrone:
                SetCameraOffset(behindDroneCameraOffsetLocalPosition, behindDroneCameraOffsetLocalEuler);

                if (virtualCockpitVisual != null)
                    virtualCockpitVisual.SetActive(false);

                if (droneBodyVisual != null)
                    droneBodyVisual.SetActive(true);

                break;
        }

        if (logStateChanges)
            Debug.Log("[Point Drone] View mode: " + currentViewMode);
    }

    private void SetCameraOffset(Vector3 localPosition, Vector3 localEuler)
    {
        if (cameraOffset == null)
        {
            Debug.LogWarning("[Point Drone] Camera Offset is not assigned.");
            return;
        }

        cameraOffset.localPosition = localPosition;
        cameraOffset.localRotation = Quaternion.Euler(localEuler);
    }
}