using System;
using System.Collections.Generic;
using UnityEngine;

public class HandGestureMovementVisualizer : MonoBehaviour, IVisualizer<HandGestureMovement>
{
    [SerializeField] private HandGestureMovement _movement;
    
    [Header("Render")]
    [SerializeField] private List<Renderer> _renderers;
    [SerializeField] private LineRenderer _line;
    
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
        transform.position = _movement.Base.position;
        transform.rotation = _movement.Base.rotation;
        
        Vector2 inputValue = _movement.Value;
        Quaternion invLineLocalRot = Quaternion.Inverse(_line.transform.localRotation);
        _line.SetPositions(new Vector3[]
        {
            invLineLocalRot * new Vector3(inputValue.x, 0f, inputValue.y),
            invLineLocalRot * new Vector3(0f, 0f, 0f)
        });
        
        float valueMagnitude = _movement.Value.magnitude;
        if (!Mathf.Approximately(valueMagnitude, 0f))
        {
            _line.textureScale = new Vector3(-valueMagnitude / _line.startWidth, 1f, 1f);
        }
    }        
}
