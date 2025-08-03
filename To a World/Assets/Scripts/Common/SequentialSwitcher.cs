using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class SequentialSwitcher<T> : MonoBehaviour, ISwitcher
{
    [SerializeField] protected List<T> _elements;

    protected int _current = 0;

    protected virtual void Start()
    {
        Assert.IsTrue(_elements.Count > 0);
        for (int i = 0; i < _elements.Count; i++)
        {
            if (i == _current)
            {
                ActiveElement(i);
            }
            else
            {
                InactiveElement(i);
            }
        }
    }

    public void Switch()
    {
        InactiveElement(_current);
        _current = (_current + 1) % _elements.Count;
        ActiveElement(_current);
    }

    protected abstract void ActiveElement(int index);
    protected abstract void InactiveElement(int index);
}
