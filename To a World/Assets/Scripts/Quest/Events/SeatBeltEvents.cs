using System;

public class SeatBeltEvents: IEvents
{
    public event Action OnConnect;
    public void ConnectedBelt() => OnConnect?.Invoke();
    
    public event Action OnDisconnect;
    public void DisconnectedBelt() => OnDisconnect?.Invoke();
}
