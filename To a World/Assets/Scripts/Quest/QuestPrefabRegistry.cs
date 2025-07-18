using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestPrefabRegistryEntity
{
    [field: SerializeField] public EQuestType Type { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
}

[CreateAssetMenu(fileName = "QuestPrefabRegistry", menuName = "ScriptableObjects/QuestPrefabRegistry")]
public class QuestPrefabRegistry: ScriptableObject, IRegistry<EQuestType, GameObject>
{
    [SerializeField] private List<QuestPrefabRegistryEntity> _initEntity;
    
    private Dictionary<EQuestType, GameObject> _container;

    private void Initialize()
    {
        _container = new();
        foreach (var entity in _initEntity)
        {
            if (!_container.TryAdd(entity.Type, entity.Prefab))
            {
                Debug.LogWarning("Duplicate object has same type");
            }
        }
    }

    public GameObject Get(EQuestType type)
    {
        // lazy initialize
        if (_container == null)
        {
            Initialize();
        }
        
        return _container[type];
    }
}
