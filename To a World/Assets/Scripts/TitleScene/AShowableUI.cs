using System;
using UnityEngine;

public abstract class AShowableUI : MonoBehaviour
{
    public event Action ActOnClose;
    public abstract void Show();

    protected virtual void Close()
    {
        ActOnClose?.Invoke();
    }
}
