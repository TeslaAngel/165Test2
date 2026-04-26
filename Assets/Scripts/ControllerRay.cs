using UnityEngine;

public class ControllerRay: MonoBehaviour
{
  public float rayDistance = 100f;
  private LineRenderer line;

  void Start()
  {
    line = GetComponent<LineRenderer>();
  }
  void Update()
  {
    Ray ray = new Ray(transform.position, transform.forward);
    RaycastHit hit;

    line.SetPosition(0, ray.origin);

    if (Physics.Raycast(ray, out hit, rayDistance))
    {
        line.SetPosition(1, hit.point);
    }
    else
    {
        line.SetPosition(1, ray.origin + ray.direction * rayDistance);
    }
  }
}