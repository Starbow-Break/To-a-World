using UnityEngine;

public class HandGestureItemSpawner : MonoBehaviour, IItemSpawner
{
    [SerializeField] private StaticHandGesture _handGesture;
    
    private string _itemId;
    private bool _isReady;

    private void OnEnable()
    {
        _handGesture.GesturePerformed.AddListener(GesturePerformed);
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        _handGesture.GesturePerformed.RemoveListener(GesturePerformed);
        if (GameEventsManager.TryGetEvents<QuestEvents>(out var questEvents))
        {
            questEvents.OnQuestStateChange -= QuestStateChange;
        }
    }
    
    public bool TrySpawn()
    {
        if (_isReady)
        {
            Spawn();
            return true;
        }

        return false;
    }
    
    public void Spawn()
    {
        var prefab = ItemRegistry.Instance.Get(_itemId);
        Instantiate(prefab, transform.position, transform.rotation, null);
    }
    
    private void GesturePerformed()
    {
        if (TrySpawn())
        {
            _isReady = false;
        }
    }

    private void QuestStateChange(AQuest quest)
    {
        var placeItemQuest = quest as PlaceItemQuest;
        if (placeItemQuest != null)
        {
            if (placeItemQuest.State == EQuestState.IN_PROGRESS)
            {
                ReadySpawn(placeItemQuest.TargetItemId);
            }
            else if (placeItemQuest.State == EQuestState.CAN_FINISH || placeItemQuest.State == EQuestState.FINISHED)
            {
                TryReleaseSpawn(placeItemQuest.TargetItemId);
            }
        }
    }

    private void ReadySpawn(string itemId)
    {
        _itemId = itemId;
        _isReady = true;
    }

    private void TryReleaseSpawn(string itemId)
    {
        if (_itemId == itemId)
        {
            ReleaseSpawn();
        }
    }

    private void ReleaseSpawn()
    {
        _itemId = null;
        _isReady = false;
    }
}
