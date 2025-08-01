using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TalkMicStateParams", menuName = "ScriptableObjects/TalkMicStateParams", order = int.MaxValue)]
public class TalkMicStateParams: ScriptableObject
{
    [Header("Button")]
    [field: SerializeField] public Color ButtonColor  { get; private set; }
    [field: SerializeField] public Sprite ButtonIconSprite  { get; private set; }
    
    [Header("Message")]
    [field: SerializeField] public string Message { get; private set; }
}