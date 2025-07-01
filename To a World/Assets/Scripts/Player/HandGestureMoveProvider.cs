using UnityEngine;
using UnityEngine.Assertions;

public class HandMoveProvider : MonoBehaviour
{
    [SerializeField] CharacterController _characterController;
    [SerializeField] private ABaseInput<Vector2> _moveHandInput;
    [SerializeField] private float _maxSpeed = 5f;
    
    public void FixedUpdate()
    {
        Assert.IsNotNull(_characterController, "Character controller is null");
        
        if (_moveHandInput?.isActiveAndEnabled == true)
        {
            Vector2 value = _moveHandInput.Value;
            Transform charaTransform = _characterController.transform;
            Vector3 velocity = _maxSpeed * (value.x * charaTransform.right + value.y * charaTransform.forward);
            _characterController.Move(Time.deltaTime * velocity);
        }
    }
}