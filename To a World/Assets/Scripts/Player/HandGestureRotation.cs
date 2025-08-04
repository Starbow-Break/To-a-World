using UnityEngine;

public static class RotationDirection
{
    public static readonly Vector2 None = Vector2.zero;
    public static readonly Vector2 Left = Vector2.left;
    public static readonly Vector2 Right = Vector2.right;
}

public class HandGestureRotation: BaseHandXRInput<Vector2>
{
    [Header("General")]
    [field: SerializeField] public Transform Base { get; private set; }
    [SerializeField] private Transform _target;
    [SerializeField] private float _unhandleBorder = 30f;
    [SerializeField] private float _handleBorder = 60f;
    
    private bool _handleRotation;
    private bool _gestureFlag;

    protected virtual void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (!IsWork) return;

        float dotBT = Vector3.Dot(Base.up, _target.up);
        var angle = 90f - Mathf.Acos(dotBT) * Mathf.Rad2Deg;
        if (_handleRotation && Mathf.Abs(angle) >= _handleBorder)
        {
            _handleRotation = false;
            _value = _target.up.y > 0f ? RotationDirection.Left : RotationDirection.Right;
        }
        else if (!_handleRotation && Mathf.Abs(angle) <= _unhandleBorder)
        {
            _handleRotation = true;
            _value = RotationDirection.None;
        }
        else
        {
            _value = RotationDirection.None;
        }
    }
    
    protected override void StartInput()
    {
        Base.position = _target.position;
        base.StartInput();
    }
}
