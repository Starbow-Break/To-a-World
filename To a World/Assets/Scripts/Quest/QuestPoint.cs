using System;
using UnityEngine;

public class QuestPoint : MonoBehaviour
{
    [Header("Target Layer")]
    [SerializeField] private LayerMask _playerLayerMask;

    [Header("Quest")] 
    [SerializeField] private QuestInfoSO _questInfoForPoint;

    [Header("Config")] 
    [SerializeField] private bool _startPoint = true;
    [SerializeField] private bool _finishPoint = true;
    
    private bool _playerIsNear = false;
    private string questId;
    private QuestState currentQuestState;

    private QuestIcon _questIcon;

    private void Awake()
    {
        questId = _questInfoForPoint.ID;
        _questIcon = GetComponentInChildren<QuestIcon>();
    }

    private void OnEnable()
    {
        Debug.Log(GameEventsManager.Instance);
        Debug.Log(GameEventsManager.QuestEvents);
        GameEventsManager.QuestEvents.OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.QuestEvents.OnQuestStateChange -= QuestStateChange;
    }

    public void SubmitPressed()
    {
        /*if (!_playerIsNear)
        {
            return;
        }*/

        if (currentQuestState.Equals(QuestState.CAN_START) && _startPoint)
        {
            GameEventsManager.QuestEvents.StartQuest(questId);
        }
        if (currentQuestState.Equals(QuestState.CAN_FINISH) && _finishPoint)
        {
            GameEventsManager.QuestEvents.FinishQuest(questId);
        }
    }

    private void QuestStateChange(Quest quest)
    {
        if (quest.Info.ID.Equals(questId))
        {
            currentQuestState = quest.State;
            _questIcon.SetState(currentQuestState, _startPoint, _finishPoint);
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
}
