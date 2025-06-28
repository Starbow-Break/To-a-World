using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectCoinsQuestStepParams", 
    menuName = "ScriptableObjects/QuestStepParams/CollectCoinsQuestStepParams", 
    order = int.MaxValue)]
public class CollectCoinsQuestParams: AQuestParams
{
    [field: SerializeField] public int CoinsToComplete { get; private set; }
}
