using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// NPC 대화 히스토리 및 상태 관리 클래스
    /// 
    /// 주요 기능:
    /// - 대화 히스토리 관리 (추가, 삭제, 조회) - 메모리 내에서만 관리
    /// - NPC 정보 및 퀘스트 상태 관리
    /// - 대화 세션 관리
    /// - 서버에서 대화 내용을 관리하므로 클라이언트 저장 없음
    /// 
    /// 사용 방법:
    /// ```csharp
    /// var conversationManager = GetComponent<NPCConversationManager>();
    /// conversationManager.OnConversationUpdated += UpdateUI;
    /// conversationManager.AddMessage(ConversationSenders.PLAYER, "안녕하세요!");
    /// ```
    /// </summary>
    public class NPCConversationManager : MonoBehaviour
    {
        #region 설정 변수
        
        [Header("=== 대화 관리 설정 ===")]
        [SerializeField] 
        [Tooltip("최대 대화 히스토리 크기 (메모리 관리용)")]
        private int maxHistorySize = 50;
        
        [Header("=== 기본 NPC 정보 ===")]
        [SerializeField] 
        [Tooltip("기본 NPC 정보")]
        private NPCInfo defaultNPCInfo = new NPCInfo();
        
        #endregion
        
        #region 상태 변수
        
        /// <summary>현재 NPC 정보</summary>
        public NPCInfo CurrentNPC { get; private set; }
        
        /// <summary>현재 퀘스트 정보</summary>
        public QuestInfo CurrentQuest { get; private set; }
        
        /// <summary>대화 히스토리</summary>
        public List<ConversationMessage> ConversationHistory { get; private set; } = new List<ConversationMessage>();
        
        /// <summary>현재 대화 세션 ID</summary>
        public string CurrentSessionId { get; private set; }
        
        /// <summary>메모리 키 (기기 고유 식별자)</summary>
        public string MemoryKey { get; private set; }
        
        /// <summary>대화 히스토리 개수</summary>
        public int MessageCount => ConversationHistory.Count;
        
        /// <summary>마지막 메시지 시간</summary>
        public string LastMessageTime { get; private set; }
        
        #endregion
        
        #region 이벤트
        
        /// <summary>새 메시지 추가 이벤트</summary>
        public event Action<ConversationMessage> OnMessageAdded;
        
        /// <summary>대화 히스토리 업데이트 이벤트</summary>
        public event Action<List<ConversationMessage>> OnConversationUpdated;
        
        /// <summary>NPC 정보 변경 이벤트</summary>
        public event Action<NPCInfo> OnNPCInfoChanged;
        
        /// <summary>퀘스트 상태 변경 이벤트</summary>
        public event Action<QuestInfo> OnQuestStatusChanged;
        
        /// <summary>대화 세션 시작 이벤트</summary>
        public event Action<string> OnSessionStarted; // sessionId
        
        /// <summary>대화 세션 종료 이벤트</summary>
        public event Action<string> OnSessionEnded; // sessionId
        
        #endregion
        
        #region 내부 변수
        
        // 서버에서 대화 내용을 관리하므로 클라이언트 저장 불필요
        
        #endregion
        
        #region Unity 라이프사이클
        
        /// <summary>
        /// Unity Start 메서드
        /// 대화 매니저를 초기화합니다
        /// </summary>
        void Start()
        {
            InitializeConversationManager();
        }
        
        /// <summary>
        /// Unity Update 메서드
        /// 현재는 특별한 처리 없음 (서버가 대화 내용 관리)
        /// </summary>
        void Update()
        {
            // 서버에서 대화 내용을 관리하므로 클라이언트 저장 불필요
        }
        
        /// <summary>
        /// Unity OnDestroy 메서드
        /// 리소스 정리
        /// </summary>
        void OnDestroy()
        {
            // 서버에서 대화 내용을 관리하므로 클라이언트 저장 불필요
        }
        
        #endregion
        
        #region 초기화
        
        /// <summary>
        /// 대화 매니저를 초기화합니다
        /// </summary>
        private void InitializeConversationManager()
        {
            // 메모리 키 생성 (기기 고유 식별자)
            MemoryKey = SystemInfo.deviceUniqueIdentifier;
            
            // 기본 NPC 정보 설정
            if (string.IsNullOrEmpty(defaultNPCInfo.name))
            {
                defaultNPCInfo.name = "Kay";
                defaultNPCInfo.gender = NPCGender.FEMALE;
                defaultNPCInfo.personality = "Friendly and helpful";
                defaultNPCInfo.background = "Airplane crew";
                defaultNPCInfo.age = 25;
                defaultNPCInfo.voice_style = "normal";
            }
            
            CurrentNPC = defaultNPCInfo;
            
            // 새 세션 시작
            StartNewSession();
            
            // 기본 NPC 정보 설정
            SetNPCInfo(defaultNPCInfo);
            
            Debug.Log($"[NPCConversationManager] 초기화 완료 - NPC: {CurrentNPC.name}, 메모리 키: {MemoryKey}");
        }
        
        #endregion
        
        #region 대화 세션 관리
        
        /// <summary>
        /// 새 대화 세션을 시작합니다
        /// </summary>
        public void StartNewSession()
        {
            CurrentSessionId = Guid.NewGuid().ToString();
            LastMessageTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            Debug.Log($"[NPCConversationManager] 새 세션 시작: {CurrentSessionId}");
            OnSessionStarted?.Invoke(CurrentSessionId);
        }
        
        /// <summary>
        /// 현재 대화 세션을 종료합니다
        /// </summary>
        public void EndCurrentSession()
        {
            if (!string.IsNullOrEmpty(CurrentSessionId))
            {
                Debug.Log($"[NPCConversationManager] 세션 종료: {CurrentSessionId}");
                OnSessionEnded?.Invoke(CurrentSessionId);
                
                CurrentSessionId = null;
            }
        }
        
        #endregion
        
        #region 대화 히스토리 관리
        
        /// <summary>
        /// 대화 메시지를 추가합니다
        /// </summary>
        /// <param name="sender">발신자</param>
        /// <param name="message">메시지 내용</param>
        /// <param name="questId">퀘스트 ID (선택적)</param>
        public void AddMessage(ConversationSenders sender, string message, string questId = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[NPCConversationManager] 빈 메시지는 추가할 수 없습니다.");
                return;
            }
            
            var conversationMessage = new ConversationMessage(sender, message, questId);
            
            // 히스토리에 추가
            ConversationHistory.Add(conversationMessage);
            
            // 최대 크기 제한 확인
            if (ConversationHistory.Count > maxHistorySize)
            {
                RemoveOldestMessage();
            }
            
            // 마지막 메시지 시간 업데이트
            LastMessageTime = DateTimeOffset.FromUnixTimeSeconds((long)conversationMessage.timestamp).ToString("yyyy-MM-dd HH:mm:ss");
            
            Debug.Log($"[NPCConversationManager] 메시지 추가: {sender} - {message}");
            
            // 이벤트 알림
            OnMessageAdded?.Invoke(conversationMessage);
            OnConversationUpdated?.Invoke(ConversationHistory);
        }
        
        /// <summary>
        /// 가장 오래된 메시지를 제거합니다
        /// </summary>
        private void RemoveOldestMessage()
        {
            if (ConversationHistory.Count > 0)
            {
                var oldestMessage = ConversationHistory[0];
                ConversationHistory.RemoveAt(0);
                
                Debug.Log($"[NPCConversationManager] 가장 오래된 메시지 제거: {oldestMessage.sender} - {oldestMessage.message}");
            }
        }
        
        /// <summary>
        /// 대화 히스토리를 지웁니다
        /// </summary>
        public void ClearConversationHistory()
        {
            ConversationHistory.Clear();
            
            Debug.Log("[NPCConversationManager] 대화 히스토리 초기화");
            
            OnConversationUpdated?.Invoke(ConversationHistory);
        }
        
        /// <summary>
        /// 특정 발신자의 메시지만 가져옵니다
        /// </summary>
        /// <param name="sender">발신자</param>
        /// <returns>해당 발신자의 메시지 목록</returns>
        public List<ConversationMessage> GetMessagesBySender(ConversationSenders sender)
        {
            return ConversationHistory.Where(msg => msg.sender == sender).ToList();
        }
        
        /// <summary>
        /// 특정 퀘스트와 관련된 메시지만 가져옵니다
        /// </summary>
        /// <param name="questId">퀘스트 ID</param>
        /// <returns>해당 퀘스트 관련 메시지 목록</returns>
        public List<ConversationMessage> GetMessagesByQuest(string questId)
        {
            return ConversationHistory.Where(msg => msg.quest_id == questId).ToList();
        }
        
        /// <summary>
        /// 최근 N개의 메시지를 가져옵니다
        /// </summary>
        /// <param name="count">가져올 메시지 수</param>
        /// <returns>최근 메시지 목록</returns>
        public List<ConversationMessage> GetRecentMessages(int count)
        {
            if (count <= 0 || ConversationHistory.Count == 0)
            {
                return new List<ConversationMessage>();
            }
            
            int startIndex = Mathf.Max(0, ConversationHistory.Count - count);
            return ConversationHistory.GetRange(startIndex, ConversationHistory.Count - startIndex);
        }
        
        #endregion
        
        #region NPC 정보 관리
        
        /// <summary>
        /// NPC 정보를 설정합니다
        /// </summary>
        /// <param name="npcInfo">새 NPC 정보</param>
        public void SetNPCInfo(NPCInfo npcInfo)
        {
            if (npcInfo == null)
            {
                Debug.LogError("[NPCConversationManager] NPC 정보가 null입니다.");
                return;
            }
            
            CurrentNPC = npcInfo;
            
            Debug.Log($"[NPCConversationManager] NPC 정보 변경: {npcInfo.name} ({npcInfo.gender})");
            
            OnNPCInfoChanged?.Invoke(CurrentNPC);
        }
        
        /// <summary>
        /// NPC 정보를 기본값으로 리셋합니다
        /// </summary>
        public void ResetNPCInfo()
        {
            SetNPCInfo(defaultNPCInfo);
        }
        
        #endregion
        
        #region 퀘스트 관리
        
        /// <summary>
        /// 퀘스트 정보를 설정합니다
        /// </summary>
        /// <param name="questInfo">새 퀘스트 정보</param>
        public void SetQuestInfo(QuestInfo questInfo)
        {
            CurrentQuest = questInfo;
            
            if (questInfo != null)
            {
                Debug.Log($"[NPCConversationManager] 퀘스트 설정: {questInfo.name} ({questInfo.status})");
            }
            else
            {
                Debug.Log("[NPCConversationManager] 퀘스트 해제");
            }
            
            OnQuestStatusChanged?.Invoke(CurrentQuest);
        }
        
        /// <summary>
        /// 퀘스트 상태를 업데이트합니다
        /// </summary>
        /// <param name="newStatus">새 퀘스트 상태</param>
        public void UpdateQuestStatus(QuestStatus newStatus)
        {
            if (CurrentQuest != null)
            {
                CurrentQuest.status = newStatus;
                
                Debug.Log($"[NPCConversationManager] 퀘스트 상태 변경: {CurrentQuest.name} -> {newStatus}");
                
                OnQuestStatusChanged?.Invoke(CurrentQuest);
            }
        }
        
        /// <summary>
        /// 퀘스트를 완료 상태로 설정합니다
        /// </summary>
        public void CompleteQuest()
        {
            if (CurrentQuest != null)
            {
                UpdateQuestStatus(QuestStatus.COMPLETED);
                
                // 퀘스트 완료 메시지 추가
                AddMessage(ConversationSenders.SYSTEM, $"퀘스트 '{CurrentQuest.name}' 완료!", CurrentQuest.id);
            }
        }
        
        #endregion
        
        #region 데이터 지속성
        
        // 서버에서 대화 내용을 관리하므로 클라이언트 측 저장 기능 제거
        // 필요 시 서버 API를 통해 대화 히스토리 조회 가능
        
        #endregion
        
        #region 공개 유틸리티 메서드
        
        /// <summary>
        /// 대화 히스토리 통계를 가져옵니다
        /// </summary>
        /// <returns>대화 통계 정보</returns>
        public ConversationStats GetConversationStats()
        {
            var stats = new ConversationStats
            {
                totalMessages = ConversationHistory.Count,
                playerMessages = ConversationHistory.Count(msg => msg.sender == ConversationSenders.PLAYER),
                npcMessages = ConversationHistory.Count(msg => msg.sender == ConversationSenders.NPC),
                systemMessages = ConversationHistory.Count(msg => msg.sender == ConversationSenders.SYSTEM),
                hasActiveQuest = CurrentQuest != null,
                questStatus = CurrentQuest?.status.ToString() ?? "없음",
                lastMessageTime = LastMessageTime
            };
            
            return stats;
        }
        
        /// <summary>
        /// 대화 히스토리를 텍스트 형태로 내보냅니다
        /// </summary>
        /// <returns>텍스트 형태의 대화 히스토리</returns>
        public string ExportConversationAsText()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"=== NPC 대화 히스토리 ===");
            sb.AppendLine($"NPC: {CurrentNPC.name} ({CurrentNPC.gender})");
            sb.AppendLine($"퀘스트: {CurrentQuest?.name ?? "없음"}");
            sb.AppendLine($"총 메시지: {ConversationHistory.Count}개");
            sb.AppendLine($"세션 ID: {CurrentSessionId}");
            sb.AppendLine();
            
            foreach (var message in ConversationHistory)
            {
                string senderName = message.sender switch
                {
                    ConversationSenders.PLAYER => "플레이어",
                    ConversationSenders.NPC => CurrentNPC.name,
                    ConversationSenders.SYSTEM => "시스템",
                    _ => "알 수 없음"
                };
                
                sb.AppendLine($"[{message.timestamp}] {senderName}: {message.message}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 메모리 키를 새로 생성합니다
        /// </summary>
        public void RegenerateMemoryKey()
        {
            MemoryKey = Guid.NewGuid().ToString();
            
            Debug.Log($"[NPCConversationManager] 새 메모리 키 생성: {MemoryKey}");
        }
        
        #endregion
    }
    
    #region 데이터 구조체
    
    /// <summary>
    /// 대화 통계 구조체
    /// </summary>
    [Serializable]
    public class ConversationStats
    {
        public int totalMessages;
        public int playerMessages;
        public int npcMessages;
        public int systemMessages;
        public bool hasActiveQuest;
        public string questStatus;
        public string lastMessageTime;
    }
    
    #endregion
} 