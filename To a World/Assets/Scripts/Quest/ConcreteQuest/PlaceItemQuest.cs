using UnityEngine;

public class PlaceItemQuest : AQuest
{
    public string TargetSocketId { get; private set; }
    public string TargetItemId { get; private set; }

    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as PlaceItemQuestParams;
        if (param != null)
        {
            TargetSocketId = param.Socket.ID;
            TargetItemId = param.Item.ID;
        }
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<ItemEvents>().OnPlaceItem += OnPlaceItem;
    }

    private void OnDisable()
    {
        if (GameEventsManager.TryGetEvents<ItemEvents>(out var itemEvents))
        {
            itemEvents.OnPlaceItem -= OnPlaceItem;
        }
    }

    private void OnPlaceItem(string socketId, string itemId)
    {
        if (socketId == TargetSocketId && itemId == TargetItemId)
        {
            CompleteQuest();
        }
    }
}
