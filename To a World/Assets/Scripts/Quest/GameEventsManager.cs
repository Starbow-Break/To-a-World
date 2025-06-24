using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public static QuestEvents QuestEvents => Instance._questEvents;
    
    private QuestEvents _questEvents;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        
        _questEvents = new QuestEvents();
    }
}
