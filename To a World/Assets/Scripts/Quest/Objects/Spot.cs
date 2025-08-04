using System;
using UnityEngine;

public class Spot : MonoBehaviour
{
    [SerializeField] private SpotData _spotData;
    [SerializeField] private GameObject _visualizer;

    public string ID { get; private set; }

    private void Awake()
    {
        ID = _spotData.ID;
    }

    private void Start()
    {
        SpotRegistry.Instance.Register(ID, this);
    }
    
    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += OnQuestStateChange;
    }
    
    private void OnDisable()
    {
        if (GameEventsManager.TryGetEvents<QuestEvents>(out var questEvents))
        {
            questEvents.OnQuestStateChange -= OnQuestStateChange;
        }
    }

    private void OnQuestStateChange(AQuest quest)
    {
        if (quest is SpotQuest apQuest)
        {
            if (apQuest != null && apQuest.TargetId == ID)
            {
                bool active = apQuest.State == EQuestState.IN_PROGRESS || apQuest.State == EQuestState.CAN_FINISH;
                _visualizer.SetActive(active);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<PlaceEvents>().Arrive(ID);
        }
    }
}
