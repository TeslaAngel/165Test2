using UnityEngine;

public class CameraToggle : MonoBehaviour
{
  public Transform mainCamera;
  public Canvas canvas;
  public Transform firstPersonAnchor;
  public Transform thirdPersonAnchor;
  public Transform firstPersonCanvasAnchor;
  public Transform thirdPersonCanvasAnchor;
  public GameObject firstPersonObject;
  public GameObject thirdPersonObject;

  private bool firstPersonActive = true;

  void Start()
  {
    if (mainCamera == null)
    {
      Camera cam = Camera.main;

      if (cam != null)
        mainCamera = cam.transform;
    }

    SetCameraAnchor(firstPersonAnchor);
    SetCanvasAnchor(firstPersonCanvasAnchor);
    firstPersonObject.gameObject.SetActive(true);
    thirdPersonObject.gameObject.SetActive(false);
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.C))
    {
      firstPersonActive = !firstPersonActive;

      if (firstPersonActive)
      {
        SetCameraAnchor(firstPersonAnchor);
        SetCanvasAnchor(firstPersonCanvasAnchor);
        firstPersonObject.gameObject.SetActive(true);
        thirdPersonObject.gameObject.SetActive(false);
      }
      else
      {
        SetCameraAnchor(thirdPersonAnchor);
        SetCanvasAnchor(thirdPersonCanvasAnchor);
        firstPersonObject.gameObject.SetActive(false);
        thirdPersonObject.gameObject.SetActive(true);
      }
    }
  }

  void SetCameraAnchor(Transform anchor)
  {
    if (mainCamera == null || anchor == null)
    {
      Debug.LogWarning("Missing camera or anchor reference");
      return;
    }

    mainCamera.SetParent(anchor);

    mainCamera.localPosition = Vector3.zero;
    mainCamera.localRotation = Quaternion.identity;
  }
  void SetCanvasAnchor(Transform anchor)
    {
        if (canvas == null || anchor == null)
        {
            Debug.LogWarning("Missing canvas or anchor reference");
            return;
        }

        Transform canvasTransform = canvas.transform;

        canvasTransform.SetParent(anchor, false);

        canvasTransform.localPosition = Vector3.zero;
        canvasTransform.localRotation = Quaternion.identity;

        canvasTransform.localScale = Vector3.one * 0.001f;
    }
}