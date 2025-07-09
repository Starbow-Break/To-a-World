using System;
using UnityEngine;

public class SeatBeltQuest : AQuest
{
    private void OnEnable()
    {
        GameEventsManager.GetEvents<SeatBeltEvents>().OnConnect += OnConnect;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<SeatBeltEvents>().OnConnect -= OnConnect;
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        
    }

    private void OnConnect()
    {
        CompleteQuest();
    }
}
