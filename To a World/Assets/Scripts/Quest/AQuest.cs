using UnityEngine;

public abstract class AQuest: MonoBehaviour
{
    [HideInInspector] public QuestInfo Info;
    [HideInInspector] public EQuestState State;

    public virtual void CompleteQuest()
    {
        GameEventsManager.GetEvents<QuestEvents>().FinishQuest(Info.ID);
    }
    
    public abstract void Initialize(AQuestParams questParams);
}
