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

    public void SetBackgroundColor(Color color)
    {
        Updater.SetBackgroundColor(color);
    }
    
    public void SetIcon(Sprite sprite)
    {
        Updater.SetIcon(sprite);
    }

    public void AddOnClickListener(UnityAction selectEnterAction)
    {
        Updater.AddOnClickListener(selectEnterAction);
    }
    
    public void RemoveAllListenersSelectEnter()
    {
        Updater.RemoveAllOnClickListeners();
    }
}
