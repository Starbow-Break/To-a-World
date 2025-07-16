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
        GameEventsManager.GetEvents<QuestEvents>().OnStartQuest += SetChatManagerQuestData;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnStartQuest -= SetChatManagerQuestData;
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

    private void SetChatManagerQuestData(string questId)
    {
        if (data.TryGetNpcQuest(questId, out DialogueQuest quest))
        {
            var chatQuestInfo = new NPCSystem.QuestInfo();
            chatQuestInfo.id = quest.Info.ID;
            chatQuestInfo.name = quest.Info.Name;
            chatQuestInfo.description = quest.Info.Description;
            chatQuestInfo.completion_condition = quest.CompletionCondition;
            
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
