using UnityEngine;

[CreateAssetMenu(fileName = "ArrivePlaceQuestStepParams", 
    menuName = "ScriptableObjects/QuestStepParams/ArrivePlaceQuestStepParams", 
    order = 0)]
public class ArrivePlaceQuestParams: AQuestParams
{
    [field: SerializeField] 
    public SpotData Spot { get; private set; }
}
