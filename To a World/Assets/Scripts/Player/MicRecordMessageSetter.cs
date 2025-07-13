using NPCSystem;
using UnityEngine;

public class MicRecordMessageSetter : MonoBehaviour
{
    [SerializeField] private RecordMessageUpdater _updater;
    
    private void OnEnable()
    {
        NPCChatSystem.NPCChatManager.OnRecordingStateChanged += RecordingStateChanged;
    }
    
    private void OnDisable()
    {
        NPCChatSystem.NPCChatManager.OnRecordingStateChanged -= RecordingStateChanged;
    }
    
    private void Start()
    {
        _updater.Initialize();
    }

    private void RecordingStateChanged(bool recording)
    {
        _updater.SetActive(recording);
    }
}
