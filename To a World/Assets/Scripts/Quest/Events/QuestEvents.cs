using System;

public class QuestEvents : IEvents
{
    public event Action<string> OnStartQuest;
    public void StartQuest(string id) => OnStartQuest?.Invoke(id);

    public event Action<string> OnFinishQuest;
    public void FinishQuest(string id) => OnFinishQuest?.Invoke(id);

    public event Action<AQuest> OnQuestStateChange;
    public void QuestStateChange(AQuest aQuest) => OnQuestStateChange?.Invoke(aQuest);
} 