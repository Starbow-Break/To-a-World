using UnityEngine;
using UnityEngine.Events;

public class HandGestureJoystick : MonoBehaviour
{
    public UnityEvent OnStartDrag;
    public UnityEvent OnStopDrag;
    public UnityEvent<Vector3> OnValueChanged;
    
    public Vector3 Value { get; private set; } = default;

    [SerializeField] private Transform _baseTransform;
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _sensitivity = 4f;
    
    private Vector3 _startPosition;
    private bool _isDragging = false;

    private void OnEnable()
    {
        if (_targetTransform != null && !_isDragging)
        {
            _baseTransform.position = _targetTransform.position;
            _isDragging = true;
            OnStartDrag?.Invoke();
        }
    }

    public void OnDisable()
    {
        if (_targetTransform != null && _isDragging)
        {
            _isDragging = false;
            TryChangeValue(default);
            OnStopDrag?.Invoke();
        }
    }

    public void Update()
    {
        if (_isDragging)
        {
            Vector3 currentValue = _targetTransform.position - _baseTransform.position;
            TryChangeValue(currentValue);
        }
    }

    private void TryChangeValue(Vector3 newValue)
    {
        Vector3 value = newValue.sqrMagnitude > Mathf.Pow(1f / _sensitivity, 2) ? _sensitivity * newValue.normalized : newValue;
        if (Value != value)
        {
            Value = value;
            OnValueChanged?.Invoke(Value);
        }
    }
}
