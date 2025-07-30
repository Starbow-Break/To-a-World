using UnityEngine;

public class PlaceItemQuest : AQuest
{
    public string TargetItemId { get; private set; }

    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as PlaceItemQuestParams;
        if (param != null)
        {
            TargetItemId = param.Item.ID;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameEventsManager.GetEvents<QuestEvents>().StartQuest(Info.ID);
        }
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<ItemEvents>().OnPlaceItem += OnPlaceItem;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<ItemEvents>().OnPlaceItem -= OnPlaceItem;
    }

    private void OnPlaceItem(string itemId)
    {
        if (itemId == TargetItemId)
        {
            CompleteQuest();
        }
    }
}
