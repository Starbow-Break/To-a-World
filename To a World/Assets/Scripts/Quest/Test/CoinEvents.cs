using System;

public class CoinEvents: IEvents
{
    public event Action<int> OnGainGold;
    public void GainGold(int amount) => OnGainGold?.Invoke(amount);
}
