using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class QuestBehaviour : PlayableBehaviour
{
    public string questId;
    public bool waitForCompletion;

    private PlayableDirector director;
    private bool isSubscribed;

    public override void OnGraphStart(Playable playable)
    {
        director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        GameEventsManager.GetEvents<QuestEvents>().StartQuest(questId);

        if (waitForCompletion && isSubscribed)
        {
            GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest += OnFinishQuest;
            director.Pause();
            isSubscribed = true;
        }
    }

    private void OnFinishQuest(string obj)
    {
        if (obj != questId)
            return;
        
        GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest -= OnFinishQuest;
        director.Resume();
    }
}
