using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TalkMicStateParams", menuName = "ScriptableObjects/TalkMicStateParams", order = int.MaxValue)]
public class TalkMicStateParams: ScriptableObject
{
    [field: SerializeField] public Color ButtonColor  { get; private set; }
    [field: SerializeField] public string Message { get; private set; }
}