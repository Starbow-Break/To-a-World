using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.2f, 0.6f, 0.8f)]
[TrackClipType(typeof(NpcTalkClip))]
[TrackBindingType(typeof(Npc))]
public class NpcTalkTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<NpcTalkMixerBehaviour>.Create(graph, inputCount);
        
        var director = go.GetComponent<PlayableDirector>();
        mixer.GetBehaviour().npc = director.GetGenericBinding(this) as Npc;
        
        return mixer;
    }
}
