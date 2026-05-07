using UnityEngine;

public class SelectableVisual : MonoBehaviour
{
    [SerializeField] private Renderer[] renderersToTint;

    private Color normalColor = Color.white;
    private Color hoverColor = Color.green;
    private Color selectedColor = Color.yellow;

    private bool isHovered = false;
    private bool isSelected = false;

    private void Awake()
    {
        if (renderersToTint == null || renderersToTint.Length == 0)
            renderersToTint = GetComponentsInChildren<Renderer>(true);

        ApplyColor(normalColor);
    }

    public void HandleHover() {isHovered = true; UpdateColor(); }
    public void HandleUnhover() {isHovered = false; UpdateColor(); }
    public void HandleSelect() {isSelected = true; UpdateColor(); }
    public void HandleUnselect() {isSelected = false; UpdateColor(); }

    private void UpdateColor()
    {
        if (isSelected)
            ApplyColor(selectedColor);
        else if (isHovered)
            ApplyColor(hoverColor);
        else
            ApplyColor(normalColor);
    }

    private void ApplyColor(Color c)
    {
        foreach (Renderer r in renderersToTint)
        {
            if (r == null) continue;

            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
                else if (mat.HasProperty("_Color"))
                    mat.color = c;
            }
        }
    }
}