using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class QuestClip : PlayableAsset, ITimelineClipAsset
{
    public QuestData quest;
    public bool waitForCompletion = true;
    
    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<QuestBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.questId = quest.ID;
        behaviour.waitForCompletion = waitForCompletion;

        //var qe = questEvents.Resolve(graph.GetResolver());

        return playable;
    }

}
