using NPCSystem;
using UnityEngine;

public class Npc : MonoBehaviour
{
    [field: SerializeField] public NpcData data { get; private set; }
    [SerializeField] private Collider _interactionCollider;
    
    private void Awake()
    {
        if (_interactionCollider == null)
        {
            _interactionCollider = GetComponentInChildren<Collider>();

            if (_interactionCollider == null)
            {
                Debug.LogWarning("No Collider attached to Npc. Npc can't interact player.");
            }
        }
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += SetChatManagerQuestData;
    }

    private void OnDisable()
    {
        if (GameEventsManager.TryGetEvents<QuestEvents>(out var questEvents))
        {
            questEvents.OnQuestStateChange -= SetChatManagerQuestData;
        }
    }

    public void Sleep()
    {
        _interactionCollider.enabled = false;
    }

    public void WakeUp()
    {
        _interactionCollider.enabled = true;
    }

    public void SpeakText(string text)
    { 
        NPCChatSystem.NPCChatManager.SendTextToNPC(text);
    }

    private void SetChatManagerQuestData(AQuest quest)
    {
        if(quest is DialogueQuest dialogueQuest && data.IsNpcQuest(quest.Info.ID))
        {
            var chatQuestInfo = new NPCSystem.QuestInfo();
            chatQuestInfo.id = dialogueQuest.Info.ID;
            chatQuestInfo.name = dialogueQuest.Info.Name;
            chatQuestInfo.description = dialogueQuest.Info.Description;
            chatQuestInfo.completion_dialogue = dialogueQuest.CompletionDialogue;
            chatQuestInfo.completion_condition = dialogueQuest.CompletionCondition;
            
            NPCChatSystem.NPCChatManager.SetQuestInfo(chatQuestInfo);
        }
    }
    
    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<NpcEvents>().EnteredNpc(this);
        }
    }
    
    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<NpcEvents>().ExitedNpc(this);
        }
    }
}
