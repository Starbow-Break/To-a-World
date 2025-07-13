using UnityEngine;

[CreateAssetMenu(fileName = "NpcQuestParams", 
    menuName = "ScriptableObjects/QuestStepParams/NpcQuestParams", 
    order = 2)]

public class NpcQuestParams : AQuestParams
{
    [field: SerializeField] 
    public string CompletionCondition { get; private set; }
}
