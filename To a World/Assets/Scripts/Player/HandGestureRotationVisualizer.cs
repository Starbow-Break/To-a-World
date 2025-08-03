using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandGestureRotationVisualizer : MonoBehaviour, IVisualizer<HandGestureRotation>
{
    [SerializeField] private HandGestureRotation _rotation;
    
    [Header("Render")]
    [SerializeField] private List<Renderer> _renderers;
    [SerializeField] private Transform _target;

    [SerializeField] private ATweenAnimator _leftArrowAnim;
    [SerializeField] private ATweenAnimator _rightArrowAnim;
    
    private void OnEnable()
    {
        _rotation.OnStartInput += Show;
        _rotation.OnStopInput += Hide;
    }
    
    private void OnDisable()
    {
        _rotation.OnStartInput -= Show;
        _rotation.OnStopInput -= Hide;
    }

    private void Start()
    {
        SetEnableAllRenderers(_rotation.isActiveAndEnabled);
    }

    private void Update()
    {
        UpdateVisual();
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
        if (_rotation.TryReadValue(out var value))
        {
            if (value == RotationDirection.Left)
            {
                _leftArrowAnim.Play();
            }
            else if (value == RotationDirection.Right)
            {
                _rightArrowAnim.Play();
            }
        }
    }
}
