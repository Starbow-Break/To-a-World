using UnityEngine;
using UnityEngine.XR.Hands.Gestures;

public class HandGestureItemSpawner : ItemSpawner
{
    [SerializeField] private StaticHandGesture _handGesture;
    
    private string _itemId;
    private bool _isReady = false;

    private void OnEnable()
    {
        _handGesture.GesturePerformed.AddListener(TrySpawn);
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        _handGesture.GesturePerformed.RemoveAllListeners();
        GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange -= QuestStateChange;
    }
    
    public void TrySpawn()
    {
        if (_isReady)
        {
            Spawn();
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
        }
    }

    private void ReadySpawn(string itemId)
    {
        _itemId = itemId;
        _isReady = true;
    }
    
    private void Spawn()
    {
        var prefab = ItemRegistry.Instance.Get(_itemId);
        Instantiate(prefab, transform.position, transform.rotation, null);
        _isReady = false;
    }
}
