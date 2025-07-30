using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private string _itemId;
    private bool _isReady = false;

    private void OnEnable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange -= QuestStateChange;
    }

    private void QuestStateChange(AQuest quest)
    {
        Debug.Log(quest.Info.ID);
        var placeItemQuest = quest as PlaceItemQuest;
        if (placeItemQuest != null)
        {
            if (placeItemQuest.State == EQuestState.IN_PROGRESS)
            {
                ReadySpawn(placeItemQuest.TargetItemId);
            }
        }
    }

    private void ReadySpawn(string itemId)
    {
        _itemId = itemId;
        _isReady = true;
        Debug.Log($"{_itemId} is Ready");
    }

    public void Spawn()
    {
        if (_isReady)
        {
            var prefab = ItemRegistry.Instance.Get(_itemId);
            Instantiate(prefab, transform.position, transform.rotation, null);
            _isReady = false;
        }
    }
}
