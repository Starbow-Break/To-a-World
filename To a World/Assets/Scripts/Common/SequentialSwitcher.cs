using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class SequentialSwitcher<T> : MonoBehaviour, ISwitcher
{
    [SerializeField] protected List<T> _elements;

    protected int _current = 0;
    protected bool _isActive = false;

    protected virtual void Start()
    {
        Assert.IsTrue(_elements.Count > 0);
        for (int i = 0; i < _elements.Count; i++)
        {
            if (i == _current && _isActive)
            {
                ActiveElement(i);
            }
            else
            {
                InactiveElement(i);
            }
        }
    }

    public virtual void ActiveSwitcher()
    {
        _isActive = true;
        ActiveElement(_current);
    }
    
    public virtual void InactiveSwitcher()
    {
        _isActive = false;
        InactiveElement(_current);
    }

    public void Switch()
    {
        if (_isActive)
        {
            InactiveElement(_current);
        }
        
        _current = (_current + 1) % _elements.Count;

        if (_isActive)
        {
            ActiveElement(_current);
        }
    }

    protected abstract void ActiveElement(int index);
    protected abstract void InactiveElement(int index);
}
