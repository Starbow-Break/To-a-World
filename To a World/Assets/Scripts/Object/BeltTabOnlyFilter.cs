using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BeltTabOnlyFilter : XRBaseTargetFilter
{
    [SerializeField] private BeltTab _targetTab;
    
    public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
    {
        foreach (var target in targets)
        {
            var tab = target.transform.GetComponent<BeltTab>();
            if (tab != null && (_targetTab == null || tab == _targetTab)) // 혹은 이름, 컴포넌트 등으로 판별
            {
                results.Add(target);
            }
        }
    }
}
