using UnityEngine;

public class NPCChatSystem : NullSafeSceneSingleton<NPCChatSystem>
{
     [SerializeField] NPCChatManager _npcChatManager;

     public static NPCChatManager NPCChatManager
     {
          get
          {
               if (Instance != null)
               {
                    return Instance._npcChatManager;
               }
               
               Debug.LogWarning("NPCChatManager가 존재하지 않습니다.");
               return null;
          }
     }

     private void Awake()
     {
          InitializeSystem();
     }
     
     private void InitializeSystem()
     {
          NPCChatManager.ClearNPCMemory("Start New Scene");
     }
}
