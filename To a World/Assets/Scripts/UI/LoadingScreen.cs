using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public event Action ActOnFadeInEnd;
    public event Action ActOnFadeOutEnd;
    
    private readonly int _hashFadeIn = Animator.StringToHash("FadeIn");
    private readonly int _hashFadeOut = Animator.StringToHash("FadeOut");
    
    public void FadeIn()
    {
        _animator.SetTrigger(_hashFadeIn);
    }
    
    public void FadeOut()
    {
        _animator.SetTrigger(_hashFadeOut);
    }

    public void OnFadeInEnd()
    {
        ActOnFadeInEnd?.Invoke();
    }
    
    public void OnFadeOutEnd()
    {
        ActOnFadeOutEnd?.Invoke();
    }
}
