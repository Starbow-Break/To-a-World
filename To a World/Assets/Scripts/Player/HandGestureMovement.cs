using UnityEngine;
using UnityEngine.Assertions;

public class HandGestureMovement : BaseHandXRInput<Vector2>
{
    #region Fields
    [Header("General")]
    [field: SerializeField] public Transform Base { get; private set; }
    [SerializeField] private Transform _target;
    [SerializeField] private float _sensitivity = 4f;

    private Vector3 _startPosition;
    private bool _gestureFlag;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        TestAssertion();
    }

    protected override void FixedUpdate()
    {
        Debug.Log(IsWork);
        base.FixedUpdate();
        
        if (!IsWork) return;
            
        Vector3 currentDistance = _target.position - Base.position;
        Vector2 newValue = CalculateInputValue(currentDistance);
        _value = newValue;
    }
    #endregion

    #region Methods

    protected override void StartInput()
    {
        Base.position = _target.position;
        base.StartInput();
    }
    
    private Vector2 CalculateInputValue(Vector3 distance)
    {
        Vector3 projDist = Vector3.ProjectOnPlane(distance, Base.up);
        Vector3 standardProjDist = Quaternion.Inverse(Base.rotation) * projDist;

        Vector2 rawValue = new(standardProjDist.x, standardProjDist.z);
        
        float maxMagnitude = 1f / _sensitivity;
        Vector2 value = _sensitivity * (rawValue.sqrMagnitude > maxMagnitude * maxMagnitude 
            ? maxMagnitude * rawValue.normalized
            : rawValue);

        return value;
    }

    private void TestAssertion()
    {
        Assert.IsNotNull(Base, "Field Base is null");
        Assert.IsNotNull(_target, "Field Target is null");
    }
    #endregion
}
