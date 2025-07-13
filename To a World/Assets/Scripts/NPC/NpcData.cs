using System.Collections.Generic;
using NPCSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcInfo", menuName = "Scriptable Objects/NpcInfo", order = 3)]
public class NpcInfo : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public NPCGender Gender { get; private set; } = NPCGender.FEMALE;
    [field: SerializeField] public string Personality { get; private set; } = "친근하고 도움이 되는";
    [field: SerializeField] public string Background { get; private set; } = "게임 세계의 NPC";
    [field: SerializeField] public int Age { get; private set; } = 25;
    [field: SerializeField] public string VoiceStyle { get; private set; } = "자연스러운";

    [SerializeField] List<QuestData> _questDatas = new();

    public bool TryGetNpcQuest(string questId, out NpcQuest npcQuest)
    {
        npcQuest = null;
        
        foreach (var questData in _questDatas)
        {
            if (questData.ID == questId)
            {
                var quest = QuestManager.Instance.GetQuestById(questId);
                if (quest is NpcQuest)
                {
                    npcQuest = quest as NpcQuest;
                    return true;
                }
            }
        }

        return false;
    }
}
