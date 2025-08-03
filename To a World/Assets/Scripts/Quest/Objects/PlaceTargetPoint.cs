using System;
using UnityEngine;

public class PlaceTargetPoint : MonoBehaviour
{
    [field: SerializeField] public string ID { get; private set; }
    [SerializeField] private GameObject _visualizer;

    private void Start()
    {
        PlaceTargetPointRegistry.Instance.Register(ID, this);
    }
    
    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += OnQuestStateChange;
    }
    
    private void OnDisable()
    {
        var questEvents = GameEventsManager.GetEvents<QuestEvents>();
        if (questEvents != null)
        {
            questEvents.OnQuestStateChange -= OnQuestStateChange;
        }
    }

    private void OnQuestStateChange(AQuest quest)
    {
        if (quest is ArrivePlaceQuest apQuest)
        {
            if (apQuest != null && apQuest.TargetPointId == ID)
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
    
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (String.IsNullOrEmpty(ID))
        {
            ID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
