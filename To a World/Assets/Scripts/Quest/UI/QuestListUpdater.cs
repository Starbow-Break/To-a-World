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
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += OnQuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange -= OnQuestStateChange;
    }

    private void OnQuestStateChange(AQuest quest)
    {
        if (
            quest.State == EQuestState.IN_PROGRESS
            || quest.State == EQuestState.CAN_FINISH)
        {
            TryAddQuestListItem(quest);
        }
        if (quest.State == EQuestState.FINISHED)
        {
            TryRemoveQuestListItem(quest.Info.ID);
        }
    }
    
    private void TryAddQuestListItem(AQuest quest)
    {
        if (!_itemUpdaters.ContainsKey(quest.Info.ID))
        {
            var itemObj = Instantiate(_itemPrefab, _parentTransform);
            var updater = itemObj.GetComponent<QuestListItemUpdater>();
            _itemUpdaters.Add(quest.Info.ID, updater);
            updater.SetQuestTitle(quest.Info.Name);
            updater.SetQuestDescription(quest.Info.Description);
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