using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRGrabbableSnapAttractor: SnapAttractor
{
    [SerializeField] private XRGrabInteractable _interactable;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (_interactable == null)
        {
            _interactable = GetComponentInChildren<XRGrabInteractable>();

            if (_interactable == null)
            {
                Debug.LogError("BeltBuckle needs a XRSocketInteractor");
            }
        }
    }

    public void AddListenerOnSelectEntered(UnityAction<SelectEnterEventArgs> action)
    {
        _interactable.selectEntered.AddListener(action);
    }
    
    public void AddListenerOnSelectExited(UnityAction<SelectExitEventArgs> action)
    {
        _interactable.selectExited.AddListener(action);
    }
    
    public void RemoveAllListenersOnSelectEntered()
    {
        _interactable.selectEntered.RemoveAllListeners();
    }
    
    public void RemoveAllListenersOnSelectExited()
    {
        _interactable.selectExited.RemoveAllListeners();
    }
}
