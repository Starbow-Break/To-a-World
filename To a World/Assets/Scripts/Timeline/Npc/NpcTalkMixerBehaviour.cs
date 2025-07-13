using UnityEngine;
using UnityEngine.Playables;

public class NpcTalkMixerBehaviour : PlayableBehaviour
{
    [HideInInspector] public Npc npc;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            var inputPlayable = (ScriptPlayable<NpcTalkBehaviour>) playable.GetInput(i);
            var behaviour = inputPlayable.GetBehaviour();
            behaviour.npc = npc;
        }
    }
}
