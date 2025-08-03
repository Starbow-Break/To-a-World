using System;
using System.Collections.Generic;
using UnityEngine;

public class HandGestureMovementVisualizer : MonoBehaviour, IVisualizer<HandGestureMovement>
{
    [SerializeField] private HandGestureMovement _movement;
    
    [Header("Render")]
    [SerializeField] private List<Renderer> _renderers;
    [SerializeField] private LineRenderer _line;
    [SerializeField] private Transform _target; 
    
    private void OnEnable()
    {
        _movement.OnStartInput += Show;
        _movement.OnStopInput += Hide;
    }
    
    private void OnDisable()
    {
        _movement.OnStartInput -= Show;
        _movement.OnStopInput -= Hide;
    }

    private void Start()
    {
        SetEnableAllRenderers(_movement.isActiveAndEnabled);
    }

    private void Update()
    {
        if (isActiveAndEnabled)
        {
            UpdateVisual();    
        }
    }

    private void Show()
    {
        SetEnableAllRenderers(true);
        transform.position = _target.position;
    }

    private void Hide()
    {
        SetEnableAllRenderers(false);
    }

    private void SetEnableAllRenderers(bool enable)
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = enable;
        }
    }

    public void UpdateVisual()
    {
        if (_movement.TryReadValue(out var value))
        {
            Quaternion invLineLocalRot = Quaternion.Inverse(_line.transform.localRotation);
            _line.SetPositions(new Vector3[]
            {
                invLineLocalRot * new Vector3(value.x, 0f, value.y),
                invLineLocalRot * new Vector3(0f, 0f, 0f)
            });
        
            float valueMag = value.magnitude;
            if (!Mathf.Approximately(valueMag, 0f))
            {
                _line.textureScale = new Vector3(-valueMag / _line.startWidth, 1f, 1f);
            }
        }
    }        
}
