using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ItemXRTargetFilter : XRBaseTargetFilter
{
    [SerializeField] List<ItemData> _items;

    private HashSet<string> _targetItemIds = new();

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var item in _items)
        {
            if (_targetItemIds.TryGetValue(item.ID, out var _))
            {
                Debug.LogWarning($"Duplicate ID : {item.ID}");
            }
            else
            {
                _targetItemIds.Add(item.ID);
            }
        }
    }

    public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
    {
        foreach (var target in targets)
        {
            var item = target.transform.GetComponentInParent<Item>();
            if (item != null && _targetItemIds.TryGetValue(item.ID, out var _))
            {
                results.Add(target);
            }
        }
    }
}
