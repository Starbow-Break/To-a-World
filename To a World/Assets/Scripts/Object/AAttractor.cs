using System.Collections.Generic;
using UnityEngine;

public abstract class AAttractor : MonoBehaviour
{
    [SerializeField] protected List<GameObject> _initTargetObjects;

    protected HashSet<GameObject> _targetObjects = new();

    protected virtual void Awake()
    {
        foreach (var obj in _initTargetObjects)
        {
            _targetObjects.Add(obj);
        }
    }

    protected virtual void Update()
    {
        foreach (var obj in _targetObjects)
        {
            MoveObjectPerFrame(obj);
        }
    }

    protected abstract void MoveObjectPerFrame(GameObject obj);
    
    public virtual void AddTarget(GameObject obj)
    {
        _targetObjects.Add(obj);
    }

    public virtual void RemoveTarget(GameObject obj)
    {
        _targetObjects.Remove(obj);
    }

    public virtual void RemoveAllTargets()
    {
        _targetObjects.Clear();
    }
}
