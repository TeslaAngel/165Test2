using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class HandDroneFlightController : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("Usually your XR Origin / Player Rig root. This object will move and rotate.")]
    [SerializeField] private Transform playerRig;

    [Tooltip("Usually the Main Camera / Center Eye Camera.")]
    [SerializeField] private Transform headCamera;

    [Tooltip("Usually the same XR Origin transform used by XR Hands.")]
    [SerializeField] private Transform xrOrigin;

    [Header("Flight Settings")]
    [SerializeField] private float maxForwardSpeed = 4.0f;
    [SerializeField] private float acceleration = 8.0f;
    [SerializeField] private float deceleration = 10.0f;

    [Header("Hand Movement Mapping")]
    [Tooltip("How far hands must move forward/back from default position before movement starts.")]
    [SerializeField] private float forwardDeadzone = 0.08f;

    [Tooltip("Hand displacement needed to reach full speed.")]
    [SerializeField] private float fullSpeedHandDistance = 0.35f;

    [Header("Turning Settings")]
    [Tooltip("Difference between left and right hand forward distance before turning starts.")]
    [SerializeField] private float turnDeadzone = 0.08f;

    [Tooltip("Difference needed to reach full turn speed.")]
    [SerializeField] private float fullTurnHandDifference = 0.30f;

    [SerializeField] private float maxTurnSpeedDegrees = 90.0f;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    [SerializeField] private bool logRuntimeValues = false;

    private XRHandSubsystem handSubsystem;

    private bool leftFist;
    private bool rightFist;
    private bool droneArmed;

    private Vector3 defaultLeftHandHeadLocal;
    private Vector3 defaultRightHandHeadLocal;
    private Vector3 defaultAverageHandHeadLocal;

    private float currentForwardSpeed;

    private void Awake()
    {
        if (playerRig == null)
            playerRig = transform;

        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        if (xrOrigin == null)
            xrOrigin = playerRig;
    }

    private void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        if (subsystems.Count == 0)
        {
            Debug.LogError("[Hand Drone] No XRHandSubsystem found. Hand tracking cannot control drone.");
            return;
        }

        handSubsystem = subsystems[0];

        if (logStateChanges)
            Debug.Log("[Hand Drone] XRHandSubsystem found. Waiting for both fists.");
    }

    private void Update()
    {
        if (!droneArmed)
        {
            currentForwardSpeed = Mathf.MoveTowards(
                currentForwardSpeed,
                0f,
                deceleration * Time.deltaTime
            );

            return;
        }

        if (!TryGetPalmWorldPositions(out Vector3 leftPalmWorld, out Vector3 rightPalmWorld))
        {
            StopDrone("Lost hand tracking.");
            return;
        }

        Vector3 leftHeadLocal = headCamera.InverseTransformPoint(leftPalmWorld);
        Vector3 rightHeadLocal = headCamera.InverseTransformPoint(rightPalmWorld);
        Vector3 averageHeadLocal = (leftHeadLocal + rightHeadLocal) * 0.5f;

        HandleForwardBackwardMovement(averageHeadLocal);
        HandleTurning(leftHeadLocal, rightHeadLocal);

        if (logRuntimeValues && Time.frameCount % 30 == 0)
        {
            float forwardDelta = averageHeadLocal.z - defaultAverageHandHeadLocal.z;
            float handDifference = rightHeadLocal.z - leftHeadLocal.z;

            Debug.Log(
                $"[Hand Drone] forwardDelta={forwardDelta:F3}, handDifference={handDifference:F3}, speed={currentForwardSpeed:F2}"
            );
        }
    }

    private void HandleForwardBackwardMovement(Vector3 averageHandHeadLocal)
    {
        float forwardDelta = averageHandHeadLocal.z - defaultAverageHandHeadLocal.z;

        float targetSpeed = 0f;

        if (Mathf.Abs(forwardDelta) > forwardDeadzone)
        {
            float effectiveDelta = Mathf.Abs(forwardDelta) - forwardDeadzone;
            float input01 = Mathf.Clamp01(effectiveDelta / fullSpeedHandDistance);

            targetSpeed = Mathf.Sign(forwardDelta) * input01 * maxForwardSpeed;
        }

        float speedChangeRate = Mathf.Abs(targetSpeed) > Mathf.Abs(currentForwardSpeed)
            ? acceleration
            : deceleration;

        currentForwardSpeed = Mathf.MoveTowards(
            currentForwardSpeed,
            targetSpeed,
            speedChangeRate * Time.deltaTime
        );

        Vector3 flightDirection = headCamera.forward.normalized;

        playerRig.position += flightDirection * currentForwardSpeed * Time.deltaTime;
    }

    private void HandleTurning(Vector3 leftHandHeadLocal, Vector3 rightHandHeadLocal)
    {
        /*
         * Head-local z:
         * Higher z = farther in front of the player's face/body.
         * Lower z = closer to the player's body.
         *
         * If left z is smaller than right z, left hand is closer.
         * We turn left.
         *
         * If right z is smaller than left z, right hand is closer.
         * We turn right.
         */

        float leftZ = leftHandHeadLocal.z;
        float rightZ = rightHandHeadLocal.z;

        float difference = rightZ - leftZ;

        float turnInput = 0f;

        if (Mathf.Abs(difference) > turnDeadzone)
        {
            float effectiveDifference = Mathf.Abs(difference) - turnDeadzone;
            float input01 = Mathf.Clamp01(effectiveDifference / fullTurnHandDifference);

            /*
             * Positive difference means:
             * right hand is farther, left hand is closer -> turn left.
             *
             * Unity positive yaw usually turns right, so we use negative here.
             * If your turning direction feels reversed, remove the minus sign.
             */
            turnInput = -Mathf.Sign(difference) * input01;
        }

        float yawAmount = turnInput * maxTurnSpeedDegrees * Time.deltaTime;

        playerRig.Rotate(Vector3.up, yawAmount, Space.World);
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

    private void TryArmDrone()
    {
        if (droneArmed)
            return;

        if (!leftFist || !rightFist)
            return;

        if (!TryGetPalmWorldPositions(out Vector3 leftPalmWorld, out Vector3 rightPalmWorld))
        {
            Debug.LogWarning("[Hand Drone] Both fists detected, but palm positions are not available yet.");
            return;
        }

        defaultLeftHandHeadLocal = headCamera.InverseTransformPoint(leftPalmWorld);
        defaultRightHandHeadLocal = headCamera.InverseTransformPoint(rightPalmWorld);
        defaultAverageHandHeadLocal = (defaultLeftHandHeadLocal + defaultRightHandHeadLocal) * 0.5f;

        droneArmed = true;
        currentForwardSpeed = 0f;

        if (logStateChanges)
        {
            Debug.Log(
                $"[Hand Drone] Drone armed. Default average hand position={defaultAverageHandHeadLocal}"
            );
        }
    }

    private void StopDrone(string reason)
    {
        if (!droneArmed)
            return;

        droneArmed = false;
        currentForwardSpeed = 0f;

        if (logStateChanges)
            Debug.Log("[Hand Drone] Drone stopped. Reason: " + reason);
    }

    // ----------------------------------------------------------------------
    // These methods are called by your Static Hand Gesture UnityEvents.
    // ----------------------------------------------------------------------

    public void OnLeftFistPerformed()
    {
        leftFist = true;

        if (logStateChanges)
            Debug.Log("[Hand Drone] Left fist active.");

        TryArmDrone();
    }

    public void OnLeftFistEnded()
    {
        leftFist = false;

        if (logStateChanges)
            Debug.Log("[Hand Drone] Left fist released.");

        StopDrone("Left fist released.");
    }

    public void OnRightFistPerformed()
    {
        rightFist = true;

        if (logStateChanges)
            Debug.Log("[Hand Drone] Right fist active.");

        TryArmDrone();
    }

    public void OnRightFistEnded()
    {
        rightFist = false;

        if (logStateChanges)
            Debug.Log("[Hand Drone] Right fist released.");

        StopDrone("Right fist released.");
    }

    // Optional manual reset, useful for testing.
    public void ResetDefaultHandPosition()
    {
        if (!leftFist || !rightFist)
        {
            Debug.LogWarning("[Hand Drone] Cannot reset default position unless both fists are active.");
            return;
        }

        if (!TryGetPalmWorldPositions(out Vector3 leftPalmWorld, out Vector3 rightPalmWorld))
        {
            Debug.LogWarning("[Hand Drone] Cannot reset default position because palm positions are unavailable.");
            return;
        }

        defaultLeftHandHeadLocal = headCamera.InverseTransformPoint(leftPalmWorld);
        defaultRightHandHeadLocal = headCamera.InverseTransformPoint(rightPalmWorld);
        defaultAverageHandHeadLocal = (defaultLeftHandHeadLocal + defaultRightHandHeadLocal) * 0.5f;

        Debug.Log("[Hand Drone] Default hand position reset.");
    }
}