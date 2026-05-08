using UnityEngine;

public class FollowCameraRotationVisual : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform headCamera;

    [Header("Rotation Options")]
    [SerializeField] private bool followYaw = true;
    [SerializeField] private bool followPitch = true;
    [SerializeField] private bool followRoll = false;

    [Tooltip("Extra rotation offset if your model faces the wrong direction.")]
    [SerializeField] private Vector3 localEulerOffset = Vector3.zero;

    private void Awake()
    {
        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (headCamera == null)
            return;

        Vector3 cameraEuler = headCamera.rotation.eulerAngles;

        float x = followPitch ? cameraEuler.x : transform.rotation.eulerAngles.x;
        float y = followYaw ? cameraEuler.y : transform.rotation.eulerAngles.y;
        float z = followRoll ? cameraEuler.z : transform.rotation.eulerAngles.z;

        Quaternion targetRotation = Quaternion.Euler(x, y, z) * Quaternion.Euler(localEulerOffset);

        transform.rotation = targetRotation;
    }
}