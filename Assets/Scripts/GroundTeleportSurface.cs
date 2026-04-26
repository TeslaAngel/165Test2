using UnityEngine;

public class GroundTeleportSurface : MonoBehaviour
{
    [Header("References")]
    public Transform rayOrigin;
    public LineRenderer rayVisual;
    public Transform xrRigRoot;
    public GameObject circlePrefab;

    [Header("Settings")]
    public float rayDistance = 100f;
    public Color defaultRayColor = Color.white;
    public Color validRayColor = Color.blue;

    private GameObject currentCircle;
    private Vector3 targetPosition;
    private bool isValidTarget = false;

    void Start()
    {
        if (rayOrigin == null)
            rayOrigin = GameObject.Find("RightHandAnchor").transform;
        if (xrRigRoot == null)
            xrRigRoot = GameObject.Find("OVRCameraRig").transform;
    }
    void Update()
    {
      CheckRayHit();
      if (isValidTarget && OVRInput.GetDown(OVRInput.Button.Two)) // A button (right controller)
      {
          TeleportPlayer();
      }
    }

    void CheckRayHit()
  {
    Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
    RaycastHit hit;

    isValidTarget = false;
    if (Physics.Raycast(ray, out hit, rayDistance))
    {
        if (hit.collider.gameObject == gameObject)
        {
            isValidTarget = true;
            targetPosition = hit.point;

            UpdateCircle(hit);
            SetRayColor(validRayColor);
            return;
        }
    }

    ClearVisuals();
    SetRayColor(defaultRayColor);
  }

  void UpdateCircle(RaycastHit hit)
  {
      if (currentCircle == null)
      {
          currentCircle = Instantiate(circlePrefab);
      }
      currentCircle.SetActive(true);
      currentCircle.transform.position = hit.point + Vector3.up * 0.01f; // Slightly above the ground to avoid z-fighting
      currentCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
  }

  void ClearVisuals()
  {
      if (currentCircle != null)
      {
          currentCircle.SetActive(false);
      }
  }

  void SetRayColor(Color c)
  {
      if (rayVisual != null)
      {
          rayVisual.startColor = c;
          rayVisual.endColor = c;
      }
  }

  void TeleportPlayer()
  {
      if (xrRigRoot != null)
      {
          xrRigRoot.position = targetPosition;
      }
  }
}