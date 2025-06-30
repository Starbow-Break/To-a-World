using System;
using System.Collections.Generic;
using UnityEngine;

public class HandGestureJoystickVisualizer : MonoBehaviour, IVisualizer<HandGestureJoystick>
{
    [SerializeField] private HandGestureJoystick _joystick;
    
    [Header("Render")]
    [SerializeField] private List<Renderer> _renderers;
    [SerializeField] private LineRenderer _line;
    
    private void OnEnable()
    {
        _joystick.OnStartInput += Show;
        _joystick.OnStopInput += Hide;
    }
    
    private void OnDisable()
    {
        _joystick.OnStartInput -= Show;
        _joystick.OnStopInput -= Hide;
    }

    private void Start()
    {
        SetEnableAllRenderers(_joystick.isActiveAndEnabled);
    }

    private void Update()
    {
        if (isActiveAndEnabled)
        {
            UpdateVisual();    
        }
    }

    private void Show() => SetEnableAllRenderers(true);
    private void Hide() => SetEnableAllRenderers(false);

    private void SetEnableAllRenderers(bool enable)
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = enable;
        }
    }

    public void UpdateVisual()
    {
        transform.position = _joystick.Base.position;
        transform.rotation = _joystick.Base.rotation;
        
        Vector2 inputValue = _joystick.Value;
        Quaternion invLineRotation = Quaternion.Inverse(_line.transform.rotation);
        _line.SetPositions(new Vector3[]
        {
            invLineRotation * new Vector3(inputValue.x, 0f, inputValue.y),
            invLineRotation * new Vector3(0f, 0f, 0f)
        });
        
        float valueMagnitude = _joystick.Value.magnitude;
        if (!Mathf.Approximately(valueMagnitude, 0f))
        {
            _line.textureScale = new Vector3(-valueMagnitude / _line.startWidth, 1f, 1f);
        }
    }        
}
