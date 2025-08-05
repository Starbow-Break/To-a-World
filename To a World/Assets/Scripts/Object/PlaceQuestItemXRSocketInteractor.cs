using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ItemXRSocketInteractor: MonoBehaviour
{
    [Header("General")]
    [SerializeField] private ItemSocketData _itemSocketData;
    [SerializeField] private GameObject _visualizer;
    [SerializeField, Min(0.0f)] private float _inactiveDelay = 0.1f;

    private string _id;
    private XRSocketInteractor _interactor;

    private void Awake()
    {
        _id = _itemSocketData.ID;
        _interactor = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        _interactor.selectEntered.AddListener(PlaceItem);
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        _interactor.selectEntered.AddListener(PlaceItem);
        if (GameEventsManager.TryGetEvents<QuestEvents>(out var questEvents))
        {
            questEvents.OnQuestStateChange -= QuestStateChange;
        }
    }

    private void QuestStateChange(AQuest quest)
    {
        var piQuest = quest as PlaceItemQuest;
        if (piQuest != null && piQuest.TargetSocketId == _id)
        {
            bool active = piQuest.State == EQuestState.IN_PROGRESS || piQuest.State == EQuestState.CAN_FINISH;
            if (active)
            {
                _visualizer.SetActive(true);
                _interactor.socketActive = true;
            }
            else
            {
                _visualizer.SetActive(false);
                SocketInactiveSequence().Forget();
            }
        }
    }

    private async UniTaskVoid SocketInactiveSequence()
    {
        await UniTask.WaitForSeconds(_inactiveDelay);
        _interactor.socketActive = false;
    }

    private void PlaceItem(SelectEnterEventArgs args)
    {
        var interactable = args.interactableObject;
        var item = interactable.transform.GetComponent<Item>();
        GameEventsManager.GetEvents<ItemEvents>().PlaceItem(_itemSocketData.ID, item.ID);
    }
}
