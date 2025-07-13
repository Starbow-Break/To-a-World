using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class NPCChatSystem : SceneSingleton<NPCChatSystem>
{
     [SerializeField] NPCChatManager _npcChatManager;
     
     public static NPCChatManager NPCChatManager => Instance?._npcChatManager;
}
