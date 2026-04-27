using UnityEngine;

public class SpawnMenu : MonoBehaviour
{
    [Header("Spawn References")]
    [SerializeField] private Transform rightHandAnchor;

    [Header("Prefabs")]
    [SerializeField] private GameObject option1Prefab;
    [SerializeField] private GameObject option2Prefab;
    [SerializeField] private GameObject option3Prefab;

    [Header("Spawn Settings")]
    [SerializeField] private bool parentToHand = false;
    [SerializeField] private Vector3 localSpawnOffset = Vector3.zero;

    public void SpawnOption1()
    {
        Spawn(option1Prefab);
    }

    public void SpawnOption2()
    {
        Spawn(option2Prefab);
    }

    public void SpawnOption3()
    {
        Spawn(option3Prefab);
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null || rightHandAnchor == null)
        {
            Debug.LogWarning("VRSpawnMenu: Missing prefab or rightHandAnchor.");
            return;
        }

        Vector3 spawnPos = rightHandAnchor.TransformPoint(localSpawnOffset);
        Quaternion spawnRot = rightHandAnchor.rotation;

        GameObject spawned = Instantiate(prefab, spawnPos, spawnRot);

        if (parentToHand)
        {
            spawned.transform.SetParent(rightHandAnchor, true);
        }

        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}