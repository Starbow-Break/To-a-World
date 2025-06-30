using UnityEngine;

public class QuestFactory: SceneSingleton<QuestFactory>
{
    private void Awake()
    {
        base.Awake();
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
