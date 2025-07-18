using UnityEngine;

public class QuestFactory: SceneSingleton<QuestFactory>
{
    [SerializeField] private QuestPrefabRegistry _questPrefabRegistry;
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    
    public static AQuest CreateQuest(EQuestType type, AQuestParams param, Transform parent = null)
        => Instance.CreateQuestInternal(type, param, parent);
    
    public AQuest CreateQuestInternal(EQuestType type, AQuestParams param, Transform parent = null)
    {
        var questPrefab = _questPrefabRegistry.Get(type);
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
