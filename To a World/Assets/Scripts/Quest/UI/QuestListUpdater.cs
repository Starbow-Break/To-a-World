using System;
using System.Collections.Generic;
using UnityEngine;

public class QusetListUpdater : MonoBehaviour
{
    [SerializeField] private Transform _parentTransform;
    [SerializeField] private GameObject _itemPrefab;

    private Dictionary<string, QuestListItemUpdater> _itemUpdaters;

    private void Start()
    {
        _itemUpdaters = new Dictionary<string, QuestListItemUpdater>();
    }

    private void OnEnable()
    {
        GameEventsManager.QuestEvents.OnQuestStateChange += OnQuestStateChange;
        GameEventsManager.QuestEvents.OnQuestStepStateChange += OnQuestStepStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.QuestEvents.OnQuestStateChange -= OnQuestStateChange;
        GameEventsManager.QuestEvents.OnQuestStepStateChange -= OnQuestStepStateChange;
    }

    private void OnQuestStateChange(Quest quest)
    {
        if (
            quest.State == QuestState.IN_PROGRESS
            || quest.State == QuestState.CAN_FINISH)
        {
            TryAddQuestListItem(quest);
        }
        if (quest.State == QuestState.FINISHED)
        {
            TryRemoveQuestListItem(quest.Info.ID);
        }
    }
    
    private void OnQuestStepStateChange(string questId, int stepIndex, QuestStepState questStepState)
    {
        if (_itemUpdaters.TryGetValue(questId, out QuestListItemUpdater questItemUpdater))
        {
            questItemUpdater.SetQuestStepDescription("questStepState");
        }
        else
        {
            Debug.LogError($"Quest Item of ID : {questId} does not exist.");
        }
    }
    
    private void TryAddQuestListItem(Quest quest)
    {
        if (!_itemUpdaters.ContainsKey(quest.Info.ID))
        {
            var itemObj = Instantiate(_itemPrefab, _parentTransform);
            var updater = itemObj.GetComponent<QuestListItemUpdater>();
            _itemUpdaters.Add(quest.Info.ID, updater);
            updater.SetQuestTitle(quest.Info.DisplayName);
        }
    }
    
    private void TryRemoveQuestListItem(string questId)
    {
        if (_itemUpdaters.TryGetValue(questId, out QuestListItemUpdater questItemUpdater))
        {
            _itemUpdaters.Remove(questId);
            Destroy(questItemUpdater.gameObject);
        }
    }
}
