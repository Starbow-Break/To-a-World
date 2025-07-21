using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class GameEventsManager : AInitializedSceneSingleton<GameEventsManager>
{
    private List<IEvents> _eventsList = new();
    
    public static T GetEvents<T>() where T : IEvents
    {
        if (Instance != null)
        {
            return Instance.GetEventsInternal<T>();
        }

        Debug.LogWarning("GameEventsManager의 인스턴스가 존재하지 않습니다.");
        return default;
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
