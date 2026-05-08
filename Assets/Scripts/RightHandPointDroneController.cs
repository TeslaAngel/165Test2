using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class RightHandGestureDroneController : MonoBehaviour
{
    private enum ViewMode
    {
        PilotNoVisuals,
        PilotWithCockpit,
        BehindDrone
    }

    [Header("XR Rig References")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform cameraOffset;
    [SerializeField] private Transform headCamera;

    [Header("Flight")]
    [SerializeField] private float flySpeed = 3.0f;
    [SerializeField] private bool allowVerticalFlight = true;

    [Header("View Visuals")]
    [SerializeField] private GameObject virtualCockpitVisual;
    [SerializeField] private GameObject droneBodyVisual;

    [Header("View Positions")]
    [SerializeField] private Vector3 pilotCameraOffsetLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 pilotCameraOffsetLocalEuler = Vector3.zero;

    [SerializeField] private Vector3 behindDroneCameraOffsetLocalPosition = new Vector3(0f, 1.0f, -4.0f);
    [SerializeField] private Vector3 behindDroneCameraOffsetLocalEuler = Vector3.zero;

    [Header("Third-Person Drone Visual Anchor")]
    [SerializeField] private bool keepDroneVisualInFrontOfCameraInBehindView = true;

    [Tooltip("Camera-local position of the drone body in third-person view.")]
    [SerializeField] private Vector3 thirdPersonDroneCameraLocalPosition = new Vector3(0f, -0.45f, 2.2f);

    [Header("Visual Movement Rotation")]
    [Tooltip("The visual transform that should face current movement direction. Usually DroneBodyVisual transform.")]
    [SerializeField] private Transform visualRootToFaceMovement;

    [Tooltip("Use this if your drone model faces the wrong direction.")]
    [SerializeField] private Vector3 visualForwardEulerOffset = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    [SerializeField] private bool logRuntimeValues = false;

    private XRHandSubsystem handSubsystem;

    private bool rightIndexPointHeld;
    private bool rightThumbUpHeld;

    private ViewMode currentViewMode = ViewMode.PilotNoVisuals;

    private Quaternion lastMovementVisualRotation = Quaternion.identity;
    private bool hasMovementVisualRotation;

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

        if (visualRootToFaceMovement == null && droneBodyVisual != null)
            visualRootToFaceMovement = droneBodyVisual.transform;

        ApplyViewMode();
    }

    private void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        if (subsystems.Count == 0)
        {
            Debug.LogError("[Gesture Drone] No XRHandSubsystem found.");
            return;
        }

        handSubsystem = subsystems[0];

        if (logStateChanges)
            Debug.Log("[Gesture Drone] Ready. XR gestures control active state; joints control direction.");
    }

    private void Update()
    {
        if (rightIndexPointHeld)
        {
            if (TryGetRightIndexPointDirection(out Vector3 direction))
            {
                MoveAlongDirection(direction);
            }
            else if (logRuntimeValues && Time.frameCount % 30 == 0)
            {
                Debug.Log("[Gesture Drone] Point gesture held, but index direction unavailable.");
            }
        }
    }

    private void LateUpdate()
    {
        UpdateThirdPersonDroneVisualAnchor();
    }

    private void MoveAlongDirection(Vector3 direction)
    {
        if (!allowVerticalFlight)
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        playerRig.position += direction * flySpeed * Time.deltaTime;

        FaceVisualTowardMovement(direction);

        if (logRuntimeValues && Time.frameCount % 30 == 0)
            Debug.Log("[Gesture Drone] Moving direction: " + direction);
    }

    private bool TryGetRightIndexPointDirection(out Vector3 direction)
    {
        direction = Vector3.zero;

        if (handSubsystem == null)
            return false;

        XRHand rightHand = handSubsystem.rightHand;

        if (!rightHand.isTracked)
            return false;

        if (!TryGetWorldPosition(rightHand, XRHandJointID.IndexProximal, out Vector3 indexProximal))
            return false;

        if (!TryGetWorldPosition(rightHand, XRHandJointID.IndexTip, out Vector3 indexTip))
            return false;

        direction = indexTip - indexProximal;

        return direction.sqrMagnitude > 0.0001f;
    }

    private bool TryGetWorldPosition(XRHand hand, XRHandJointID jointId, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;

        XRHandJoint joint = hand.GetJoint(jointId);

        if (!joint.TryGetPose(out Pose localPose))
            return false;

        Pose originPose = new Pose(xrOrigin.position, xrOrigin.rotation);
        Pose worldPose = localPose.GetTransformedBy(originPose);

        worldPosition = worldPose.position;
        return true;
    }

    private void FaceVisualTowardMovement(Vector3 moveDirection)
    {
        if (visualRootToFaceMovement == null)
            return;

        if (moveDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection.normalized, Vector3.up) *
            Quaternion.Euler(visualForwardEulerOffset);

        lastMovementVisualRotation = targetRotation;
        hasMovementVisualRotation = true;

        visualRootToFaceMovement.rotation = targetRotation;
    }

    private void UpdateThirdPersonDroneVisualAnchor()
    {
        if (currentViewMode != ViewMode.BehindDrone)
            return;

        if (!keepDroneVisualInFrontOfCameraInBehindView)
            return;

        if (headCamera == null || droneBodyVisual == null)
            return;

        Transform droneTransform = droneBodyVisual.transform;

        droneTransform.position = headCamera.TransformPoint(thirdPersonDroneCameraLocalPosition);

        if (visualRootToFaceMovement == droneTransform && hasMovementVisualRotation)
        {
            droneTransform.rotation = lastMovementVisualRotation;
        }
    }

    // ------------------------------------------------------------------
    // XR Hands Static Gesture UnityEvents
    // ------------------------------------------------------------------

    // Connect to Right Index Point Gesture > Gesture Performed
    public void OnRightIndexPointPerformed()
    {
        rightIndexPointHeld = true;

        if (logStateChanges)
            Debug.Log("[Gesture Drone] Right index point active. Flying started.");
    }

    // Connect to Right Index Point Gesture > Gesture Ended
    public void OnRightIndexPointEnded()
    {
        rightIndexPointHeld = false;

        if (logStateChanges)
            Debug.Log("[Gesture Drone] Right index point ended. Flying stopped.");
    }

    // Connect to Right Thumb Up Gesture > Gesture Performed
    public void OnRightThumbUpPerformed()
    {
        if (rightThumbUpHeld)
            return;

        rightThumbUpHeld = true;
        CycleViewMode();

        if (logStateChanges)
            Debug.Log("[Gesture Drone] Right thumb-up active.");
    }

    // Connect to Right Thumb Up Gesture > Gesture Ended
    public void OnRightThumbUpEnded()
    {
        rightThumbUpHeld = false;

        if (logStateChanges)
            Debug.Log("[Gesture Drone] Right thumb-up ended.");
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

                UpdateThirdPersonDroneVisualAnchor();

                break;
        }

        if (logStateChanges)
            Debug.Log("[Gesture Drone] View mode: " + currentViewMode);
    }

    private void SetCameraOffset(Vector3 localPosition, Vector3 localEuler)
    {
        if (cameraOffset == null)
        {
            Debug.LogWarning("[Gesture Drone] Camera Offset is not assigned.");
            return;
        }

        cameraOffset.localPosition = localPosition;
        cameraOffset.localRotation = Quaternion.Euler(localEuler);
    }
}