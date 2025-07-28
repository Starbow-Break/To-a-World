using UnityEngine;

[CreateAssetMenu(fileName = "CameraQuestParams", 
    menuName = "ScriptableObjects/QuestStepParams/CameraQuestParams", 
    order = 4)]
public class CameraQuestParams: AQuestParams
{
    [field: SerializeField] public string TargetID { get; private set; }
}
