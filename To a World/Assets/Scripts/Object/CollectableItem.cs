using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CollectableItem : MonoBehaviour
{
    [field: SerializeField] public string ID { get; private set; }
    [SerializeField] private XRBaseInteractable _interactable;
    
    private void Awake()
    {
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (String.IsNullOrEmpty(ID))
        {
            ID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
