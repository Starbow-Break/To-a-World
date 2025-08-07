using ManagerScene;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] private Vector3 direction = Vector3.forward;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private Transform _endPoint;
    
    private bool isMoving = false;
    
    private void Update()
    {
        if (isMoving == false) 
            return;
        transform.position += direction * (movementSpeed * Time.deltaTime);
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }
}
