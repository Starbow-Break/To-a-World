using System;
using UnityEngine;

public class QuestPoint : MonoBehaviour
{
    #region Fields
    [Header("Quest")] 
    [SerializeField] private QuestData _questInfoForPoint;

    [Header("Config")] 
    [SerializeField] private bool _startPoint = true;
    [SerializeField] private bool _finishPoint = true;
    
    private bool _playerIsNear = false;
    private string _questId;
    private EQuestState _currentEQuestState;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        _questId = _questInfoForPoint.ID;
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange -= QuestStateChange;
    }
    #endregion
    
    #region Methods
    public void SubmitPressed()
    {
        /*if (!_playerIsNear)
        {
            return;
        }*/

        Debug.Log(_currentEQuestState);

        if (_currentEQuestState.Equals(EQuestState.CAN_START) && _startPoint)
        {
            GameEventsManager.GetEvents<QuestEvents>().StartQuest(_questId);
        }
        if (_currentEQuestState.Equals(EQuestState.CAN_FINISH) && _finishPoint)
        {
            GameEventsManager.GetEvents<QuestEvents>().FinishQuest(_questId);
        }
    }

    private void QuestStateChange(AQuest aQuest)
    {
        if (aQuest.Info.ID.Equals(_questId))
        {
            _currentEQuestState = aQuest.State;
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        var hitLayer =  other.gameObject.layer;
        if (other.CompareTag("Player"))
        {
            _playerIsNear = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        var hitLayer =  other.gameObject.layer;
        if (other.CompareTag("Player"))
        {
            _playerIsNear = false;
        }
    }*/
    #endregion
}
