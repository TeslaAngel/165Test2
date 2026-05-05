using UnityEngine;
using System.Collections.Generic;

public class CheckpointLoader : MonoBehaviour
{
    public string fileName = "checkpoints";
    public GameObject checkpointPrefab;

    void Awake()
    {
        LoadCheckpoints();
    }

    void LoadCheckpoints()
    {
        TextAsset file = Resources.Load<TextAsset>(fileName);

        if (file == null)
        {
            Debug.LogError("File not found in Resources!");
            return;
        }

        string[] lines = file.text.Split('\n');

        int checkpointID = 1;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Trim().Split(' ');

            if (parts.Length < 3) continue;

            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);

            Vector3 position = new Vector3(x, y, z) * 0.0254f; // scaled to unity

            GameObject checkpointObj = Instantiate(checkpointPrefab, position, Quaternion.identity);

            Checkpoint cp = checkpointObj.GetComponent<Checkpoint>();
            cp.checkpointID = checkpointID;

            checkpointID++;
        }

        Debug.Log("Loaded " + (checkpointID - 1) + " checkpoints.");
    }
}