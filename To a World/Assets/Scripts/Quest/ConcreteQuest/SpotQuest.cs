using System;
using UnityEngine;

public class SpotQuest : AQuest
{
    public string TargetId { get; private set; }
    
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
            TargetId = param.Spot.ID;
        }
    }

    private void OnArrive(string placeId)
    {
        if (placeId == TargetId)
        {
            CompleteQuest();
        }
    }
}
