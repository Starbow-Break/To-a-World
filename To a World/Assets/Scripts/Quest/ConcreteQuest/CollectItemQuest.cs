using UnityEngine;

public class CollectItemQuest : AQuest
{
    public string TargetItemId { get; private set; }
    public int TargetQuantity { get; private set; }

    private int _acquiredItems = 0;

    private void OnEnable()
    {
        GameEventsManager.GetEvents<ItemEvents>().OnCollect += OnAcquire;
    }

    private void OnDisable()
    {
        var itemEvents = GameEventsManager.GetEvents<ItemEvents>();
        if (itemEvents != null)
        {
            itemEvents.OnCollect -= OnAcquire;
        }
    }

    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as CollectItemQuestParams;
        if (param != null)
        {
            TargetItemId = param.ItemData.ID;
            TargetQuantity = param.Quantity;
        }
    }

    private void OnAcquire(string itemId, int quantity)
    {
        if (itemId == TargetItemId)
        {
            _acquiredItems += quantity;
            if (_acquiredItems >= TargetQuantity)
            {
                CompleteQuest();
            }
        }
    }
}
