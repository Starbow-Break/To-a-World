using UnityEngine;

public class PlaceableItem: Item
{
    [SerializeField, Min(0.0f)] private float _destroyDelayWhenFinishQuest = 3f;
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

    private void QuestStateChange(AQuest quest)
    {
        if (quest is PlaceItemQuest piQuest)
        {
            if (piQuest.State == EQuestState.FINISHED && piQuest.TargetItemId == ID)
            {
                DestroyItem();
            }
        }
    }

    private void DestroyItem()
    {
        Destroy(gameObject, _destroyDelayWhenFinishQuest);
    }
}