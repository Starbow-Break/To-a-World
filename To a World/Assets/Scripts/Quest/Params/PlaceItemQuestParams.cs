using UnityEngine;

[CreateAssetMenu(fileName = "PlaceItemQuestParams", 
    menuName = "ScriptableObjects/QuestStepParams/PlaceItemQuestParams", 
    order = 5)]
public class PlaceItemQuestParams: AQuestParams
{
    [field: SerializeField]
    public ItemData Item { get; private set; }
}
