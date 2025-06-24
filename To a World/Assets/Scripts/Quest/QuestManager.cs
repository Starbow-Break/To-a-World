using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private bool _loadQuestState = true;
    
    private Dictionary<string, Quest> _questMap;

    private void Awake()
    {
        _questMap = CreateQuestMap();
    }

    private void OnEnable()
    {
        GameEventsManager.QuestEvents.OnStartQuest += StartQuest;
        GameEventsManager.QuestEvents.OnAdvanceQuest += AdvancedQuest;
        GameEventsManager.QuestEvents.OnFinishQuest += FinishQuest;

        GameEventsManager.QuestEvents.OnQuestStepStateChange += QuestStepStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.QuestEvents.OnStartQuest -= StartQuest;
        GameEventsManager.QuestEvents.OnAdvanceQuest -= AdvancedQuest;
        GameEventsManager.QuestEvents.OnFinishQuest -= FinishQuest;
        
        GameEventsManager.QuestEvents.OnQuestStepStateChange -= QuestStepStateChange;
    }
    
    private void Start()
    {
        foreach (var quest in _questMap.Values)
        {
            if (quest.State == QuestState.IN_PROGRESS)
            { 
                quest.InstantiateCurrentQuestStep(transform);    
            }
            GameEventsManager.QuestEvents.QuestStateChange(quest);
        }
    }

    private void Update()
    {
        foreach (var quest in _questMap.Values)
        {
            if (quest.State == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.Info.ID, QuestState.CAN_START);
            }
        }
    }

    private bool CheckRequirementsMet(Quest quest)
    {
        return true;
    }

    private void ChangeQuestState(string id, QuestState state)
    {
        var quest = GetQuestById(id);
        quest.State = state;
        GameEventsManager.QuestEvents.QuestStateChange(quest);
    }

    private void StartQuest(string id)
    {
        var quest = GetQuestById(id);
        quest.InstantiateCurrentQuestStep(transform);
        ChangeQuestState(quest.Info.ID, QuestState.IN_PROGRESS);
    }
    
    private void AdvancedQuest(string id)
    {
        var quest = GetQuestById(id);
        quest.MoveToNextStep();

        if (quest.CurrentStepExists())
        {
            quest.InstantiateCurrentQuestStep(transform);
        }
        else
        {
            ChangeQuestState(quest.Info.ID, QuestState.CAN_FINISH);
        }
    }
    
    private void FinishQuest(string id)
    {
        var quest = GetQuestById(id);
        ChangeQuestState(quest.Info.ID, QuestState.FINISHED);
    }

    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        var quest = GetQuestById(id);
        quest.StoreQuestStepState(questStepState, stepIndex);
        ChangeQuestState(id, quest.State);
    }

    private Dictionary<string, Quest> CreateQuestMap()
    {
        QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
        
        Dictionary<string, Quest> idToQuestMap = new();
        foreach (var questInfo in allQuests)
        {
            if (idToQuestMap.ContainsKey(questInfo.ID))
            {
                Debug.LogWarning($"Duplicate Id found : {questInfo.ID}");
            }

            idToQuestMap.Add(questInfo.ID, LoadQuest(questInfo));
        }

        return idToQuestMap;
    }

    private Quest GetQuestById(string id)
    {
        var quest = _questMap[id];
        
        if (quest == null)
        {
            Debug.LogError($"ID not found : {id}");
        }
        return quest;
    }

    private void OnApplicationQuit()
    {
        foreach (var quest in _questMap.Values)
        {
            SaveQuest(quest);
        }
    }

    private void SaveQuest(Quest quest)
    {
        try
        {
            var questData = quest.GetQuestData();
            string serializedData = JsonUtility.ToJson(questData);
            PlayerPrefs.SetString(quest.Info.ID, serializedData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save quest with id {quest.Info.ID} : {e}");
        }
    }

    private Quest LoadQuest(QuestInfoSO questInfo)
    {
        Quest quest = null;

        try
        {
            if (PlayerPrefs.HasKey(questInfo.ID) && _loadQuestState)
            {
                string serializedData = PlayerPrefs.GetString(questInfo.ID);
                var questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quest(questInfo, questData.State, questData.QuestStepIndex, questData.QuestStepStates);
            }
            else
            {
                quest = new Quest(questInfo);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load quest with id {quest.Info.ID} : {e}");
        }

        return quest;
    }
}
