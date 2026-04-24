using UnityEngine;

public class SelectableVisual : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private Renderer[] renderersToTint;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f); // pale yellow
    [SerializeField] private Color selectedColor = Color.red;

    private bool _isHovered;
    private bool _isSelected;

    private void Awake()
    {
        if (renderersToTint == null || renderersToTint.Length == 0)
        {
            renderersToTint = GetComponentsInChildren<Renderer>(true);
        }

        ApplyCurrentColor();
    }

    public void HandleHover()
    {
        _isHovered = true;
        ApplyCurrentColor();
    }

    public void HandleUnhover()
    {
        _isHovered = false;
        ApplyCurrentColor();
    }

    public void HandleSelect()
    {
        _isSelected = true;
        ApplyCurrentColor();
    }

    public void HandleUnselect()
    {
        _isSelected = false;
        ApplyCurrentColor();
    }

    private void ApplyCurrentColor()
    {
        Color target =
            _isSelected ? selectedColor :
            _isHovered ? hoverColor :
            normalColor;

        foreach (var r in renderersToTint)
        {
            if (r == null) continue;

            var materials = r.materials;
            foreach (var mat in materials)
            {
                if (mat != null && mat.HasProperty("_Color"))
                {
                    mat.color = target;
                }
            }
        }
    }
}