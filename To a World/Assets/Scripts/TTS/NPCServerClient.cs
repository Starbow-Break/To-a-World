using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace NPCSystem
{
    /// <summary>
    /// NPC 서버 클라이언트 클래스
    /// 
    /// 주요 기능:
    /// - NPC 음성 대화 API 요청 (STT → TTS)
    /// - NPC 선제 대화 API 요청
    /// - 실시간 SSE 스트리밍 처리
    /// - 서버 응답 데이터 파싱
    /// 
    /// 사용 방법:
    /// ```csharp
    /// var client = GetComponent<NPCServerClient>();
    /// client.OnSSEMessageReceived += HandleSSEMessage;
    /// client.SendChatAudioRequest(audioData, npcInfo, conversationHistory);
    /// ```
    /// </summary>
    public class NPCServerClient : MonoBehaviour
    {
        #region 설정 변수
        
        [Header("=== 서버 설정 ===")]
        [SerializeField] 
        [Tooltip("NPC 서버 URL")]
        private string serverUrl = "http://localhost:8000";
        
        [SerializeField] 
        [Tooltip("요청 타임아웃 시간 (초)")]
        private float timeoutSeconds = 30f;
        
        [SerializeField] 
        [Tooltip("재시도 횟수")]
        private int retryCount = 3;
        
        [SerializeField] 
        [Tooltip("재시도 간격 (초)")]
        private float retryDelay = 1f;
        
        #endregion
        
        #region 상태 변수
        
        /// <summary>현재 요청 중인지 여부</summary>
        public bool IsRequestActive { get; private set; }
        
        /// <summary>현재 SSE 스트리밍 중인지 여부</summary>
        public bool IsStreamingActive { get; private set; }
        
        /// <summary>서버 연결 상태</summary>
        public bool IsServerConnected { get; private set; }
        
        /// <summary>현재 스트리밍 요청 참조</summary>
        public UnityWebRequest CurrentStreamRequest { get; private set; }
        
        #endregion
        
        #region 이벤트
        
        /// <summary>SSE 메시지 수신 이벤트</summary>
        public event Action<string, string> OnSSEMessageReceived; // type, jsonData
        
        /// <summary>요청 시작 이벤트</summary>
        public event Action<string> OnRequestStarted; // requestType
        
        /// <summary>요청 완료 이벤트</summary>
        public event Action<string> OnRequestCompleted; // requestType
        
        /// <summary>서버 오류 이벤트</summary>
        public event Action<string> OnServerError; // errorMessage
        
        /// <summary>서버 연결 상태 변경 이벤트</summary>
        public event Action<bool> OnServerConnectionChanged; // isConnected
        
        #endregion
        
        #region 내부 변수
        
        /// <summary>SSE 메시지 처리 큐</summary>
        private Queue<SSEMessage> sseMessageQueue = new Queue<SSEMessage>();
        
        /// <summary>큐 동기화 객체</summary>
        private readonly object queueLock = new object();
        
        /// <summary>현재 요청 타입</summary>
        private string currentRequestType;
        
        #endregion
        
        #region 내부 구조체
        
        /// <summary>
        /// SSE 메시지 구조체
        /// </summary>
        private struct SSEMessage
        {
            public string type;
            public string jsonData;
            
            public SSEMessage(string type, string jsonData)
            {
                this.type = type;
                this.jsonData = jsonData;
            }
        }
        
        #endregion
        
        #region Unity 라이프사이클
        
        /// <summary>
        /// Unity Update 메서드
        /// 큐에 있는 SSE 메시지들을 처리합니다
        /// </summary>
        void Update()
        {
            ProcessQueuedSSEMessages();
        }
        
        /// <summary>
        /// Unity OnDestroy 메서드
        /// 진행 중인 요청들을 정리합니다
        /// </summary>
        void OnDestroy()
        {
            CleanupResources();
        }
        
        #endregion
        
        #region 서버 연결 테스트
        
        /// <summary>
        /// 서버 연결 상태를 테스트합니다
        /// </summary>
        /// <returns>연결 테스트 코루틴</returns>
        public IEnumerator TestServerConnection()
        {
            string testUrl = $"{serverUrl}/health";
            
            using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
            {
                request.timeout = 5; // 짧은 타임아웃
                
                yield return request.SendWebRequest();
                
                bool wasConnected = IsServerConnected;
                IsServerConnected = request.result == UnityWebRequest.Result.Success;
                
                if (wasConnected != IsServerConnected)
                {
                    OnServerConnectionChanged?.Invoke(IsServerConnected);
                }
                
                if (IsServerConnected)
                {
                    Debug.Log("[NPCServerClient] 서버 연결 확인됨");
                }
                else
                {
                    Debug.LogWarning($"[NPCServerClient] 서버 연결 실패: {request.error}");
                }
            }
        }
        
        #endregion
        
        #region NPC 음성 대화 API
        
        /// <summary>
        /// NPC 음성 대화 요청을 전송합니다 (실시간 스트리밍)
        /// </summary>
        /// <param name="audioData">오디오 데이터 (Base64 문자열)</param>
        /// <param name="npcInfo">NPC 정보</param>
        /// <param name="conversationHistory">대화 히스토리</param>
        /// <param name="questInfo">퀘스트 정보 (선택적)</param>
        /// <param name="memoryKey">메모리 키</param>
        /// <param name="language">언어 코드</param>
        /// <param name="useThinking">사고 과정 사용 여부</param>
        /// <returns>요청 코루틴</returns>
        public IEnumerator SendChatAudioRequest(
            string audioData, 
            NPCInfo npcInfo, 
            List<ConversationMessage> conversationHistory, 
            QuestInfo questInfo = null, 
            string memoryKey = "", 
            string language = "en", 
            bool useThinking = false)
        {
            if (IsRequestActive)
            {
                Debug.LogWarning("[NPCServerClient] 이미 요청이 진행 중입니다.");
                yield break;
            }
            
            currentRequestType = "chat_audio";
            
            // 요청 데이터 생성
            var requestData = CreateChatAudioRequestData(
                audioData, npcInfo, conversationHistory, questInfo, memoryKey, language, useThinking);
            
            // 실시간 스트리밍 요청 전송
            yield return StartCoroutine(SendStreamingRequest($"{serverUrl}/npc/chat_audio", requestData));
        }
        
        /// <summary>
        /// NPC 음성 대화 요청 데이터를 생성합니다
        /// </summary>
        private Dictionary<string, object> CreateChatAudioRequestData(
            string audioData, 
            NPCInfo npcInfo, 
            List<ConversationMessage> conversationHistory, 
            QuestInfo questInfo, 
            string memoryKey, 
            string language, 
            bool useThinking)
        {
            var requestData = new Dictionary<string, object>
            {
                ["audio_data"] = audioData,
                ["npc"] = new Dictionary<string, object>
                {
                    ["name"] = npcInfo.name ?? "",
                    ["gender"] = npcInfo.gender.ToString().ToLower(),
                    ["personality"] = npcInfo.personality ?? "",
                    ["background"] = npcInfo.background ?? "",
                    ["age"] = npcInfo.age ?? 25,
                    ["voice_style"] = npcInfo.voice_style ?? "normal"
                },
                ["conversation_history"] = ConvertConversationHistoryToDict(conversationHistory),
                ["memory_key"] = memoryKey,
                ["language"] = language,
                ["use_thinking"] = useThinking,
                ["audio_format"] = "wav"
            };
            
            // 퀘스트 정보 추가 (선택적)
            if (questInfo != null)
            {
                requestData["quest"] = new Dictionary<string, object>
                {
                    ["id"] = questInfo.id ?? "",
                    ["name"] = questInfo.name ?? "",
                    ["description"] = questInfo.description ?? "",
                    ["completion_condition"] = questInfo.completion_condition ?? "",
                    ["reward"] = questInfo.reward ?? "",
                    ["difficulty"] = questInfo.difficulty ?? "normal",
                    ["status"] = questInfo.status.ToString().ToLower()
                };
            }
            
            return requestData;
        }
        
        #endregion
        
        #region NPC 텍스트 대화 API
        
        /// <summary>
        /// NPC 텍스트 대화 요청을 전송합니다 (실시간 스트리밍)
        /// </summary>
        /// <param name="textMessage">플레이어가 보낸 텍스트 메시지</param>
        /// <param name="npcInfo">NPC 정보</param>
        /// <param name="conversationHistory">대화 히스토리</param>
        /// <param name="questInfo">퀘스트 정보 (선택적)</param>
        /// <param name="memoryKey">메모리 키</param>
        /// <param name="language">언어 코드</param>
        /// <param name="useThinking">사고 과정 사용 여부</param>
        /// <returns>요청 코루틴</returns>
        public IEnumerator SendChatTextRequest(
            string textMessage, 
            NPCInfo npcInfo, 
            List<ConversationMessage> conversationHistory, 
            QuestInfo questInfo = null, 
            string memoryKey = "", 
            string language = "en", 
            bool useThinking = false)
        {
            if (IsRequestActive)
            {
                Debug.LogWarning("[NPCServerClient] 이미 요청이 진행 중입니다.");
                yield break;
            }
            
            currentRequestType = "chat_text";
            
            // 요청 데이터 생성
            var requestData = CreateChatTextRequestData(
                textMessage, npcInfo, conversationHistory, questInfo, memoryKey, language, useThinking);
            
            // 스트리밍 요청 전송
            yield return StartCoroutine(SendStreamingRequest($"{serverUrl}/npc/chat_text", requestData));
        }
        
        /// <summary>
        /// NPC 텍스트 대화 요청 데이터를 생성합니다
        /// </summary>
        private Dictionary<string, object> CreateChatTextRequestData(
            string textMessage, 
            NPCInfo npcInfo, 
            List<ConversationMessage> conversationHistory, 
            QuestInfo questInfo, 
            string memoryKey, 
            string language, 
            bool useThinking)
        {
            // 디버그 로그 추가
            Debug.Log($"[NPCServerClient] CreateChatTextRequestData - textMessage: '{textMessage}', memoryKey: '{memoryKey}'");
            Debug.Log($"[NPCServerClient] NPC Info - name: '{npcInfo.name}', gender: '{npcInfo.gender}', personality: '{npcInfo.personality}'");
            
            // memory_key가 빈 문자열이면 기본값 설정
            if (string.IsNullOrEmpty(memoryKey))
            {
                memoryKey = SystemInfo.deviceUniqueIdentifier;
                Debug.LogWarning($"[NPCServerClient] memory_key가 비어있어 기본값으로 설정: {memoryKey}");
            }
            
            var requestData = new Dictionary<string, object>
            {
                {"text", textMessage},
                {"npc", new Dictionary<string, object>
                {
                    {"name", npcInfo.name ?? ""},
                    {"gender", npcInfo.gender.ToString().ToLower()},
                    {"personality", npcInfo.personality ?? ""},
                    {"background", npcInfo.background ?? ""},
                    {"age", npcInfo.age ?? 25},
                    {"voice_style", npcInfo.voice_style ?? "normal"}
                }},
                {"conversation_history", ConvertConversationHistoryToDict(conversationHistory)},
                {"memory_key", memoryKey},
                {"language", language},
                {"use_thinking", useThinking}
            };
            
            // 퀘스트 정보 추가 (선택적)
            if (questInfo != null)
            {
                requestData["quest"] = new Dictionary<string, object>
                {
                    {"id", questInfo.id ?? ""},
                    {"name", questInfo.name ?? ""},
                    {"description", questInfo.description ?? ""},
                    {"completion_condition", questInfo.completion_condition ?? ""},
                    {"reward", questInfo.reward ?? ""},
                    {"difficulty", questInfo.difficulty ?? "normal"},
                    {"status", questInfo.status.ToString().ToLower()}
                };
            }
            
            return requestData;
        }
        
        #endregion
        
        #region NPC 선제 대화 API
        
        /// <summary>
        /// NPC 선제 대화 요청을 전송합니다 (실시간 스트리밍)
        /// </summary>
        /// <param name="initialMessage">초기 메시지</param>
        /// <param name="npcInfo">NPC 정보</param>
        /// <param name="conversationHistory">대화 히스토리</param>
        /// <param name="questInfo">퀘스트 정보 (선택적)</param>
        /// <param name="memoryKey">메모리 키</param>
        /// <param name="language">언어 코드</param>
        /// <param name="useThinking">사고 과정 사용 여부</param>
        /// <returns>요청 코루틴</returns>
        public IEnumerator SendInitiateChatRequest(
            string initialMessage, 
            NPCInfo npcInfo, 
            List<ConversationMessage> conversationHistory, 
            QuestInfo questInfo = null, 
            string memoryKey = "", 
            string language = "en", 
            bool useThinking = false)
        {
            if (IsRequestActive)
            {
                Debug.LogWarning("[NPCServerClient] 이미 요청이 진행 중입니다.");
                yield break;
            }
            
            currentRequestType = "initiate_chat";
            
            // 요청 데이터 생성
            var requestData = CreateInitiateChatRequestData(
                initialMessage, npcInfo, conversationHistory, questInfo, memoryKey, language, useThinking);
            
            // 실시간 스트리밍 요청 전송
            yield return StartCoroutine(SendStreamingRequest($"{serverUrl}/npc/initiate_chat", requestData));
        }
        
        /// <summary>
        /// NPC 선제 대화 요청 데이터를 생성합니다
        /// </summary>
        private Dictionary<string, object> CreateInitiateChatRequestData(
            string initialMessage, 
            NPCInfo npcInfo, 
            List<ConversationMessage> conversationHistory, 
            QuestInfo questInfo, 
            string memoryKey, 
            string language, 
            bool useThinking)
        {
            var requestData = new Dictionary<string, object>
            {
                ["initial_message"] = initialMessage,
                ["npc"] = new Dictionary<string, object>
                {
                    ["name"] = npcInfo.name ?? "",
                    ["gender"] = npcInfo.gender.ToString().ToLower(),
                    ["personality"] = npcInfo.personality ?? "",
                    ["background"] = npcInfo.background ?? "",
                    ["age"] = npcInfo.age ?? 25,
                    ["voice_style"] = npcInfo.voice_style ?? "normal"
                },
                ["conversation_history"] = ConvertConversationHistoryToDict(conversationHistory),
                ["memory_key"] = memoryKey,
                ["language"] = language,
                ["use_thinking"] = useThinking
            };
            
            // 퀘스트 정보 추가 (선택적)
            if (questInfo != null)
            {
                requestData["quest"] = new Dictionary<string, object>
                {
                    ["id"] = questInfo.id ?? "",
                    ["name"] = questInfo.name ?? "",
                    ["description"] = questInfo.description ?? "",
                    ["completion_condition"] = questInfo.completion_condition ?? "",
                    ["reward"] = questInfo.reward ?? "",
                    ["difficulty"] = questInfo.difficulty ?? "normal",
                    ["status"] = questInfo.status.ToString().ToLower()
                };
            }
            
            return requestData;
        }
        
        #endregion
        
        #region NPC 메모리 삭제 API
        
        /// <summary>
        /// NPC 대화 기록 삭제 요청을 전송합니다 (실시간 스트리밍)
        /// </summary>
        /// <param name="memoryKey">메모리 키 (기기 고유 ID)</param>
        /// <param name="reason">삭제 사유 (선택적)</param>
        /// <returns>요청 코루틴</returns>
        public IEnumerator SendClearMemoryRequest(string memoryKey, string reason = null)
        {
            if (IsRequestActive)
            {
                Debug.LogWarning("[NPCServerClient] 이미 요청이 진행 중입니다.");
                yield break;
            }
            
            currentRequestType = "clear_memory";
            
            // 요청 데이터 생성
            var requestData = CreateClearMemoryRequestData(memoryKey, reason);
            
            // 실시간 스트리밍 요청 전송
            yield return StartCoroutine(SendStreamingRequest($"{serverUrl}/npc/clear_memory", requestData));
        }
        
        /// <summary>
        /// NPC 메모리 삭제 요청 데이터를 생성합니다
        /// </summary>
        private Dictionary<string, object> CreateClearMemoryRequestData(string memoryKey, string reason)
        {
            Debug.Log($"[NPCServerClient] CreateClearMemoryRequestData - memoryKey: '{memoryKey}', reason: '{reason}'");
            
            var requestData = new Dictionary<string, object>
            {
                {"memory_key", memoryKey}
            };
            
            // 삭제 사유 추가 (선택적)
            if (!string.IsNullOrEmpty(reason))
            {
                requestData["reason"] = reason;
            }
            
            return requestData;
        }
        
        #endregion
        
        #region 스트리밍 요청 처리
        
        /// <summary>
        /// 실시간 스트리밍 요청을 전송합니다
        /// </summary>
        /// <param name="url">요청 URL</param>
        /// <param name="requestData">요청 데이터</param>
        /// <returns>요청 코루틴</returns>
        private IEnumerator SendStreamingRequest(string url, Dictionary<string, object> requestData)
        {
            IsRequestActive = true;
            IsStreamingActive = true;
            
            OnRequestStarted?.Invoke(currentRequestType);
            
            string jsonData = ConvertToProperJson(requestData);

            Debug.Log($"[NPCServerClient] 스트리밍 요청 시작: {url}");
            Debug.Log($"[NPCServerClient] 전송 데이터: {jsonData}");
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                // 요청 설정
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new StreamingDownloadHandler(this);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "text/event-stream");
                request.SetRequestHeader("Cache-Control", "no-cache");
                request.timeout = (int)timeoutSeconds;
                
                CurrentStreamRequest = request;
                
                // 요청 전송
                yield return request.SendWebRequest();
                
                // 결과 처리
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string error = $"스트리밍 요청 실패: {request.error} (응답 코드: {request.responseCode})";
                    string responseText = request.downloadHandler?.text ?? "응답 텍스트 없음";
                    Debug.LogError($"[NPCServerClient] {error}");
                    Debug.LogError($"[NPCServerClient] 서버 응답: {responseText}");
                    OnServerError?.Invoke(error);
                }
                else
                {
                    Debug.Log($"[NPCServerClient] 스트리밍 요청 완료: {currentRequestType}");
                }
                
                CurrentStreamRequest = null;
            }
            
            IsRequestActive = false;
            IsStreamingActive = false;
            
            OnRequestCompleted?.Invoke(currentRequestType);
        }
        
        /// <summary>
        /// 현재 스트리밍 요청을 중단합니다
        /// </summary>
        public void AbortCurrentRequest()
        {
            if (CurrentStreamRequest != null)
            {
                CurrentStreamRequest.Abort();
                CurrentStreamRequest = null;
            }
            
            IsRequestActive = false;
            IsStreamingActive = false;
            
            Debug.Log("[NPCServerClient] 스트리밍 요청 중단됨");
        }
        
        #endregion
        
        #region SSE 메시지 처리
        
        /// <summary>
        /// SSE 메시지를 큐에 추가합니다 (스레드 안전)
        /// </summary>
        /// <param name="jsonData">SSE 메시지 JSON 데이터</param>
        internal void EnqueueSSEMessage(string jsonData)
        {
            try
            {
                // 응답 타입 확인
                var baseResponse = JsonUtility.FromJson<BaseResponse>(jsonData);
                string responseType = baseResponse.type;
                
                lock (queueLock)
                {
                    sseMessageQueue.Enqueue(new SSEMessage(responseType, jsonData));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NPCServerClient] SSE 메시지 큐 추가 오류: {e.Message}");
            }
        }
        
        /// <summary>
        /// 큐에 있는 SSE 메시지들을 처리합니다
        /// </summary>
        private void ProcessQueuedSSEMessages()
        {
            lock (queueLock)
            {
                while (sseMessageQueue.Count > 0)
                {
                    var message = sseMessageQueue.Dequeue();
                    
                    try
                    {
                        Debug.Log($"[NPCServerClient] SSE 메시지 처리: {message.type}");
                        
                        // 이벤트 알림
                        OnSSEMessageReceived?.Invoke(message.type, message.jsonData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[NPCServerClient] SSE 메시지 처리 오류: {e.Message}");
                    }
                }
            }
        }
        
        #endregion
        
        #region 유틸리티 메서드
        
        /// <summary>
        /// 대화 히스토리를 서버 호환 Dictionary 배열로 변환합니다
        /// </summary>
        /// <param name="history">대화 히스토리</param>
        /// <returns>Dictionary 배열</returns>
        private object[] ConvertConversationHistoryToDict(List<ConversationMessage> history)
        {
            var result = new object[history.Count];
            for (int i = 0; i < history.Count; i++)
            {
                var msg = history[i];
                result[i] = new Dictionary<string, object>
                {
                    ["sender"] = msg.sender.ToString().ToLower(),
                    ["message"] = msg.message ?? "",
                    ["timestamp"] = msg.timestamp,
                    ["quest_id"] = msg.quest_id ?? ""
                };
            }
            
            // 대화 히스토리 로그 출력
            Debug.Log($"[NPCServerClient] 대화 히스토리 변환: {history.Count}개 메시지");
            for (int i = 0; i < history.Count; i++)
            {
                var msg = history[i];
                Debug.Log($"[NPCServerClient] 메시지 {i}: {msg.sender} - '{msg.message}' (timestamp: {msg.timestamp})");
            }
            
            return result;
        }
        
        /// <summary>
        /// Dictionary를 JSON 문자열로 변환합니다
        /// </summary>
        /// <param name="data">변환할 Dictionary</param>
        /// <returns>JSON 문자열</returns>
        private string ConvertToProperJson(Dictionary<string, object> data)
        {
            var json = new StringBuilder();
            json.Append("{");
            
            bool first = true;
            foreach (var kvp in data)
            {
                if (!first) json.Append(",");
                first = false;
                
                json.Append($"\"{kvp.Key}\":");
                json.Append(SerializeValue(kvp.Value));
            }
            
            json.Append("}");
            return json.ToString();
        }
        
        /// <summary>
        /// 값을 JSON 형식으로 직렬화합니다
        /// </summary>
        /// <param name="value">직렬화할 값</param>
        /// <returns>JSON 문자열</returns>
        private string SerializeValue(object value)
        {
            if (value == null) return "null";
            
            if (value is string str)
            {
                return $"\"{str.Replace("\"", "\\\"")}\"";
            }
            
            if (value is bool b)
            {
                return b ? "true" : "false";
            }
            
            if (value is int || value is float || value is double)
            {
                return value.ToString();
            }
            
            if (value is Dictionary<string, object> dict)
            {
                var json = new StringBuilder();
                json.Append("{");
                
                bool first = true;
                foreach (var kvp in dict)
                {
                    if (!first) json.Append(",");
                    first = false;
                    
                    json.Append($"\"{kvp.Key}\":");
                    json.Append(SerializeValue(kvp.Value));
                }
                
                json.Append("}");
                return json.ToString();
            }
            
            if (value is object[] array)
            {
                var json = new StringBuilder();
                json.Append("[");
                
                for (int i = 0; i < array.Length; i++)
                {
                    if (i > 0) json.Append(",");
                    json.Append(SerializeValue(array[i]));
                }
                
                json.Append("]");
                return json.ToString();
            }
            
            return JsonUtility.ToJson(value);
        }
        
        #endregion
        
        #region 리소스 정리
        
        /// <summary>
        /// 모든 리소스를 정리합니다
        /// </summary>
        private void CleanupResources()
        {
            // 진행 중인 요청 중단
            AbortCurrentRequest();
            
            // 큐 정리
            lock (queueLock)
            {
                sseMessageQueue.Clear();
            }
        }
        
        #endregion
        
        #region 공개 설정 메서드
        
        /// <summary>
        /// 서버 URL을 설정합니다
        /// </summary>
        /// <param name="url">새 서버 URL</param>
        public void SetServerUrl(string url)
        {
            if (IsRequestActive)
            {
                Debug.LogWarning("[NPCServerClient] 요청 중에는 서버 URL을 변경할 수 없습니다.");
                return;
            }
            
            serverUrl = url;
            Debug.Log($"[NPCServerClient] 서버 URL 변경: {url}");
        }
        
        /// <summary>
        /// 타임아웃 시간을 설정합니다
        /// </summary>
        /// <param name="seconds">타임아웃 시간 (초)</param>
        public void SetTimeout(float seconds)
        {
            timeoutSeconds = seconds;
            Debug.Log($"[NPCServerClient] 타임아웃 시간 변경: {seconds}초");
        }
        
        #endregion
    }
    
    #region 스트리밍 다운로드 핸들러
    
    /// <summary>
    /// 실시간 스트리밍 다운로드 핸들러
    /// SSE 메시지를 실시간으로 처리합니다
    /// </summary>
    public class StreamingDownloadHandler : DownloadHandlerScript
    {
        private NPCServerClient serverClient;
        private StringBuilder dataBuffer = new StringBuilder();
        
        /// <summary>
        /// 스트리밍 다운로드 핸들러 생성자
        /// </summary>
        /// <param name="client">NPC 서버 클라이언트</param>
        public StreamingDownloadHandler(NPCServerClient client) : base()
        {
            serverClient = client;
        }
        
        /// <summary>
        /// 데이터 수신 처리
        /// </summary>
        /// <param name="data">수신된 데이터</param>
        /// <param name="dataLength">데이터 길이</param>
        /// <returns>처리 성공 여부</returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            try
            {
                // 바이트 데이터를 문자열로 변환
                string receivedData = Encoding.UTF8.GetString(data, 0, dataLength);
                dataBuffer.Append(receivedData);
                
                // 완성된 SSE 메시지들을 처리
                string bufferContent = dataBuffer.ToString();
                string[] lines = bufferContent.Split('\n');
                
                // 마지막 줄이 완성되지 않았을 수 있으므로 보관
                dataBuffer.Clear();
                
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    
                    if (i == lines.Length - 1 && !line.EndsWith("\n"))
                    {
                        // 마지막 줄이 완성되지 않았으면 다음에 처리하기 위해 보관
                        dataBuffer.Append(line);
                        break;
                    }
                    
                    if (line.StartsWith("data: "))
                    {
                        string jsonData = line.Substring(6); // "data: " 제거
                        
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            // 메인 스레드에서 처리하도록 큐에 추가
                            serverClient.EnqueueSSEMessage(jsonData);
                        }
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[StreamingDownloadHandler] 데이터 처리 오류: {e.Message}");
                return false;
            }
        }
    }
    
    #endregion
    
    #region 기본 응답 구조체
    
    /// <summary>
    /// 기본 응답 구조체 (타입 확인용)
    /// </summary>
    [Serializable]
    public class BaseResponse
    {
        public string type;
    }
    
    #endregion
} 