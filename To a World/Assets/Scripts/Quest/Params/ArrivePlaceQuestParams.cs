using UnityEngine;

[CreateAssetMenu(fileName = "ArrivePlaceQuestStepParams", 
    menuName = "ScriptableObjects/QuestStepParams/ArrivePlaceQuestStepParams", 
    order = 0)]
public class ArrivePlaceQuestParams: AQuestParams
{
    [field: SerializeField] public string TargetPlaceID { get; private set; }
}
