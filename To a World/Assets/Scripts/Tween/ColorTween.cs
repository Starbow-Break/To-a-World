using DG.Tweening;
using UnityEngine;

public class ColorTween: ATweenAnimator
{
    [Header("General")]
    [SerializeField] private Color _from = Color.white;
    [SerializeField] private Color _to = Color.white;
    [SerializeField] private float _duration;
    
    private Material _material;
    
    private void Awake()
    {
        var renderer = GetComponent<Renderer>();
        _material = renderer.material;
    }

    private void Start()
    {
        _tweener = _material.DOColor(_to, _duration)
                            .SetAutoKill(false)
                            .Pause();
    }
    
    public override void Play()
    {
        _material.color = _from;
        _tweener.Restart();
    }
}
