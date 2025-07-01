using System.Collections.Specialized;
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
    
    private bool _handleRotation = true;

    private void FixedUpdate()
    {
        if (!_isActive)
        {
            Value = ERotationDirection.NONE;
            return;
        }
        
        var thumbDirection = Vector3.ProjectOnPlane(_target.forward, Base.forward);
        thumbDirection = Quaternion.Inverse(Base.rotation) * thumbDirection;
        var angle = Vector3.Angle(thumbDirection, Vector3.up);
        if (_handleRotation && Mathf.Abs(angle) >= _handleBorder)
        {
            _handleRotation = false;
            Value = thumbDirection.x > 0f ? ERotationDirection.RIGHT : ERotationDirection.LEFT;
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
