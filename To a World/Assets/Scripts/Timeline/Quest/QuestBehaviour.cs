using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class QuestBehaviour : PlayableBehaviour
{
    public string questId;
    public bool waitForCompletion;

    private PlayableDirector _director;
    private bool _isSubscribed = false;
    private bool _isWaitForQuestFinish = false;

    public override void OnGraphStart(Playable playable)
    {
        _director = (playable.GetGraph().GetResolver() as PlayableDirector);
        _isWaitForQuestFinish = false;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        GameEventsManager.GetEvents<QuestEvents>().StartQuest(questId);
        _isWaitForQuestFinish = true;
        
        if (waitForCompletion && !_isSubscribed)
        {
            GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest += OnFinishQuest;
            _isSubscribed = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!_isWaitForQuestFinish)
            return;
        double time = playable.GetTime();
        double duration = playable.GetDuration();
        if (playable.GetTime() < playable.GetDuration() - 0.01)
            return;
        
        _director.Pause();
        _isWaitForQuestFinish = true;
    }   
    
    private void OnFinishQuest(string obj)
    {
        if (obj != questId)
            return;
        
        GameEventsManager.GetEvents<QuestEvents>().OnFinishQuest -= OnFinishQuest;
        _isWaitForQuestFinish = false;
        
        if (_director.state == PlayState.Paused)
           _director.Resume();
    }
}
