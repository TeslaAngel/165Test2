using UnityEngine;

public class ControllerRay : MonoBehaviour
{
    public float rayDistance = 100f;
    private LineRenderer line;

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask interactableLayer;

    [Header("Colors")]
    public Color defaultColor = Color.white;
    public Color groundColor = Color.blue;
    public Color interactableColor = Color.yellow;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Color rayColor = defaultColor;

        line.SetPosition(0, ray.origin);

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            line.SetPosition(1, hit.point);

            if (IsInLayerMask(hit.collider.gameObject, interactableLayer))
            {
                rayColor = interactableColor;
            }
            else if (IsInLayerMask(hit.collider.gameObject, groundLayer))
            {
                rayColor = groundColor;
            }
        }
        else
        {
            line.SetPosition(1, ray.origin + ray.direction * rayDistance);
        }

        ApplyRayColor(rayColor);
    }
    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
    void ApplyRayColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
        line.material.color = color;
    }
}