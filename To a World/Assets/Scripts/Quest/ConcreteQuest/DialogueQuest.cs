using UnityEngine;

public class DialogueQuest : AQuest
{
    public string CompletionCondition { get; private set; }

    private void OnEnable()
    {
        // Test 용
        NPCChatSystem.NPCChatManager.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDisable()
    {
        if (NPCChatSystem.NPCChatManager == null) return;
        
        NPCChatSystem.NPCChatManager.OnQuestCompleted -= OnQuestCompleted;
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as DialogueQuestParams;
        CompletionCondition = param.CompletionCondition;
    }

    private void OnQuestCompleted(string questId)
    {
        if (Info.ID == questId)
        {
            CompleteQuest();
        }
    }
}
