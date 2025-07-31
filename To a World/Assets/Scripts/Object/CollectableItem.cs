using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CollectableItem : Item
{
    [SerializeField] private XRBaseInteractable _interactable;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (_interactable == null)
        {
            _interactable = GetComponentInChildren<XRBaseInteractable>();
        }
    }
    
    private void OnEnable()
    {
        _interactable.selectEntered.AddListener(Collect);
    }

    private void OnDisable()
    {
        _interactable.selectEntered.RemoveAllListeners();
    }

    private void Collect(SelectEnterEventArgs args)
    {
        GameEventsManager.GetEvents<ItemEvents>().Collect(ID, 1);
        Destroy(gameObject);
    }
}
