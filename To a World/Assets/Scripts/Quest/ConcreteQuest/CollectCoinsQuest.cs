using UnityEngine;

public class CollectCoinsQuest : AQuest
{
    private int _coinsCollected = 0;
    private int _coinsToComplete;

    private void OnEnable()
    {
        GameEventsManager.GetEvents<CoinEvents>().OnGainGold += CoinCollected;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<CoinEvents>().OnGainGold -= CoinCollected;
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as CollectCoinsQuestParams;
        if (param != null)
        {
           _coinsToComplete = param.CoinsToComplete;
        }
    }

    private void CoinCollected(int amount)
    {
        if (_coinsCollected < _coinsToComplete)
        {
            _coinsCollected += amount;
        }
        
        if (_coinsCollected >= _coinsToComplete)
        {
            CompleteQuest();
        }
    }
}
