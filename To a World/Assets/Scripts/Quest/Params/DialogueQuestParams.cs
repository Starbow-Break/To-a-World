using UnityEngine;

[CreateAssetMenu(fileName = "DialogueQuestParams", 
    menuName = "ScriptableObjects/QuestStepParams/DialogueQuestParams", 
    order = 2)]

public class DialogueQuestParams : AQuestParams
{
    [field: SerializeField] public string CompletionCondition { get; private set; }
    [field: SerializeField] public string CompletionDialogue { get; private set; }
}
