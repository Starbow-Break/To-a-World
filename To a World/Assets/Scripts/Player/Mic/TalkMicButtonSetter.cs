using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class TalkMicButtonSetter : MonoBehaviour
{
    [field: SerializeField] public TalkMicButtonUpdater Updater { get; private set; }

    public void SetButtonInteractable(bool interactable)
    {
        Updater.SetInteractable(interactable);
    }

    public void SetColor(Color color)
    {
        Updater.SetColor(color);
    }

    public void AddListenerSelectEnter(UnityAction<SelectEnterEventArgs> selectEnterAction)
    {
        Updater.AddListenerSelectEnter(selectEnterAction);
    }
    
    public void RemoveAllListenersSelectEnter()
    {
        Updater.RemoveAllListenersSelectEnter();
    }
}
