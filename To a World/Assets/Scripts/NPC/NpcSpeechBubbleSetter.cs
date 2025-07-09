using TTSSystem;
using UnityEngine;

public class NpcSpeechBubbleSetter : MonoBehaviour
{
    [SerializeField] private SpeechBubbleUpdater _speechBubbleUpdater;
    [SerializeField] private UnityRealtimeTTSClient _ttsPlayer;
    
    private void Start()
    {
        _speechBubbleUpdater.Initialize();
    }
    
    private void OnEnable()
    {
        _ttsPlayer.OnRequestStarted += TTSPlayerStart;
        _ttsPlayer.OnRequestCompleted += TTSPlayerAllCompleted;
        _ttsPlayer.OnError += TTSPlayerError;
    }

    private void OnDisable()
    {
        _ttsPlayer.OnRequestStarted -= TTSPlayerStart;
        _ttsPlayer.OnRequestCompleted -= TTSPlayerAllCompleted;
        _ttsPlayer.OnError -= TTSPlayerError;
    }

    private void TTSPlayerStart()
    {
        _speechBubbleUpdater.Active();
    }

    private void TTSPlayerAllCompleted()
    {
        _speechBubbleUpdater.InActive();
    }

    private void TTSPlayerError(string message)
    {
        _speechBubbleUpdater.InActive();
    }
}
