using System.Collections.Generic;
using UnityEngine;

public abstract class ASingletonRegistry<K, V> : NullSafeSceneSingleton<ASingletonRegistry<K, V>>, IRegistry<K, V>
{
    protected Dictionary<K, V> _container = new();

    public virtual void Register(K id, V value)
    {
        if (!_container.TryAdd(id, value))
        {
            Debug.LogWarning($"Duplicate ID : {id}");
        }
    }

    public virtual void UnRegister(K id)
    {
        if (!_container.Remove(id))
        {
            Debug.LogWarning($"Object With ID {id} does not exist");
        }
    }

    public virtual V Get(K id)
    {
        if (_container.TryGetValue(id, out var value))
        {
            return value;
        }
        
        return default(V);
    }
}
