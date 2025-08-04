using UnityEngine;

public class CollectItemQuestItemSpawner: MonoBehaviour, IItemSpawner
{
    [SerializeField] private QuestData _questData;
    [SerializeField] private Vector3 _offset;

    private string _itemId;

    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        if (GameEventsManager.TryGetEvents<QuestEvents>(out var questEvents))
        {
            questEvents.OnQuestStateChange -= QuestStateChange;
        }
    }
    
    public bool TrySpawn()
    {
        Spawn();
        return true;
    }

    public void Spawn()
    {
        var prefab = ItemRegistry.Instance.Get(_itemId);
        Instantiate(prefab, transform.position + _offset, transform.rotation, null);
    }

    private void QuestStateChange(AQuest quest)
    {
        var collectQuest = quest as CollectItemQuest;
        
        if (collectQuest != null && collectQuest.Info.ID == _questData.ID)
        {
            if (collectQuest.State == EQuestState.IN_PROGRESS)
            {
                _itemId = collectQuest.TargetItemId;
                Spawn();
            }
        }
    }
}