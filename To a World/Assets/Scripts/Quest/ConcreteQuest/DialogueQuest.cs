using UnityEngine;

public class DialogueQuest : AQuest
{
    public string CompletionCondition { get; private set; }
    public string CompletionDialogue { get; private set; }
    
    private void OnEnable()
    {
        NPCChatSystem.NPCChatManager.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDisable()
    {
        var npcChatMgr = NPCChatSystem.NPCChatManager;
        if (npcChatMgr != null)
        {
            npcChatMgr.OnQuestCompleted -= OnQuestCompleted;
        }
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as DialogueQuestParams;
        CompletionCondition = param.CompletionCondition;
        CompletionDialogue = param.CompletionDialogue;
    }

    private void OnQuestCompleted(string questId)
    {
        if (Info.ID == questId)
        {
            CompleteQuest();
        }
    }
}
