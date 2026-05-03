using UnityEngine;

public class Drone : MonoBehaviour
{
    private int nextCheckpointID = 1;
    private Vector3 lastCheckpointPosition;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastCheckpointPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();

        if (checkpoint == null) return;

        HandleCheckpoint(checkpoint);
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
        // Incorrect checkpoint (ahead of sequence)
        else if (checkpoint.checkpointID > nextCheckpointID)
        {
            Debug.Log("Wrong checkpoint! Resetting...");

            ResetToLastCheckpoint();
        }
        // Ignore already completed checkpoints
    }

    void ResetToLastCheckpoint()
    {
        transform.position = lastCheckpointPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}