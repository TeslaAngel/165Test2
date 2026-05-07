using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class SimpleHandDroneController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform xrOrigin;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float deadzone = 0.10f;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    [SerializeField] private bool logRuntimeValues = false;

    private XRHandSubsystem handSubsystem;

    private bool leftFist;
    private bool rightFist;
    private bool droneActive;

    private Vector3 defaultLeftPalmWorld;
    private Vector3 defaultRightPalmWorld;
    private Vector3 defaultAveragePalmWorld;

    private void Awake()
    {
        if (playerRig == null)
            playerRig = transform;

        if (xrOrigin == null)
            xrOrigin = playerRig;
    }

    private void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        if (subsystems.Count == 0)
        {
            Debug.LogError("[Simple Hand Drone] No XRHandSubsystem found.");
            return;
        }

        handSubsystem = subsystems[0];

        if (logStateChanges)
            Debug.Log("[Simple Hand Drone] Ready. Waiting for both fists.");
    }

    private void Update()
    {
        if (!droneActive)
            return;

        if (!TryGetPalmWorldPositions(out Vector3 leftPalmWorld, out Vector3 rightPalmWorld))
        {
            StopDrone("Lost hand tracking.");
            return;
        }

        Vector3 currentAveragePalmWorld = (leftPalmWorld + rightPalmWorld) * 0.5f;

        Vector3 displacement = currentAveragePalmWorld - defaultAveragePalmWorld;

        if (displacement.magnitude < deadzone)
        {
            if (logRuntimeValues && Time.frameCount % 30 == 0)
                Debug.Log("[Simple Hand Drone] Inside deadzone.");

            return;
        }

        Vector3 moveDirection = displacement.normalized;

        playerRig.position += moveDirection * moveSpeed * Time.deltaTime;

        if (logRuntimeValues && Time.frameCount % 30 == 0)
        {
            Debug.Log(
                $"[Simple Hand Drone] displacement={displacement}, direction={moveDirection}"
            );
        }
    }

    private void TryStartDrone()
    {
        if (droneActive)
            return;

        if (!leftFist || !rightFist)
            return;

        if (!TryGetPalmWorldPositions(out Vector3 leftPalmWorld, out Vector3 rightPalmWorld))
        {
            Debug.LogWarning("[Simple Hand Drone] Both fists detected, but palm poses are unavailable.");
            return;
        }

        defaultLeftPalmWorld = leftPalmWorld;
        defaultRightPalmWorld = rightPalmWorld;
        defaultAveragePalmWorld = (leftPalmWorld + rightPalmWorld) * 0.5f;

        droneActive = true;

        if (logStateChanges)
        {
            Debug.Log("[Simple Hand Drone] Drone started.");
            Debug.Log("[Simple Hand Drone] Default average palm world position: " + defaultAveragePalmWorld);
        }
    }

    private void StopDrone(string reason)
    {
        if (!droneActive)
            return;

        droneActive = false;

        if (logStateChanges)
            Debug.Log("[Simple Hand Drone] Drone stopped. Reason: " + reason);
    }

    private bool TryGetPalmWorldPositions(out Vector3 leftPalmWorld, out Vector3 rightPalmWorld)
    {
        leftPalmWorld = Vector3.zero;
        rightPalmWorld = Vector3.zero;

        if (handSubsystem == null)
            return false;

        if (!handSubsystem.leftHand.isTracked || !handSubsystem.rightHand.isTracked)
            return false;

        XRHandJoint leftPalm = handSubsystem.leftHand.GetJoint(XRHandJointID.Palm);
        XRHandJoint rightPalm = handSubsystem.rightHand.GetJoint(XRHandJointID.Palm);

        if (!leftPalm.TryGetPose(out Pose leftPose))
            return false;

        if (!rightPalm.TryGetPose(out Pose rightPose))
            return false;

        Pose originPose = new Pose(xrOrigin.position, xrOrigin.rotation);

        Pose leftWorldPose = leftPose.GetTransformedBy(originPose);
        Pose rightWorldPose = rightPose.GetTransformedBy(originPose);

        leftPalmWorld = leftWorldPose.position;
        rightPalmWorld = rightWorldPose.position;

        return true;
    }

    // ------------------------------------------------------------
    // Call these from your Static Hand Gesture UnityEvents.
    // ------------------------------------------------------------

    public void OnLeftFistPerformed()
    {
        leftFist = true;

        if (logStateChanges)
            Debug.Log("[Simple Hand Drone] Left fist active.");

        TryStartDrone();
    }

    public void OnLeftFistEnded()
    {
        leftFist = false;

        if (logStateChanges)
            Debug.Log("[Simple Hand Drone] Left fist released.");

        StopDrone("Left fist released.");
    }

    public void OnRightFistPerformed()
    {
        rightFist = true;

        if (logStateChanges)
            Debug.Log("[Simple Hand Drone] Right fist active.");

        TryStartDrone();
    }

    public void OnRightFistEnded()
    {
        rightFist = false;

        if (logStateChanges)
            Debug.Log("[Simple Hand Drone] Right fist released.");

        StopDrone("Right fist released.");
    }
}