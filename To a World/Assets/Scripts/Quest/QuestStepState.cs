using System;

[Serializable]
public class QuestStepState
{
     public string State;

     public QuestStepState(string state)
     {
          State = state;
     }

     public QuestStepState()
     {
          State = "";
     }

     public event Action<Quest> OnQuestStateChange;
     public void QuestStateChange(Quest quest) => OnQuestStateChange?.Invoke(quest);
     
     public event Action<string, int, QuestStepState> OnQuestStepStateChange;
     public void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState) 
          => OnQuestStepStateChange?.Invoke(id, stepIndex, questStepState);
}
