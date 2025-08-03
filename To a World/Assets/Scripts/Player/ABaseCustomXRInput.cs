using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public abstract class ABaseCustomXRInput: MonoBehaviour
{
    public event Action OnStartInput;
    public event Action OnStopInput;
    
    public bool Sleeping { get; protected set; }
    public abstract bool IsWork { get; }

    protected bool _lastIsWork;

    protected virtual void FixedUpdate()
    {
        if (_lastIsWork != IsWork)
        {
            if (IsWork)
            {
                StartInput();
            }
            else
            {
                StopInput();
            }
            
            _lastIsWork = IsWork;
        }
    }

    public virtual void Sleep() => Sleeping = true;
    public virtual void WakeUp() => Sleeping = false;
    
    protected virtual void StartInput() => OnStartInput?.Invoke();
    protected virtual void StopInput() => OnStopInput?.Invoke();
}

public abstract class ABaseCustomXRInput<T>: ABaseCustomXRInput, IXRInputValueReader<T> where T : struct
{
    protected T _value;

    public T ReadValue()
    {
        return _value;
    }

    public bool TryReadValue(out T value)
    {
        value = _value;
        return true;
    }

    protected override void StopInput()
    {
        _value = default;
        base.StopInput();
    }
}