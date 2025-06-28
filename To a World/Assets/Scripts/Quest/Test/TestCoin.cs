using UnityEngine;

public class TestCoin : MonoBehaviour
{
    public void OnClick()
    {
        GameEventsManager.GetEvents<CoinEvents>().GainGold(1);
    }
}