using UnityEngine.Playables;
using UnityEngine;

[System.Serializable]
public class NpcTalkBehaviour : PlayableBehaviour
{
    public string talkString;
    public Npc npc;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (npc == null)
        {
            Debug.LogWarning("NpcActionBehaviour: npc 참조가 설정되지 않았습니다.");
            return;
        }

        Debug.Log("SpeakText: " + talkString);
        npc.SpeakText(talkString);
    }
}