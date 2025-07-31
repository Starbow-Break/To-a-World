using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ItemXRTargetFilter : XRBaseTargetFilter
{
    private string _targetItemId;

    public void Initialize(string itemId)
    {
        _targetItemId = itemId;
        Debug.Log(_targetItemId);
    }

    public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
    {
        foreach (var target in targets)
        {
            var item = target.transform.GetComponentInParent<Item>();
            Debug.Log($"{item.ID}");
            if (item != null && item.ID == _targetItemId)
            {
                results.Add(target);
            }
        }
    }
}
