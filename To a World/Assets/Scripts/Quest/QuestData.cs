using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData", order = 1)]
public class QuestData : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public EQuestType Type { get; private set; }
    [field: SerializeField] public AQuestParams Param { get; private set; }

    [Header("General")]
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }

    [Header("Requirements")] 
    [field: SerializeField] public QuestData[] questPrerequisites { get; private set; }
    
    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (String.IsNullOrEmpty(ID))
        {
            ID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}
