using System.Collections.Generic;
using UnityEngine;

public class QuestManager : SceneSingleton<QuestManager>
{
    private Dictionary<string, AQuest> _questMap;
    
    #region Unity Lifecycles
    private void Awake()
    {
        base.Awake();
        _questMap = CreateQuestMap();
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnStartQuest += StartQuest;
        GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest += FinishQuest;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnStartQuest -= StartQuest;
        GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest -= FinishQuest;
    }
    
    private void Start()
    {
        foreach (var quest in _questMap.Values)
        {
            GameEventsManager.GetEvents<QuestEvents>().QuestStateChange(quest);
            
            quest.gameObject.SetActive(quest.State == EQuestState.IN_PROGRESS || quest.State == EQuestState.CAN_FINISH);
        }
    }

    private void Update()
    {
        foreach (var quest in _questMap.Values)
        {
            if (quest.State == EQuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.Info.ID, EQuestState.CAN_START);
            }
        }
    }
    #endregion

    #region Methods
    private bool CheckRequirementsMet(AQuest quest)
    {
        bool meetsRequirements = true;
        
        foreach (var id in quest.Info.QuestPrerequisiteIDs)
        {
            if (GetQuestById(id).State != EQuestState.FINISHED)
            {
                meetsRequirements = false;
            }
        }

        return meetsRequirements;
    }

    private void ChangeQuestState(string id, EQuestState state)
    {
        var quest = GetQuestById(id);
        quest.State = state;
        quest.gameObject.SetActive(quest.State == EQuestState.IN_PROGRESS || quest.State == EQuestState.CAN_FINISH);
        GameEventsManager.GetEvents<QuestEvents>().QuestStateChange(quest);
    }

    private void StartQuest(string id)
    {
        var quest = GetQuestById(id);
        ChangeQuestState(quest.Info.ID, EQuestState.IN_PROGRESS);
    }
    
    private void FinishQuest(string id)
    {
        var quest = GetQuestById(id);
        ChangeQuestState(quest.Info.ID, EQuestState.FINISHED);
    }

    private Dictionary<string, AQuest> CreateQuestMap()
    {
        QuestData[] allQuests = Resources.LoadAll<QuestData>("Quests");
        
        Dictionary<string, AQuest> idToQuestMap = new();
        foreach (var questData in allQuests)
        {
            var quest = QuestFactory.CreateQuest(questData.Type, questData.Param, transform);
            quest.Info = new QuestInfo(
                questData.ID,
                questData.Name,
                questData.Description,
                questData.questPrerequisites);
            
            if (!idToQuestMap.TryAdd(questData.ID, quest))
            {
                Debug.LogWarning($"Duplicate Id found : {questData.ID}");
                Destroy(quest.gameObject);
            }
        }

        return idToQuestMap;
    }

    private AQuest GetQuestById(string id)
    {
        var quest = _questMap[id];
        
        if (quest == null)
        {
            Debug.LogError($"ID not found : {id}");
        }
        return quest;
    }
    #endregion
}
