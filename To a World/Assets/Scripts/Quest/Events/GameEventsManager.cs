using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : AInitializedSceneSingleton<GameEventsManager>
{
    private List<IEvents> _eventsList = new();
    
    public static bool TryGetEvents<T>(out T events) where T : IEvents
    {
        events = default;
        
        if (Instance != null)
        {
            return Instance.TryGetEventsInternal(out events);
        }

        return false;
    }

    public static T GetEvents<T>() where T : IEvents
    {
        if (Instance != null)
        {
            return Instance.GetEventsInternal<T>();
        }

        throw new EventsNotFoundException($"{nameof(T)} not found.");
    }

    private void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    protected override void Initialize()
    {
        _eventsList.Add(new QuestEvents());
        _eventsList.Add(new PlaceEvents());
        _eventsList.Add(new NpcEvents());
        _eventsList.Add(new SeatBeltEvents());
        _eventsList.Add(new ItemEvents());
        _eventsList.Add(new CameraEvents());
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

        throw new EventsNotFoundException($"{nameof(T)} not found.");
    }

    private bool TryGetEventsInternal<T>(out T events) where T : IEvents
    {
        events = default;
        
        foreach (var e in _eventsList)
        {
            if (e is T tempEvents)
            {
                events = tempEvents;
                return true;
            }
        }

        return false;
    }
}