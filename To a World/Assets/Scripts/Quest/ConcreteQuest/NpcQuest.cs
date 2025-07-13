using UnityEngine;

public class NpcQuest : AQuest
{
    public string CompletionCondition { get; private set; }

    private void OnEnable()
    {
        // Test 용
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged += OnQuestCompleted;
        NPCChatSystem.NPCChatManager.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDisable()
    {
        if (NPCChatSystem.NPCChatManager == null) return;
        
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged -= OnQuestCompleted;
        NPCChatSystem.NPCChatManager.OnQuestCompleted -= OnQuestCompleted;
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as NpcQuestParams;
        CompletionCondition = param.CompletionCondition;
    }
    
    // 테스트 용
    private void OnQuestCompleted(bool process)
    {
        if (!process)
        {
            CompleteQuest();
        }
    }

    private void OnQuestCompleted(string questId)
    {
        if (Info.ID == questId)
        {
            CompleteQuest();
        }
    }
}
