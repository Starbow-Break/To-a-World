using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : ASingletonRegistry<string, GameObject>
{
    [SerializeField] private List<ItemData> _items;
    
    private bool _initialized = false;

    private void Initialize()
    {
        foreach (var entity in _items)
        {
            Register(entity.ID, entity.Prefab);
        }

        _initialized = true;
    }

    public override GameObject Get(string itemId)
    {
        if (!_initialized)
        {
            Initialize();
        }

        return base.Get(itemId);
    }
}