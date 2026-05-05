using UnityEngine;

public class Drone : MonoBehaviour
{
    private Rigidbody rb;
    private RaceManager raceManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        raceManager = FindObjectOfType<RaceManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint == null) return;

        raceManager.HandleCheckpoint(this, checkpoint);
    }

    public void ResetDrone(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}