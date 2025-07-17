using System;
using UnityEngine;

public class PlaceTargetPoint : MonoBehaviour
{
    [field: SerializeField]
    public string ID { get; private set; }
    
    // 테스트 용
    private void Start()
    {
        GameEventsManager.GetEvents<QuestEvents>().StartQuest("ArrivePlaceQuest_Npc");
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
