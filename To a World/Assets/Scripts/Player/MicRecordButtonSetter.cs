using System.Collections;
using NPCSystem;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MicRecordButtonSetter : MonoBehaviour
{
    [SerializeField] private RecordButtonUpdater _updater;

    [SerializeField] private GestureMic _mic;

    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _recordingColor;
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _updater.AddListenerSelectEnter(OnFocusEnter);
        _updater.AddListenerSelectExit(OnFocusExit);
    }
    
    private void OnFocusEnter(SelectEnterEventArgs args)
    {
        NPCChatSystem.NPCChatManager.TryStartRecording();
        _updater.SetColor(_recordingColor);
    }

    private void OnFocusExit(SelectExitEventArgs args)
    {
        if (_mic.IsActive)
        {
            NPCChatSystem.NPCChatManager.TryStopRecording();
        }
        else
        {
            NPCChatSystem.NPCChatManager.CancelRecording();
        }
        
        _updater.SetColor(_normalColor);
    }
}
