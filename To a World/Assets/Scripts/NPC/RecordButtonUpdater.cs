using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RecordButtonUpdater : MonoBehaviour
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

    public void AddListenerFocusEnter(UnityAction<FocusEnterEventArgs> focusEnterAction)
    {
        _xrInteractable.focusEntered.AddListener(focusEnterAction);
    }
    
    public void AddListenerFocusExit(UnityAction<FocusExitEventArgs> focusExitAction)
    {
        _xrInteractable.focusExited.AddListener(focusExitAction);
    }

    public void SetColor(Color color)
    {
        _modelRenderer.material.color = color;
    }
}
