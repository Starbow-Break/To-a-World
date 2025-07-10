using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestPrefabRegistryEntity
{
    [field: SerializeField] public EQuestType Type { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
}

[DefaultExecutionOrder(-1000)]
public class QuestPrefabRegistry: SceneSingleton<QuestPrefabRegistry>, IRegistry<EQuestType, GameObject>
{
    [SerializeField] private List<QuestPrefabRegistryEntity> _initEntity;
    
    private Dictionary<EQuestType, GameObject> _container;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        _container = new();
        foreach (var entity in _initEntity)
        {
            Register(entity.Type, entity.Prefab);
        }
    }
    
    public void Register(EQuestType type, GameObject prefab)
    {
        if (!_container.TryAdd(type, prefab))
        {
            Debug.LogWarning($"Duplicate Type : {type}");
        }
    }

    public void UnRegister(EQuestType type)
    {
        if (!_container.Remove(type))
        {
            Debug.LogWarning($"Object With Type {type} does not exist");
        }
    }

    public GameObject Get(EQuestType type)
    {
        return _container[type];
    }
}
