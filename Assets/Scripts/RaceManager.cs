using UnityEngine;
using System.Linq;

public class RaceManager : MonoBehaviour
{
    private int nextCheckpointID = 1;
    private Vector3 lastCheckpointPosition;

    private Checkpoint[] checkpoints;

    private Drone drone;
    private float raceStartTime;
    private bool raceStarted = false;
    private bool raceFinished = false;
    private Vector3 lastDronePosition;
    public float movementThreshold = 0.01f; // small threshold to ignore jitter

    void Start()
    {
        drone = FindObjectOfType<Drone>();

        checkpoints = FindObjectsOfType<Checkpoint>()
            .OrderBy(cp => cp.checkpointID)
            .ToArray();
        Debug.Log("num checkpoints: " + checkpoints.Length);

        InitializeDronePosition();
    }
    // update function determines when the race starts based on if the drone moves
    void Update()
    {
        if (raceStarted || raceFinished) return;

        Vector3 currentPosition = drone.transform.position;
        float distanceMoved = Vector3.Distance(currentPosition, lastDronePosition);

        if (distanceMoved > movementThreshold)
        {
            StartRace();
        }

        lastDronePosition = currentPosition;
    }
    void InitializeDronePosition()
    {
        if (drone == null || checkpoints.Length < 2)
        {
            Debug.LogError("Cannot initialize drone: missing drone or checkpoints");
            return;
        }

        Checkpoint first = GetCheckpointByID(1);
        Checkpoint second = GetCheckpointByID(2);

        if (first == null || second == null)
        {
            Debug.LogError("Missing checkpoint 1 or 2");
            return;
        }
        // place drone at first checkpoint and face second checkpoint
        Vector3 spawnPos = first.transform.position;
        drone.transform.position = spawnPos;

        Vector3 direction = (second.transform.position - first.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            drone.transform.rotation = lookRot;
        }

        lastCheckpointPosition = spawnPos;
        lastDronePosition = spawnPos;
    }
    // starts the race timer
    void StartRace()
    {
        raceStarted = true;
        raceStartTime = Time.time;

        Debug.Log("Race started!");
    }
    // called by the drone component each time it hits a checkpoint collider
    public void HandleCheckpoint(Drone drone, Checkpoint checkpoint)
    {
        if (raceFinished) return;

        if (checkpoint.checkpointID == nextCheckpointID)
        {
            Debug.Log("Checkpoint " + checkpoint.checkpointID + " cleared!");

            lastCheckpointPosition = checkpoint.transform.position;
            nextCheckpointID++;

            if (nextCheckpointID > checkpoints.Length)
            {
                FinishRace();
            }
        }
        else if (checkpoint.checkpointID > nextCheckpointID)
        {
            Debug.Log("Wrong checkpoint! Resetting...");
            ResetToLastCheckpoint(drone);
        }
    }

    void FinishRace()
    {
        raceFinished = true;

        float totalTime = Time.time - raceStartTime;
        Debug.Log("Race finished! Time: " + totalTime.ToString("F2") + " seconds");
    }
    // 
    void ResetToLastCheckpoint(Drone drone)
    {
        Quaternion rotation = GetRotationTowardNextCheckpoint(lastCheckpointPosition);
        drone.ResetDrone(lastCheckpointPosition, rotation);

        // Prevent accidental restart trigger after reset
        lastDronePosition = lastCheckpointPosition;
    }
    // make drone face next checkpoint
    Quaternion GetRotationTowardNextCheckpoint(Vector3 position)
    {
        Checkpoint nextCheckpoint = GetCheckpointByID(nextCheckpointID);
        if (nextCheckpoint == null) return Quaternion.identity;

        Vector3 direction = (nextCheckpoint.transform.position - position).normalized;
        direction.y = 0;

        if (direction == Vector3.zero) return Quaternion.identity;

        return Quaternion.LookRotation(direction);
    }

    Checkpoint GetCheckpointByID(int id)
    {
        int index = id - 1;

        if (index < 0 || index >= checkpoints.Length)
            return null;

        return checkpoints[index];
    }
    public Checkpoint GetNextCheckpoint()
  {
    return GetCheckpointByID(nextCheckpointID);
  }
}