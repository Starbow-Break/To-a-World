using UnityEngine;

public enum ERotationDirection
{
    NONE,
    LEFT,
    RIGHT
}

public class HandGestureRotation: ABaseInput<ERotationDirection>
{
    [field: SerializeField] public Transform Base { get; private set; }
    [SerializeField] private Transform _target;
    
    [Header("Rotation")]
    [SerializeField] private float _unhandleBorder = 30f;
    [SerializeField] private float _handleBorder = 60f;
    
    private bool _handleRotation = false;

    private void FixedUpdate()
    {
        if (!_isActive)
        {
            Value = ERotationDirection.NONE;
            return;
        }

        float dotBT = Vector3.Dot(Base.up, _target.up);
        var angle = 90f - Mathf.Acos(dotBT) * Mathf.Rad2Deg;
        if (_handleRotation && Mathf.Abs(angle) >= _handleBorder)
        {
            _handleRotation = false;
            Value = _target.up.y > 0f ? ERotationDirection.LEFT : ERotationDirection.RIGHT;
        }
        else if (!_handleRotation && Mathf.Abs(angle) <= _unhandleBorder)
        {
            _handleRotation = true;
            Value = ERotationDirection.NONE;
        }
        else
        {
            Value = ERotationDirection.NONE;
        }
    }
    
    public override void StartInput()
    {
        Base.position = _target.position;
        base.StartInput();
    }
}
