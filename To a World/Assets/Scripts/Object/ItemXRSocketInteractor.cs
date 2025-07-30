using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ItemXRSocketInteractor: MonoBehaviour
{
    [SerializeField] private ItemData _targetItem;
    [SerializeField] private ItemXRTargetFilter _targetFilter;
    
    private XRSocketInteractor _interactor;

    private void Awake()
    {
        _interactor = GetComponent<XRSocketInteractor>();    
    }
    
    private void Start()
    {
        _targetFilter.Initialize(_targetItem.ID);
    }

    private void OnEnable()
    {
        _interactor.selectEntered.AddListener(PlaceTargetItem);
    }

    private void OnDisable()
    {
        _interactor.selectEntered.AddListener(PlaceTargetItem);
    }

    private void PlaceTargetItem(SelectEnterEventArgs args)
    {
        GameEventsManager.GetEvents<ItemEvents>().PlaceItem(_targetItem.ID);
    }
}
