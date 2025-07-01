using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandGestureRotationVisualizer : MonoBehaviour, IVisualizer<HandGestureRotation>
{
    [SerializeField] private HandGestureRotation _rotation;
    
    [Header("Render")]
    [SerializeField] private List<Renderer> _renderers;

    [SerializeField] private Renderer _left;
    [SerializeField] private Renderer _right;

    private Tween colorTween;
    
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
        transform.position = _rotation.Base.position;
        transform.rotation = _rotation.Base.rotation;
        
        var value = _rotation.Value;

        Color from = Color.white;
        Color to = new Color(1f, 1f, 1f, 0f);

        switch (value)
        {
            case ERotationDirection.LEFT:
                TweenColor(_left.material, from, to, 0.5f);
                break;
            case ERotationDirection.RIGHT:
                TweenColor(_right.material, from, to, 0.5f);
                break;
            default:
                break;
        }
    }
    
    private void TweenColor(Material material, Color from, Color to, float duration)
    {
        if (colorTween != null)
        {
            colorTween.Complete();
            colorTween = null;
        }
        
        material.color = from;
        colorTween = material.DOColor(to, duration);
    }
}
