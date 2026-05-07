using UnityEngine;

public class DroneTestController : MonoBehaviour
{
    public RaceManager raceManager;
    public float moveSpeed = 10f;
    public float mouseSensitivity = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        // Clamp vertical look
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        if (raceManager != null && raceManager.GetPenalty()) return;
        if (raceManager != null && raceManager.IsCountdownActive()) return;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
}