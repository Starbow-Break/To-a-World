using UnityEngine;
using UnityEngine.Assertions;

public class HandGestureRotationProvider : MonoBehaviour
{
    [SerializeField] CharacterController _characterController;
    [SerializeField] private ABaseInput<ERotationDirection> _rotationHandInput;
    [SerializeField] private float _angle = 60f;
    
    public void FixedUpdate()
    {
        Assert.IsNotNull(_characterController, "Character controller is null");

        var value = _rotationHandInput.Value;
        switch (value)
        {
            case ERotationDirection.LEFT:
                _characterController.transform.Rotate(0, -_angle, 0);
                break;
            case ERotationDirection.RIGHT:
                _characterController.transform.Rotate(0, _angle, 0);
                break;
            default:
                break;
        }
    }
}