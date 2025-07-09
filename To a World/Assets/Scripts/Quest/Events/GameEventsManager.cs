using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    private List<IEvents> _eventsList = new();
    
    public static T GetEvents<T>() where T : IEvents
        => Instance.GetEventsInternal<T>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        
        _eventsList.Add(new QuestEvents());
        _eventsList.Add(new CoinEvents());
        _eventsList.Add(new PlaceEvents());
        _eventsList.Add(new NpcEvents());
        _eventsList.Add(new SeatBeltEvents());
    }
    
    private T GetEventsInternal<T>() where T : IEvents
    {
        foreach (var e in _eventsList)
        {
            if (e is T events)
            {
                return events;
            }
        }

        return default;
    }
}
