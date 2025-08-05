using System;
using UnityEngine;

public class ShipMover : MonoBehaviour
{
    [SerializeField] private Transform[] passPoints;
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private float boostSpeed = 8f;
    [SerializeField] private float slowDistance = 2f;
    [SerializeField] private float boostDuration = 3f;

    private bool _isStarted = false;
    private float _currentSpeed = 0f;
    private int _currentIndex = 0;

    public void StartMoving()
    {
        _isStarted = true;
        //_currentSpeed = maxSpeed;
    }
    
    public void ToNextPoint()
    {
        
    }
    
    private void Awake()
    {
        _isStarted = false;
    }
    
    private void Update()
    {
        if(_isStarted == false)
            return;
        if (_currentIndex > passPoints.Length)
            return;

        Transform target = passPoints[_currentIndex];
        Vector3 dir = (target.position - transform.position);
        float distance = dir.magnitude;
        
        
        
        
        // 이동
        //transform.position += dir.normalized * currentSpeed * Time.deltaTime;
        
        
    }
}
