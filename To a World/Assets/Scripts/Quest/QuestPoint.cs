using System;
using UnityEngine;

public class QuestPoint : MonoBehaviour
{
    #region Fields
    [Header("Target Layer")]
    [SerializeField] private LayerMask _playerLayerMask;

    [Header("Quest")] 
    [SerializeField] private QuestInfoSO _questInfoForPoint;

    [Header("Config")] 
    [SerializeField] private bool _startPoint = true;
    [SerializeField] private bool _finishPoint = true;
    
    private bool _playerIsNear = false;
    private string _questId;
    private QuestState _currentQuestState;

    // private QuestIcon _questIcon;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        _questId = _questInfoForPoint.ID;
        //_questIcon = GetComponentInChildren<QuestIcon>();
    }

    private void OnEnable()
    {
        GameEventsManager.QuestEvents.OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.QuestEvents.OnQuestStateChange -= QuestStateChange;
    }
    #endregion
    
    #region Methods
    public void SubmitPressed()
    {
        /*if (!_playerIsNear)
        {
            return;
        }*/

        if (_currentQuestState.Equals(QuestState.CAN_START) && _startPoint)
        {
            GameEventsManager.QuestEvents.StartQuest(_questId);
        }
        if (_currentQuestState.Equals(QuestState.CAN_FINISH) && _finishPoint)
        {
            GameEventsManager.QuestEvents.FinishQuest(_questId);
        }
    }

    private void QuestStateChange(Quest quest)
    {
        if (quest.Info.ID.Equals(_questId))
        {
            _currentQuestState = quest.State;
            //_questIcon.SetState(_currentQuestState, _startPoint, _finishPoint);
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        var hitLayer =  other.gameObject.layer;
        if ((_playerLayerMask & 1 << hitLayer) > 0)
        {
            _playerIsNear = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        var hitLayer =  other.gameObject.layer;
        if ((_playerLayerMask & 1 << hitLayer) > 0)
        {
            _playerIsNear = false;
        }
    }*/
    #endregion
}
