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
    }

    void ResetToLastCheckpoint()
    {
        transform.position = lastCheckpointPosition;

        OrientTowardNextCheckpoint();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OrientTowardNextCheckpoint()
    {
        Checkpoint nextCheckpoint = FindCheckpointByID(nextCheckpointID);

        if (nextCheckpoint == null) return;

        Vector3 direction = (nextCheckpoint.transform.position - transform.position).normalized;

        // if 0, does not tilt drone vertically; remove to add vertical tilting
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    Checkpoint FindCheckpointByID(int id)
    {
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();

        foreach (Checkpoint cp in checkpoints)
        {
            if (cp.checkpointID == id)
                return cp;
        }

        return null;
    }
}