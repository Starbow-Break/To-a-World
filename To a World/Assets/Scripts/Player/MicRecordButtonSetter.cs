using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MicRecordButtonSetter : MonoBehaviour
{
    [SerializeField] private RecordButtonUpdater _updater;

    [SerializeField] private GestureMic _mic;
    [SerializeField] private TTSUIController _ttsSender;

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
        _ttsSender.TryStartRecording();
        _updater.SetColor(_recordingColor);
    }

    private void OnFocusExit(SelectExitEventArgs args)
    {
        if (_mic.IsActive)
        {
            _ttsSender.TryStopRecording();
        }
        else
        {
            _ttsSender.TryStopRecordingAndDontSend();
        }
        
        _updater.SetColor(_normalColor);
    }
}
