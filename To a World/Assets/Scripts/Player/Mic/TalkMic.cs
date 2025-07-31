using System.Collections.Generic;
using UnityEngine;

public class TalkMic : MonoBehaviour
{
    public bool IsActive { get; private set; } = true;
    
    [SerializeField] private List<GameObject> _visualObjects;

    [field: SerializeField] public TalkMicButtonSetter ButtonSetter { get; private set; }
    [field: SerializeField] public TalkMicMessageSetter MessageSetter { get; private set; }

    private ATalkMicState _state;
    
    private bool _wakeUpFlag = true;
    private bool _meetNpcFlag;

    private bool _nextAcviveValue => _wakeUpFlag && _meetNpcFlag;

    private void OnEnable()
    {
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc += EnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc += ExitedNpc;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc -= EnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc -= ExitedNpc;
    }

    private void Update()
    {
        if (IsActive != _nextAcviveValue)
        {
            IsActive = _nextAcviveValue;
            SetActive(IsActive);
        }
    }

    public void Sleep() => _wakeUpFlag = false;
    public void WakeUp() => _wakeUpFlag = true;

    public void SetActive(bool active)
    {
        foreach (GameObject obj in _visualObjects)
        {
            obj.SetActive(IsActive);
        }
    }

    #region Event Callbacks
    private void EnteredNpc(Npc npc)
    {
        var ttsNpcInfo = new NPCSystem.NPCInfo();
        ttsNpcInfo.name = npc.data.Name;
        ttsNpcInfo.gender = npc.data.Gender;
        ttsNpcInfo.personality = npc.data.Personality;
        ttsNpcInfo.background = npc.data.Background;
        ttsNpcInfo.age = npc.data.Age;
        ttsNpcInfo.voice_style = npc.data.VoiceStyle;
        
        NPCChatSystem.NPCChatManager.SetNPCInfo(ttsNpcInfo);
        _meetNpcFlag = true;
    }
    
    private void ExitedNpc(Npc npc)
    {
        _meetNpcFlag = false;
    }
    #endregion
}