using UnityEngine;

public class MyGrabbable : MonoBehaviour
{
    public SelectableVisual visual;
    public PushPullObject PPO;
    public TwoHandScale THS;
    [SerializeField] private Rigidbody rb;

    private int hoverCount = 0;
    private int selectCount = 0;

    public Rigidbody Rigidbody => rb;

    private void Awake()
    {
        if (visual == null)
            visual = GetComponentInChildren<SelectableVisual>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    public void HoverEnter()
    {
        hoverCount++;
        visual?.HandleHover();
    }

    public void HoverExit()
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);

        if (hoverCount == 0)
            visual?.HandleUnhover();
    }

    public void SelectEnter(Transform lh, Transform rh, Transform disRef)
    {
        selectCount++;
        visual?.HandleSelect();
        PPO?.HandleSelect(disRef);
        THS?.HandleSelect(lh, rh);
    }

    public void SelectExit()
    {
        selectCount = Mathf.Max(0, selectCount - 1);

        if (selectCount == 0)
        {
            visual?.HandleUnselect();
            PPO?.HandleUnselect();
            THS?.HandleUnselect();
        }
    }
}