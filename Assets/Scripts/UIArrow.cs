using UnityEngine;

public class UIArrow : MonoBehaviour
{
    public float distanceFromCamera = 2f; // how far in front of camera
    public Vector3 offset = new Vector3(0, -0.5f, 0); // screen position tweak

    private Transform cam;
    private RaceManager raceManager;

    void Start()
    {
        cam = transform.parent;
        raceManager = FindObjectOfType<RaceManager>();

        if (cam == null)
        {
            Debug.LogError("UIArrow: No main camera found!");
        }

        if (raceManager == null)
        {
            Debug.LogError("UIArrow: No RaceManager found!");
        }
    }

    void LateUpdate()
    {
        if (cam == null || raceManager == null) return;

        // keep arrow fixed relative to camera
        transform.position = cam.position 
            + cam.forward * distanceFromCamera 
            + cam.TransformDirection(offset);

        Checkpoint nextCheckpoint = raceManager.GetNextCheckpoint();
        if (nextCheckpoint == null) return;

        // direction from camera to checkpoint
        Vector3 direction = (nextCheckpoint.transform.position - cam.position).normalized;

        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
    }
}