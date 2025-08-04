using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestSkipper : MonoBehaviour
{
    private HashSet<string> _progressingQuestIds = new();

    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnStartQuest += StartQuest;
        GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest += FinishQuest;
    }

    private void OnDisable()
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
    }

    private void FinishQuest(string questId)
    {
        if (!_progressingQuestIds.Remove(questId))
        {
            Debug.LogWarning($"Quest {questId} does not exist. But remove tried");
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipAllProgressingQuest();
        }
    }
    
    public void SkipAllProgressingQuest()
    {
        var questIdList = _progressingQuestIds.ToList();
        foreach (var questId in questIdList)
        {
            GameEventsManager.GetEvents<QuestEvents>().FinishQuest(questId);
        }
    }
}
