using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class TalkMicButtonSetter : MonoBehaviour
{
    [field: SerializeField] public TalkMicButtonUpdater Updater { get; private set; }
    [field: SerializeField] public Color ActiveColor { get; private set; }
    [field: SerializeField] public Color InActiveColor { get; private set; }

    public void SetButtonInteractable(bool interactable)
    {
        Updater.SetInteractable(interactable);
        Updater.SetColor(interactable ? ActiveColor : InActiveColor);
    }

    public void AddListenerSelectEnter(UnityAction<SelectEnterEventArgs> selectEnterAction)
    {
        Updater.AddListenerSelectEnter(selectEnterAction);
    }
    
    public void AddListenerSelectExit(UnityAction<SelectExitEventArgs> selectExitAction)
    {
        Updater.AddListenerSelectExit(selectExitAction);
    }
    
    public void RemoveAllListenersSelectEnter()
    {
        Updater.RemoveAllListenersSelectEnter();
    }
    
    public void RemoveAllListenersSelectExit()
    {
        Updater.RemoveAllListenersSelectEnter();
    }
}
