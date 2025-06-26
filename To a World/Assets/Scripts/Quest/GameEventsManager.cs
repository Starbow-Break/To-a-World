using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public static QuestEvents QuestEvents => Instance._questEvents;
    public static CoinEvents CoinEvents => Instance._coinEvents;
    
    private QuestEvents _questEvents;
    private CoinEvents _coinEvents;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        
        _questEvents = new QuestEvents();
        _coinEvents = new CoinEvents();
    }
}
