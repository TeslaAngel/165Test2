using UnityEngine;
using System.Collections.Generic;

public class Drone : MonoBehaviour
{
    public float detectionRadius = 9.144f; // 30 feet in meters

    private int nextCheckpointID = 1;
    private Vector3 lastCheckpointPosition;

    private List<Checkpoint> allCheckpoints;

    void Start()
    {
        allCheckpoints = new List<Checkpoint>(FindObjectsOfType<Checkpoint>());

        // Sort checkpoints by id
        allCheckpoints.Sort((a, b) => a.checkpointID.CompareTo(b.checkpointID));

        if (allCheckpoints.Count > 0)
        {
            lastCheckpointPosition = transform.position;
        }
    }

    void Update()
    {
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            float distance = Vector3.Distance(transform.position, checkpoint.transform.position);

            if (distance <= detectionRadius)
            {
                HandleCheckpoint(checkpoint);
            }
        }
    }

    void HandleCheckpoint(Checkpoint checkpoint)
    {
        // Correct checkpoint
        if (checkpoint.checkpointID == nextCheckpointID)
        {
            Debug.Log("Checkpoint " + checkpoint.checkpointID + " cleared!");

            lastCheckpointPosition = checkpoint.transform.position;
            nextCheckpointID++;
        }
        // Incorrect checkpoint
        else if (checkpoint.checkpointID > nextCheckpointID)
        {
            Debug.Log("Wrong checkpoint! Resetting...");

            ResetToLastCheckpoint();
        }
        // Ignore already passed checkpoints
    }

    void ResetToLastCheckpoint()
    {
        transform.position = lastCheckpointPosition;

        // Optional: reset velocity if using Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}