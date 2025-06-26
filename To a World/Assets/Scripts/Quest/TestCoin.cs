using UnityEngine;

public class TestCoin : MonoBehaviour
{
    public void OnClick()
    {
        GameEventsManager.CoinEvents.GainGold(1);
    }
}
