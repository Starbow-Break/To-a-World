public class CollectItemQuest : AQuest
{
    private string _targetItemId;
    private int _targetQuantity;

    private int _acquiredItems = 0;

    private void OnEnable()
    {
        GameEventsManager.GetEvents<ItemEvents>().OnCollect += OnAcquire;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<ItemEvents>().OnCollect -= OnAcquire;
    }

    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as CollectItemQuestParams;
        if (param != null)
        {
            _targetItemId = param.ItemID;
            _targetQuantity = param.Quantity;
        }
    }

    private void OnAcquire(string itemId, int quantity)
    {
        if (itemId == _targetItemId)
        {
            _acquiredItems += quantity;
            if (_acquiredItems >= _targetQuantity)
            {
                CompleteQuest();
            }
        }
    }
}
