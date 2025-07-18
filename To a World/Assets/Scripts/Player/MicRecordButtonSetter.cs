using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public struct RecordButtonData
{
    [field: SerializeField] public RecordButtonUpdater Updater { get; private set; }
    [field: SerializeField] public Color InActive { get; private set; }
    [field: SerializeField] public Color Active { get; private set; }
}

public class MicRecordButtonSetter : MonoBehaviour
{
    [SerializeField] private GestureMic _mic;
    [SerializeField] private RecordButtonData _startRecordButtonData;
    [SerializeField] private RecordButtonData _stopRecordButtonData;
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _startRecordButtonData.Updater.AddListenerSelectEnter(OnSelectStartButton);
        _stopRecordButtonData.Updater.AddListenerSelectExit(OnSelectExitButton);
    }
    
    void OnSelectStartButton(SelectEnterEventArgs arg)
    {
        NPCChatSystem.NPCChatManager.TryStartRecording();
        SetButtonInteractable(_startRecordButtonData, false);
        SetButtonInteractable(_stopRecordButtonData, true);
    }
        
    void OnSelectExitButton(SelectExitEventArgs arg)
    {
        if (_mic.IsActive)
        {
            NPCChatSystem.NPCChatManager.TryStopRecording();
        }
        else
        {
            NPCChatSystem.NPCChatManager.CancelRecording();
        }
    
        SetButtonInteractable(_startRecordButtonData, true);
        SetButtonInteractable(_stopRecordButtonData, false);
    }

    void SetButtonInteractable(RecordButtonData data, bool interactable)
    {
        data.Updater.SetInteractable(interactable);
        data.Updater.SetColor(interactable ? data.Active : data.InActive);
    }
}
