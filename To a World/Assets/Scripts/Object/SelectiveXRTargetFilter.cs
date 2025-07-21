using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SelectiveXRTargetFilter : XRBaseTargetFilter
{
    [SerializeField] private List<GameObject> _targetObjects;

    private HashSet<IXRInteractable> _targetSet = new();

    private void Awake()
    {
        foreach (var obj in _targetObjects)
        {
            var xrInteractable = obj.GetComponent<IXRInteractable>();
            if (xrInteractable != null)
            {
                _targetSet.Add(xrInteractable);
            }
        }
    }

    public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
    {
        foreach (var target in targets)
        {
            if (_targetSet.Contains(target))
            {
                results.Add(target);
            }
        }
    }
}
