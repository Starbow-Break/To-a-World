using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BeltBuckle : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor _socketInteractor;

    private void Awake()
    {
        if (_socketInteractor == null)
        {
            _socketInteractor = GetComponentInChildren<XRSocketInteractor>();

            if (_socketInteractor == null)
            {
                Debug.LogError("BeltBuckle needs a XRSocketInteractor");
            }
        }
    }

    public void AddListenerOnSelectEntered(UnityAction<SelectEnterEventArgs> action)
    {
        _socketInteractor.selectEntered.AddListener(action);
    }
    
    public void AddListenerOnSelectExited(UnityAction<SelectExitEventArgs> action)
    {
        _socketInteractor.selectExited.AddListener(action);
    }
    
    public void RemoveAllListenersOnSelectEntered()
    {
        _socketInteractor.selectEntered.RemoveAllListeners();
    }
    
    public void RemoveAllListenersOnSelectExited()
    {
        _socketInteractor.selectExited.RemoveAllListeners();
    }
}
