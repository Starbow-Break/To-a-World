using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class NpcTalkClip : PlayableAsset, ITimelineClipAsset
{
    [TextArea]
    public string talkString = "Hello";
    
    public ClipCaps clipCaps => ClipCaps.None;
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<NpcTalkBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.talkString = talkString;
        
        return playable;
    }

}
