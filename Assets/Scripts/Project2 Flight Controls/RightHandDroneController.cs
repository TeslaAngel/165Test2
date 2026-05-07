using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class RightHandDroneController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform xrOrigin;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float deadzone = 0.08f;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    [SerializeField] private bool logRuntimeValues = true;

    private XRHandSubsystem handSubsystem;

    private bool rightFist;
    private bool droneActive;

    private Vector3 defaultRightPalmWorld;

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
            Debug.LogError("[Right Hand Drone] No XRHandSubsystem found.");
            return;
        }

        handSubsystem = subsystems[0];

        if (logStateChanges)
            Debug.Log("[Right Hand Drone] Ready. Make right fist to start.");
    }

    private void Update()
    {
        if (!droneActive)
            return;

        if (!TryGetRightPalmWorldPosition(out Vector3 currentRightPalmWorld))
        {
            StopDrone("Lost right hand tracking.");
            return;
        }

        Vector3 displacement = currentRightPalmWorld - defaultRightPalmWorld;

        if (displacement.magnitude < deadzone)
        {
            if (logRuntimeValues && Time.frameCount % 30 == 0)
            {
                Debug.Log(
                    $"[Right Hand Drone] Inside deadzone. displacement magnitude={displacement.magnitude:F3}"
                );
            }

            return;
        }

        Vector3 moveDirection = displacement.normalized;

        playerRig.position += moveDirection * moveSpeed * Time.deltaTime;

        if (logRuntimeValues && Time.frameCount % 30 == 0)
        {
            Debug.Log(
                $"[Right Hand Drone] Moving. displacement={displacement}, direction={moveDirection}, speed={moveSpeed}"
            );
        }
    }

    private void StartDrone()
    {
        if (droneActive)
            return;

        if (!rightFist)
            return;

        if (!TryGetRightPalmWorldPosition(out Vector3 rightPalmWorld))
        {
            Debug.LogWarning("[Right Hand Drone] Right fist detected, but right palm pose is unavailable.");
            return;
        }

        defaultRightPalmWorld = rightPalmWorld;
        droneActive = true;

        if (logStateChanges)
        {
            Debug.Log("[Right Hand Drone] Drone started.");
            Debug.Log("[Right Hand Drone] Default right palm world position: " + defaultRightPalmWorld);
        }
    }

    private void StopDrone(string reason)
    {
        if (!droneActive)
            return;

        droneActive = false;

        if (logStateChanges)
            Debug.Log("[Right Hand Drone] Drone stopped. Reason: " + reason);
    }

    private bool TryGetRightPalmWorldPosition(out Vector3 rightPalmWorld)
    {
        rightPalmWorld = Vector3.zero;

        if (handSubsystem == null)
            return false;

        XRHand rightHand = handSubsystem.rightHand;

        if (!rightHand.isTracked)
            return false;

        XRHandJoint palmJoint = rightHand.GetJoint(XRHandJointID.Palm);

        if (!palmJoint.TryGetPose(out Pose palmPose))
            return false;

        Pose originPose = new Pose(xrOrigin.position, xrOrigin.rotation);
        Pose worldPose = palmPose.GetTransformedBy(originPose);

        rightPalmWorld = worldPose.position;
        return true;
    }

    // Call this from Right Fist Gesture > Gesture Performed
    public void OnRightFistPerformed()
    {
        rightFist = true;

        if (logStateChanges)
            Debug.Log("[Right Hand Drone] Right fist active.");

        StartDrone();
    }

    // Call this from Right Fist Gesture > Gesture Ended
    public void OnRightFistEnded()
    {
        rightFist = false;

        if (logStateChanges)
            Debug.Log("[Right Hand Drone] Right fist released.");

        StopDrone("Right fist released.");
    }

    // Optional: bind this to a button while testing
    public void ResetDefaultRightHandPosition()
    {
        if (!TryGetRightPalmWorldPosition(out Vector3 rightPalmWorld))
        {
            Debug.LogWarning("[Right Hand Drone] Cannot reset default. Right palm unavailable.");
            return;
        }

        defaultRightPalmWorld = rightPalmWorld;

        Debug.Log("[Right Hand Drone] Default right palm position reset: " + defaultRightPalmWorld);
    }
}