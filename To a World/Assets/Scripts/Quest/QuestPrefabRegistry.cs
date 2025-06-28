using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestStepPrefabRegistryEntity
{
    [field: SerializeField] public EQuestType Type { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
}

public class QuestPrefabRegistry: MonoBehaviour, IRegistry<EQuestType, GameObject>
{
    public static QuestPrefabRegistry Instance { get; private set; }
    
    [SerializeField] private List<QuestStepPrefabRegistryEntity> _initEntity;
    
    private Dictionary<EQuestType, GameObject> _container;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
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
