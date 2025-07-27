using System;
using UnityEngine;

public class ArrivePlaceQuest : AQuest
{
    public string TargetPointId { get; private set; }
    
    private void OnEnable()
    {
        GameEventsManager.GetEvents<PlaceEvents>().OnArrive += OnArrive;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<PlaceEvents>().OnArrive -= OnArrive;
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as ArrivePlaceQuestParams;
        if (param != null)
        {
            TargetPointId = param.TargetPlaceID;
        }
    }

    private void OnArrive(string placeId)
    {
        if (placeId == TargetPointId)
        {
            CompleteQuest();
        }
    }
    
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (String.IsNullOrEmpty(TargetPointId))
        {
            TargetPointId = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
