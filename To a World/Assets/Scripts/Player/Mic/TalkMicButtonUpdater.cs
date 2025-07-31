using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TalkMicButtonUpdater : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable _xrInteractable;
    [SerializeField] private Renderer _modelRenderer;
    
    public void AddListenerSelectEnter(UnityAction<SelectEnterEventArgs> selectEnterAction)
    {
        _xrInteractable.selectEntered.AddListener(selectEnterAction);
    }
    
    public void AddListenerSelectExit(UnityAction<SelectExitEventArgs> selectExitAction)
    {
        _xrInteractable.selectExited.AddListener(selectExitAction);
    }
    
    public void RemoveAllListenersSelectEnter()
    {
        _xrInteractable.selectEntered.RemoveAllListeners();
    }
    
    public void RemoveAllListenersSelectExit()
    {
        _xrInteractable.selectExited.RemoveAllListeners();
    }
    
    public void SetInteractable(bool interactable)
    {
        _xrInteractable.enabled = interactable;
    }

    public void SetColor(Color color)
    {
        _modelRenderer.material.color = color;
    }
}
