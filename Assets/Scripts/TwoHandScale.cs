using UnityEngine;

public class TwoHandScale : MonoBehaviour
{
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 3.0f;

    private bool _leftSelecting;
    private bool _rightSelecting;
    private bool _isScaling;

    private float _initialHandDistance;
    private Vector3 _initialScale;

    public void LeftSelect()
    {
        _leftSelecting = true;
        TryBeginScaling();
    }

    public void LeftUnselect()
    {
        _leftSelecting = false;
        _isScaling = false;
    }

    public void RightSelect()
    {
        _rightSelecting = true;
        TryBeginScaling();
    }

    public void RightUnselect()
    {
        _rightSelecting = false;
        _isScaling = false;
    }

    private void TryBeginScaling()
    {
        if (_leftSelecting && _rightSelecting && leftHand != null && rightHand != null)
        {
            _initialHandDistance = Vector3.Distance(leftHand.position, rightHand.position);
            _initialScale = transform.localScale;
            _isScaling = _initialHandDistance > 0.0001f;
        }
    }

    private void Update()
    {
        if (!_isScaling || leftHand == null || rightHand == null) return;

        float currentDistance = Vector3.Distance(leftHand.position, rightHand.position);
        if (_initialHandDistance <= 0.0001f) return;

        float ratio = currentDistance / _initialHandDistance;
        float uniform = Mathf.Clamp(_initialScale.x * ratio, minScale, maxScale);
        transform.localScale = Vector3.one * uniform;
    }
}