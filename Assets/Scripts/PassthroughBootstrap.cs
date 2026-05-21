using UnityEngine;
public class PassthroughBootstrap : MonoBehaviour
{
    public OVRPassthroughLayer layer; // drag the OVRPassthroughLayer here
    void Start()
    {
        if (!OVRManager.IsInsightPassthroughSupported())
        {
            Debug.LogError("Passthrough not supported"); return;
        }
        OVRManager.instance.isInsightPassthroughEnabled = true;
        layer.enabled = true;
        layer.textureOpacity = 1.0f;
    }
}