using UnityEngine;
using System.Collections;
using System.Linq;

public class RaceManager : MonoBehaviour
{
    private int nextCheckpointID = 1;
    private Vector3 lastCheckpointPosition;

    private Checkpoint[] checkpoints;

    private Drone drone;
    private float raceStartTime;
    private float finalTime;
    private bool raceStarted = false;
    private bool raceFinished = false;
    private Vector3 lastDronePosition;
    public float movementThreshold = 0.01f; // small threshold to ignore jitter

    private float countdownTime = 10f;
    private float countdownRemaining;
    private bool countdownActive = true;
    private float penaltyRemaining;
    private bool penalty = false;

    void Start()
    {
        drone = FindObjectOfType<Drone>();

        checkpoints = FindObjectsOfType<Checkpoint>()
            .OrderBy(cp => cp.checkpointID)
            .ToArray();
        Debug.Log("num checkpoints: " + checkpoints.Length);

        InitializeDronePosition();
        countdownRemaining = countdownTime;
    }

    void Update()
    {
        HandleCountdown();
        HandlePenalty();
        if (countdownActive || raceFinished) return;

        Vector3 currentPosition = drone.transform.position;
        lastDronePosition = currentPosition;
    }
    void HandleCountdown()
    {
      if (!countdownActive) return;
      countdownRemaining -= Time.deltaTime;

      if (countdownRemaining <= 0f)
      {
        countdownActive = false;
        StartRace();
      }
    }
    void HandlePenalty()
    {
      if (!penalty) return;
      penaltyRemaining -= Time.deltaTime;
      if (penaltyRemaining <= 0f)
      {
        penalty = false;
      }
    }
    public void StartPenalty()
    {
        penalty = true;
        penaltyRemaining = 3f;
        ResetToLastCheckpoint(drone);

        Checkpoint lastCheckpoint = GetCheckpointByID(nextCheckpointID - 1);
        if (lastCheckpoint != null)
            StartCoroutine(HideCheckpointTemporarily(lastCheckpoint, 4f));
    }

    IEnumerator HideCheckpointTemporarily(Checkpoint checkpoint, float duration)
    {
        Renderer[] renderers = checkpoint.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;

        yield return new WaitForSeconds(duration);

        foreach (var r in renderers) r.enabled = true;
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
    }

    void FinishRace()
    {
        raceFinished = true;

        finalTime = Time.time - raceStartTime;
        Debug.Log("Race finished! Time: " + finalTime.ToString("F2") + " seconds");
    }
    // 
    void ResetToLastCheckpoint(Drone drone)
    {
        Quaternion rotation = GetRotationTowardNextCheckpoint(lastCheckpointPosition);
        drone.ResetDrone(lastCheckpointPosition, rotation);

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
    public bool IsCountdownActive()
    {
      return countdownActive;
    }
    public float GetCountdownTime()
    {
      return countdownRemaining;
    }
    public float GetRaceTime()
    {
      if (!raceStarted) return 0f;
      if (raceFinished)
      {
        return finalTime;
      }
      return Time.time - raceStartTime;
    }
    public bool GetPenalty()
    {
      return penalty;
    }
    public float GetPenaltyTime()
    {
      return penaltyRemaining;
    }
}