using System;
using UnityEngine;
using UnityEngine.Events;
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

    public void SetStartButtonInteractable(bool interactable) 
        => SetButtonInteractable(_startRecordButtonData, interactable);
    public void SetStopButtonInteractable(bool interactable)
        => SetButtonInteractable(_stopRecordButtonData, interactable);
    
    public void AddStartButtonListenerSelectEnter(UnityAction<SelectEnterEventArgs> selectEnterAction)
        => AddListenerSelectEnter(_startRecordButtonData, selectEnterAction);
    public void AddStopButtonListenerSelectEnter(UnityAction<SelectEnterEventArgs> selectEnterAction)
        => AddListenerSelectEnter(_stopRecordButtonData, selectEnterAction);
    
    public void AddStartButtonListenerSelectExit(UnityAction<SelectExitEventArgs> selectExitAction)
        => AddListenerSelectExit(_startRecordButtonData, selectExitAction);
    public void AddStopButtonListenerSelectExit(UnityAction<SelectExitEventArgs> selectExitAction)
        => AddListenerSelectExit(_stopRecordButtonData, selectExitAction);
    
    public void RemoveStartButtonAllListenersSelectEnter()
        => RemoveAllListenersSelectEnter(_startRecordButtonData);
    public void RemoveStartButtonAllListenersSelectExit()
        => RemoveAllListenersSelectExit(_startRecordButtonData);
    
    public void RemoveStopButtonAllListenersSelectEnter()
        => RemoveAllListenersSelectEnter(_stopRecordButtonData);
    public void RemoveStopButtonAllListenersSelectExit()
        => RemoveAllListenersSelectExit(_stopRecordButtonData);
    
    private void SetButtonInteractable(RecordButtonData data, bool interactable)
    {
        data.Updater.SetInteractable(interactable);
        data.Updater.SetColor(interactable ? data.Active : data.InActive);
    }

    private void AddListenerSelectEnter(RecordButtonData data, UnityAction<SelectEnterEventArgs> selectEnterAction)
    {
        data.Updater.AddListenerSelectEnter(selectEnterAction);
    }
    
    private void AddListenerSelectExit(RecordButtonData data, UnityAction<SelectExitEventArgs> selectExitAction)
    {
        data.Updater.AddListenerSelectExit(selectExitAction);
    }
    
    public void RemoveAllListenersSelectEnter(RecordButtonData data)
    {
        data.Updater.RemoveAllListenersSelectEnter();
    }
    
    public void RemoveAllListenersSelectExit(RecordButtonData data)
    {
        data.Updater.RemoveAllListenersSelectEnter();
    }
}
