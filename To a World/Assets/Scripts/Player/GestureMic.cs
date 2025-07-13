using System.Collections.Generic;
using UnityEngine;

public class GestureMic : MonoBehaviour
{
    public bool IsActive { get; private set; }

    [SerializeField] private StaticHandGesture _handGesture;

    [SerializeField] private List<GameObject> _objects;

    private bool _wakeUpFlag = true;
    private bool _gestureFlag = false;
    private bool _recordingFlag = false;

    private bool _nextAcviveValue => _wakeUpFlag && (_gestureFlag || _recordingFlag);

    private void Start()
    {
        SetActiveMic(false);
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc += EnteredNpc;
        NPCChatSystem.NPCChatManager.OnRecordingStateChanged += SetRecordingFlag;
        
        _handGesture.GesturePerformed.AddListener(SetTrueGestureFlag);
        _handGesture.GestureEnded.AddListener(SetFalseGestureFlag);
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc -= EnteredNpc;
        NPCChatSystem.NPCChatManager.OnRecordingStateChanged -= SetRecordingFlag;

        _handGesture.GesturePerformed.RemoveAllListeners();
        _handGesture.GestureEnded.RemoveAllListeners();
    }

    private void Update()
    {
        if (IsActive != _nextAcviveValue)
        {
            SetActiveMic(_nextAcviveValue);
        }
    }

    public void Sleep() => _wakeUpFlag = false;
    public void WakeUp() => _wakeUpFlag = true;

    private void SetActiveMic(bool active)
    {
        IsActive = active;
        foreach (GameObject obj in _objects)
        {
            obj.SetActive(IsActive);
        }
    }

    private void SetTrueGestureFlag() => _gestureFlag = true;
    private void SetFalseGestureFlag() => _gestureFlag = false;

    private void SetRecordingFlag(bool value) => _recordingFlag = value;

    private void EnteredNpc(Npc npc)
    {
        var ttsNpcInfo = new NPCSystem.NPCInfo();
        ttsNpcInfo.name = npc.Info.Name;
        ttsNpcInfo.gender = npc.Info.Gender;
        ttsNpcInfo.personality = npc.Info.Personality;
        ttsNpcInfo.background = npc.Info.Background;
        ttsNpcInfo.age = npc.Info.Age;
        ttsNpcInfo.voice_style = npc.Info.VoiceStyle;
        
        NPCChatSystem.NPCChatManager.SetNPCInfo(ttsNpcInfo);
    }
}