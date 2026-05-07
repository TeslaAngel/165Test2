using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public int checkpointID;

    private void Reset()
    {
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}