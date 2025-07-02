using DG.Tweening;
using UnityEngine;

public abstract class ATweenAnimator: MonoBehaviour
{
    protected Tweener _tweener;
    protected Sequence _sequence;

    public ATweenAnimator SetEase(Ease easeType)
    {
        _tweener.SetEase(easeType);
        return this;
    }

    public abstract void Play();
}
