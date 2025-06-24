using System;
using UnityEngine;

public abstract class AQuestStep : MonoBehaviour
{
    private bool _isFinished = false;
    private string _questId;
    private int _stepIndex;

    public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
    {
        _questId = questId;
        _stepIndex = stepIndex;
        if (!String.IsNullOrEmpty(questStepState))
        {
            SetQuestStepState(questStepState);
        }
    }

    protected void FinishQuestStep()
    {
        if (!_isFinished)
        {
            _isFinished = true;
            GameEventsManager.QuestEvents.AdvanceQuest(_questId);
            Destroy(gameObject);
        }
    }

    protected void ChangeState(string newState)
    { 
        GameEventsManager.QuestEvents.QuestStepStateChange(_questId, _stepIndex, new QuestStepState(newState));   
    }

    protected abstract void SetQuestStepState(string state);
}
