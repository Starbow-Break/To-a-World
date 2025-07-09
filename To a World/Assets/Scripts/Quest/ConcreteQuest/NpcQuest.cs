using UnityEngine;

public class NpcQuest : AQuest
{
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as NpcQuestParams;
    }
}
