using UnityEngine;

public class CollectCoinsQuestStep : AQuestStep
{
    private int _coinsCollected = 0;
    private int _coinsToComplete = 5;

    private void CoinCollected()
    {
        if (_coinsCollected < _coinsToComplete)
        {
            _coinsCollected++;
            UpdateState();
        }
        
        if (_coinsCollected >= _coinsToComplete)
        {
            FinishQuestStep();
        }
    }

    private void UpdateState()
    {
        string state = _coinsCollected.ToString();
        ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        _coinsCollected = int.Parse(state);
        UpdateState();
    }
}
