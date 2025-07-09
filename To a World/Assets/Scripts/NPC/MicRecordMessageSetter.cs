using UnityEngine;

public class MicRecordMessageSetter : MonoBehaviour
{
    [SerializeField] private RecordMessageUpdater _updater;
    [SerializeField] private TTSUIController _ttsSender;
    
    private void OnEnable()
    {
        _ttsSender.OnStartRecording += StartTTSRecording;
        _ttsSender.OnStopRecording += StopTTSRecording;
    }
    
    private void OnDisable()
    {
        _ttsSender.OnStartRecording -= StartTTSRecording;
        _ttsSender.OnStopRecording -= StopTTSRecording;
    }
    
    private void Start()
    {
        _updater.Initialize();
    }

    private void StartTTSRecording()
    {
        _updater.Active();
    }

    private void StopTTSRecording()
    {
        _updater.InActive();
    }
}
