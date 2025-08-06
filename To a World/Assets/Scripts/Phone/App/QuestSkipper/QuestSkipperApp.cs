using System;
using Phone;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestSkipperApp : AApp
{
    [SerializeField] private QuestSkipperWindow window;

    private HashSet<string> _progressingQuestIds = new();

    private void Awake()
    {
        window.SkipButton.onClick.AddListener(SkipAllProgressingQuest);
        GameEventsManager.GetEvents<QuestEvents>().OnStartQuest += StartQuest;
        GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest += FinishQuest;
    }
    
    private void OnDestroy()
    {
        if (GameEventsManager.TryGetEvents<QuestEvents>(out var questEvents))
        {
            questEvents.OnStartQuest -= StartQuest;
            questEvents.OnFinishQuest -= FinishQuest;
        }
    }
    
    private void StartQuest(string questId)
    {
        _progressingQuestIds.Add(questId);
        window.SetQuestName(questId);
    }

    private void FinishQuest(string questId)
    {
        if (!_progressingQuestIds.Remove(questId))
        {
            Debug.LogWarning($"Quest {questId} does not exist. But remove tried");
        }
    }
    
    private void SkipAllProgressingQuest()
    {
        var questIdList = _progressingQuestIds.ToList();
        foreach (var questId in questIdList)
        {
            GameEventsManager.GetEvents<QuestEvents>().FinishQuest(questId);
        }
    }
    
    public override void Open()
    {
        window.Open();
    }

    public override void Close()
    {
        window.Close();
    }
}
