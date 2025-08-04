using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ItemXRSocketInteractor: MonoBehaviour
{
    [SerializeField] private ItemSocketData _itemSocketData;
    [SerializeField] private GameObject _visualizer;

    private string _id;
    private XRSocketInteractor _interactor;

    private void Awake()
    {
        _id = _itemSocketData.ID;
        _interactor = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        _interactor.selectEntered.AddListener(PlaceTargetItem);
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        _interactor.selectEntered.AddListener(PlaceTargetItem);
    }

    private void QuestStateChange(AQuest quest)
    {
        var piQuest = quest as PlaceItemQuest;
        if (piQuest != null && piQuest.TargetSocketId == _id)
        {
            bool active = piQuest.State == EQuestState.IN_PROGRESS || piQuest.State == EQuestState.CAN_FINISH;
            _visualizer.SetActive(active);
            _interactor.enabled = active;
        }
    }

    private void PlaceTargetItem(SelectEnterEventArgs args)
    {
        var interactable = args.interactableObject;
        var item = interactable.transform.GetComponent<Item>();
        GameEventsManager.GetEvents<ItemEvents>().PlaceItem(_itemSocketData.ID, item.ID);
    }
}
