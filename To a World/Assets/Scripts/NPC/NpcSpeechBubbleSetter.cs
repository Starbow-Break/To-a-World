using TTSSystem;
using UnityEngine;

public class NpcSpeechBubbleSetter : MonoBehaviour
{
    [SerializeField] private SpeechBubbleUpdater _speechBubbleUpdater;
    [SerializeField] private UnityRealtimeTTSClient _ttsPlayer;

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
        _ttsPlayer.OnRequestStarted += TTSPlayerStart;
        _ttsPlayer.OnRequestCompleted += TTSPlayerAllCompleted;
        _ttsPlayer.OnError += TTSPlayerError;

        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc += OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc += OnExitedNpc;
    }

    private void OnDisable()
    {
        _ttsPlayer.OnRequestStarted -= TTSPlayerStart;
        _ttsPlayer.OnRequestCompleted -= TTSPlayerAllCompleted;
        _ttsPlayer.OnError -= TTSPlayerError;
        
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc -= OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc -= OnExitedNpc;
    }

    private void TTSPlayerStart()
    {
        Debug.Log(_isWorking);
        if (_isWorking)
        {
            _speechBubbleUpdater.Active();    
        }
    }

    private void TTSPlayerAllCompleted()
    {
        _speechBubbleUpdater.InActive();
    }

    private void TTSPlayerError(string message)
    {
        _speechBubbleUpdater.InActive();
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
