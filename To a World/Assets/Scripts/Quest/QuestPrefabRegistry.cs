using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestPrefabRegistryEntity
{
    [field: SerializeField] public EQuestType Type { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
}

public class QuestPrefabRegistry: ASingletonRegistry<EQuestType, GameObject>
{
    [SerializeField] private List<QuestPrefabRegistryEntity> _initEntity;
    
    private bool _initialized = false;

    private void Initialize()
    {
        foreach (var entity in _initEntity)
        {
            Register(entity.Type, entity.Prefab);
        }

        _initialized = true;
    }

    public override GameObject Get(EQuestType type)
    {
        if (!_initialized)
        {
            Initialize();
        }

        return base.Get(type);
    }
}
