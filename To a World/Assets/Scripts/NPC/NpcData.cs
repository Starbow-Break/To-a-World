using UnityEngine;

[CreateAssetMenu(fileName = "NpcInfo", menuName = "Scriptable Objects/NpcInfo", order = 3)]
public class NpcInfo : ScriptableObject
{
    [field: SerializeField] public string Personality; // 성격
    [field: SerializeField] public string Tone; // 말투
    [field: SerializeField] public string Role; // 역할
    [field: SerializeField] public string Gender; // 성별
    [field: SerializeField] public string QuestDescription; // 퀘스트 내용
    [field: SerializeField] public string QuestCompletionConditions; // 퀘스트 완료 조건
    
    [Header("Legacy")]
    [field: SerializeField] public string Language; // 언어
    [field: SerializeField] public string Character; // 캐릭터
}
