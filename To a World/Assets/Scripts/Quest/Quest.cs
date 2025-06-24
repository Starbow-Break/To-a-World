using UnityEngine;

public class Quest
{
    public QuestInfoSO Info;

    public QuestState State;
    private int _currentQuestStepIndex;
    private QuestStepState[] _questStepStates;

    public Quest(QuestInfoSO questInfo)
    {
        Info = questInfo;
        State = QuestState.REQUIREMENTS_NOT_MET;
        _currentQuestStepIndex = 0;
        _questStepStates = new QuestStepState[Info.questStepPrefabs.Length];
        
        for (int i = 0; i < _questStepStates.Length; i++)
        {
            _questStepStates[i] = new QuestStepState();
        }
    }
    
    public Quest(QuestInfoSO questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        Info = questInfo;
        State = questState;
        _currentQuestStepIndex = currentQuestStepIndex;
        _questStepStates = questStepStates;

        if (questStepStates.Length != Info.questStepPrefabs.Length)
        {
            Debug.LogWarning("Length of questStepStates and length of questStepPrefabs are different.");
        }
    }

    public void MoveToNextStep()
    {
        _currentQuestStepIndex++;
    }

    public bool CurrentStepExists()
    {
        return _currentQuestStepIndex < Info.questStepPrefabs.Length;
    }

    public void InstantiateCurrentQuestStep(Transform parentTransform)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
        {
            var questStep = Object.Instantiate(questStepPrefab, parentTransform)
                .GetComponent<AQuestStep>();
            questStep.InitializeQuestStep(Info.ID, _currentQuestStepIndex, _questStepStates[_currentQuestStepIndex].State);
        }
    }

    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepPrefab = null;
        if (CurrentStepExists())
        {
            questStepPrefab = Info.questStepPrefabs[_currentQuestStepIndex];
        }
        else
        {
            Debug.LogWarning("Step index was out of range");
        }

        return questStepPrefab;
    }

    public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
    {
        if (stepIndex < _questStepStates.Length)
        {
            _questStepStates[stepIndex].State = questStepState.State;
        }
        else
        {   Debug.LogWarning($"Tried to access quest step data, but stepIndex was out of range."
                + $"Quest Id = {Info.ID}, Step Index = {stepIndex}");
        }
    }

    public QuestData GetQuestData()
    {
        return new QuestData(State, _currentQuestStepIndex, _questStepStates);
    }
}
