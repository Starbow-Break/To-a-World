using UnityEngine;

public class NpcSpeechBubbleSetter : MonoBehaviour
{
    [SerializeField] private SpeechBubbleUpdater _speechBubbleUpdater;

    private Npc _ownerNpc;
    private bool _isWorking = false;

    private void Awake()
    {
        _ownerNpc = GetComponentInParent<Npc>();
    }
    
    private void Start()
    {
        _speechBubbleUpdater.Initialize();
    }
    
    private void OnEnable()
    {
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged += ProcessingStateChange;

        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc += OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc += OnExitedNpc;
    }

    private void OnDisable() {
        var npcChatManager = NPCChatSystem.NPCChatManager;
        if (npcChatManager != null)
        {
            npcChatManager.OnProcessingStateChanged -= ProcessingStateChange;
        }
        
        var npcEvents = GameEventsManager.GetEvents<NpcEvents>();
        if (npcEvents != null)
        {
            GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc -= OnEnteredNpc;
            GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc -= OnExitedNpc;
        }
    }

    private void ProcessingStateChange(bool state)
    {
        if (state)
        {
            if (_isWorking)
            {
                _speechBubbleUpdater.Active();    
            }
        }
        else
        {
            _speechBubbleUpdater.InActive();
        }
        
    }

    private void OnEnteredNpc(Npc npc)
    {
        bool value = npc == _ownerNpc;
        _isWorking = value;
        Debug.Log(value);
    }

    private void OnExitedNpc(Npc npc)
    {
        _isWorking = false;
    }
}
