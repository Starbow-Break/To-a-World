using UnityEngine;

public class QuestFactory: MonoBehaviour
{
    public static QuestFactory Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);    
    }
    
    public static AQuest CreateQuest(EQuestType type, AQuestParams param, Transform parent = null)
    {
        var questPrefab = QuestPrefabRegistry.Instance.Get(type);
        if (questPrefab != null)
        {
            var quest = Instantiate(questPrefab, parent)
                .GetComponent<AQuest>();
            quest.Initialize(param);
            return quest;
        }

        return null;
    }
}
