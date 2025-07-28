using UnityEngine;

[CreateAssetMenu(fileName = "CollectItemQuestParams", 
    menuName = "ScriptableObjects/QuestStepParams/CollectItemQuestParams", 
    order = 3)]
public class CollectItemQuestParams : AQuestParams
{
    [field: SerializeField] public string ItemID { get; private set; }
    [field: SerializeField] public int Quantity { get; private set; }
}
