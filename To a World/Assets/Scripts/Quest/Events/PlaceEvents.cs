using System;

public class PlaceEvents : IEvents
{
    public event Action<string> OnArrive;
    public void Arrive(string placeId) => OnArrive?.Invoke(placeId);
}
