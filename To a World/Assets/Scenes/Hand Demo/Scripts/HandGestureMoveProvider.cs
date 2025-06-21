using UnityEngine;

public class HandGestureMoveProvider : MonoBehaviour
{
    [SerializeField] private HandGestureJoystick _handGestureJoystickInput;
    [SerializeField] CharacterController _characterController;
    [SerializeField] private float _maxSpeed = 5f;
    
    public void FixedUpdate()
    {
        _characterController.Move(_maxSpeed * _handGestureJoystickInput.Value * Time.deltaTime);
    }
}
