using System;

public class ItemEvents: IEvents
{
    public event Action<string, int> OnCollect;
    public void Collect(string itemId, int quantity) => OnCollect?.Invoke(itemId, quantity);
    
    public event Action<string> OnPlaceItem;
    public void PlaceItem(string itemId) => OnPlaceItem?.Invoke(itemId);
}
