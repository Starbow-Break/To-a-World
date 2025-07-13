using System;
using System.Collections;
using UnityEngine;
using NPCSystem;

/// <summary>
/// NPC 대화 시스템 메인 매니저 (컴포넌트 조합 방식)
/// 
/// 주요 역할:
/// - 각 기능별 컴포넌트들을 조합하여 통합 관리
/// - 컴포넌트 간 통신 및 이벤트 중계
/// - 전체 시스템의 상태 관리
/// - 외부 API와 UI 인터페이스 제공
/// 
/// 구성 컴포넌트:
/// - AudioRecorder: 마이크 녹음 관리
/// - NPCServerClient: 서버 통신 처리
/// - StreamingAudioPlayer: 실시간 오디오 재생
/// - NPCConversationManager: 대화 히스토리 관리
/// - WavUtility: 오디오 변환 (정적 유틸리티)
/// 
/// 사용 방법:
/// ```csharp
/// var chatManager = GetComponent<NPCChatManager>();
/// chatManager.OnStatusChanged += UpdateUI;
/// chatManager.StartRecording();
/// ```
/// </summary>
[DefaultExecutionOrder(-1000)]
public class NPCChatManager : MonoBehaviour
{
    #region 컴포넌트 참조
    
    [Header("=== 컴포넌트 참조 ===")]
    [SerializeField] 
    [Tooltip("오디오 녹음 컴포넌트")]
    private AudioRecorder audioRecorder;
    
    [SerializeField] 
    [Tooltip("서버 클라이언트 컴포넌트")]
    private NPCServerClient serverClient;
    
    [SerializeField] 
    [Tooltip("오디오 재생 컴포넌트")]
    private StreamingAudioPlayer audioPlayer;
    
    [SerializeField] 
    [Tooltip("대화 관리 컴포넌트")]
    private NPCConversationManager conversationManager;
    
    #endregion
    
    #region 설정 변수
    
    [Header("=== 기본 설정 ===")]
    [SerializeField] 
    [Tooltip("서버 URL")]
    private string serverUrl = "http://localhost:8000";
    
    [SerializeField] 
    [Tooltip("언어 설정")]
    private string language = "en";
    
    [SerializeField] 
    [Tooltip("사고 과정 사용 여부")]
    private bool useThinking = false;
    
    [SerializeField] 
    [Tooltip("자동 컴포넌트 참조 설정")]
    private bool autoSetupComponents = true;
    
    #endregion
    
    #region 상태 프로퍼티
    
    /// <summary>현재 녹음 중인지 여부</summary>
    public bool IsRecording => audioRecorder != null && audioRecorder.IsRecording;
    
    /// <summary>현재 서버 처리 중인지 여부</summary>
    public bool IsProcessing => serverClient != null && serverClient.IsRequestActive;
    
    /// <summary>현재 오디오 재생 중인지 여부</summary>
    public bool IsPlayingAudio => audioPlayer != null && audioPlayer.IsSequentialPlayback;
    
    /// <summary>현재 NPC 정보</summary>
    public NPCInfo CurrentNPC => conversationManager != null ? conversationManager.CurrentNPC : null;
    
    /// <summary>현재 퀘스트 정보</summary>
    public NPCSystem.QuestInfo CurrentQuest => conversationManager != null ? conversationManager.CurrentQuest : null;
    
    /// <summary>전체 시스템 상태</summary>
    public bool IsSystemBusy => IsRecording || IsProcessing || IsPlayingAudio;
    
    #endregion
    
    #region 이벤트
    
    /// <summary>상태 변경 이벤트</summary>
    public event Action<string> OnStatusChanged;
    
    /// <summary>NPC 텍스트 수신 이벤트</summary>
    public event Action<string> OnNPCTextReceived;
    
    /// <summary>전사 텍스트 수신 이벤트</summary>
    public event Action<string> OnTranscriptionReceived;
    
    /// <summary>퀘스트 완료 이벤트</summary>
    public event Action<string> OnQuestCompleted;
    
    /// <summary>오류 이벤트</summary>
    public event Action<string> OnErrorReceived;
    
    /// <summary>녹음 상태 변경 이벤트</summary>
    public event Action<bool> OnRecordingStateChanged;
    
    /// <summary>처리 상태 변경 이벤트</summary>
    public event Action<bool> OnProcessingStateChanged;
    
    /// <summary>오디오 재생 상태 변경 이벤트</summary>
    public event Action<bool> OnAudioPlaybackStateChanged;
    
    #endregion
    
    #region 메모리 삭제 상태
    
    /// <summary>현재 메모리 삭제 요청 중인지 여부</summary>
    private bool isClearingMemory = false;
    
    #endregion

    #region Unity 라이프사이클
    
    /// <summary>
    /// Unity Start 메서드
    /// 컴포넌트들을 초기화하고 이벤트를 연결합니다
    /// </summary>
    void Start()
    {
        InitializeComponents();
        SetupEventConnections();
        
        NotifyStatusChanged("NPC 대화 시스템 준비 완료");
        Debug.Log("[NPCChatManager] 초기화 완료");
    }
    
    /// <summary>
    /// Unity OnDestroy 메서드
    /// 이벤트 연결을 해제합니다
    /// </summary>
    void OnDestroy()
    {
        DisconnectEventConnections();
    }
    
    #endregion

    #region 초기화
    
    /// <summary>
    /// 컴포넌트들을 초기화합니다
    /// </summary>
    private void InitializeComponents()
    {
        // 자동 컴포넌트 참조 설정
        if (autoSetupComponents)
        {
            SetupComponentReferences();
        }
        
        // 각 컴포넌트 검증
        ValidateComponents();
        
        // 서버 클라이언트 설정
        if (serverClient != null)
        {
            serverClient.SetServerUrl(serverUrl);
        }
    }
    
    /// <summary>
    /// 컴포넌트 참조를 자동으로 설정합니다
    /// </summary>
    private void SetupComponentReferences()
    {
        // AudioRecorder 참조 설정
        if (audioRecorder == null)
        {
            audioRecorder = GetComponent<AudioRecorder>();
            if (audioRecorder == null)
            {
                audioRecorder = gameObject.AddComponent<AudioRecorder>();
            }
        }
        
        // NPCServerClient 참조 설정
        if (serverClient == null)
        {
            serverClient = GetComponent<NPCServerClient>();
            if (serverClient == null)
            {
                serverClient = gameObject.AddComponent<NPCServerClient>();
            }
        }
        
        // StreamingAudioPlayer 참조 설정
        if (audioPlayer == null)
        {
            audioPlayer = GetComponent<StreamingAudioPlayer>();
            if (audioPlayer == null)
            {
                audioPlayer = gameObject.AddComponent<StreamingAudioPlayer>();
            }
        }
        
        // NPCConversationManager 참조 설정
        if (conversationManager == null)
        {
            conversationManager = GetComponent<NPCConversationManager>();
            if (conversationManager == null)
            {
                conversationManager = gameObject.AddComponent<NPCConversationManager>();
            }
        }
        
        Debug.Log("[NPCChatManager] 컴포넌트 참조 자동 설정 완료");
    }
    
    /// <summary>
    /// 컴포넌트들의 유효성을 검증합니다
    /// </summary>
    private void ValidateComponents()
    {
        if (audioRecorder == null)
        {
            Debug.LogError("[NPCChatManager] AudioRecorder 컴포넌트가 없습니다!");
        }
        
        if (serverClient == null)
        {
            Debug.LogError("[NPCChatManager] NPCServerClient 컴포넌트가 없습니다!");
        }
        
        if (audioPlayer == null)
        {
            Debug.LogError("[NPCChatManager] StreamingAudioPlayer 컴포넌트가 없습니다!");
        }
        
        if (conversationManager == null)
        {
            Debug.LogError("[NPCChatManager] NPCConversationManager 컴포넌트가 없습니다!");
        }
    }
    
    #endregion

    #region 이벤트 연결
    
    /// <summary>
    /// 컴포넌트 간 이벤트 연결을 설정합니다
    /// </summary>
    private void SetupEventConnections()
    {
        // AudioRecorder 이벤트 연결
        if (audioRecorder != null)
        {
            audioRecorder.OnRecordingStarted += () => OnRecordingStateChanged?.Invoke(true);
            audioRecorder.OnRecordingCompleted += OnRecordingCompleted;
            audioRecorder.OnRecordingCancelled += () => OnRecordingStateChanged?.Invoke(false);
            audioRecorder.OnRecordingError += (error) => OnErrorReceived?.Invoke(error);
            audioRecorder.OnMicrophoneStatusChanged += OnMicrophoneStatusChanged;
        }
        
        // NPCServerClient 이벤트 연결
        if (serverClient != null)
        {
            serverClient.OnSSEMessageReceived += OnSSEMessageReceived;
            serverClient.OnRequestStarted += (type) => OnProcessingStateChanged?.Invoke(true);
            serverClient.OnRequestCompleted += (type) => OnProcessingStateChanged?.Invoke(false);
            serverClient.OnServerError += OnServerErrorReceived;
        }
        
        // StreamingAudioPlayer 이벤트 연결
        if (audioPlayer != null)
        {
            audioPlayer.OnAudioChunkStarted += OnAudioChunkStarted;
            audioPlayer.OnAudioChunkCompleted += OnAudioChunkCompleted;
            audioPlayer.OnAllAudioCompleted += OnAllAudioCompleted;
            audioPlayer.OnPlaybackStateChanged += OnAudioPlaybackStateChanged;
            audioPlayer.OnAudioError += (error) => OnErrorReceived?.Invoke(error);
        }
        
        // NPCConversationManager 이벤트 연결
        if (conversationManager != null)
        {
            conversationManager.OnMessageAdded += OnMessageAdded;
            conversationManager.OnNPCInfoChanged += OnNPCInfoChanged;
            conversationManager.OnQuestStatusChanged += OnQuestStatusChanged;
        }
        
        Debug.Log("[NPCChatManager] 이벤트 연결 설정 완료");
    }
    
    /// <summary>
    /// 이벤트 연결을 해제합니다
    /// </summary>
    private void DisconnectEventConnections()
    {
        // AudioRecorder 이벤트 해제
        if (audioRecorder != null)
        {
            audioRecorder.OnRecordingStarted -= () => OnRecordingStateChanged?.Invoke(true);
            audioRecorder.OnRecordingCompleted -= OnRecordingCompleted;
            audioRecorder.OnRecordingCancelled -= () => OnRecordingStateChanged?.Invoke(false);
            audioRecorder.OnRecordingError -= (error) => OnErrorReceived?.Invoke(error);
            audioRecorder.OnMicrophoneStatusChanged -= OnMicrophoneStatusChanged;
        }
        
        // NPCServerClient 이벤트 해제
        if (serverClient != null)
        {
            serverClient.OnSSEMessageReceived -= OnSSEMessageReceived;
            serverClient.OnRequestStarted -= (type) => OnProcessingStateChanged?.Invoke(true);
            serverClient.OnRequestCompleted -= (type) => OnProcessingStateChanged?.Invoke(false);
            serverClient.OnServerError -= OnServerErrorReceived;
        }
        
        // StreamingAudioPlayer 이벤트 해제
        if (audioPlayer != null)
        {
            audioPlayer.OnAudioChunkStarted -= OnAudioChunkStarted;
            audioPlayer.OnAudioChunkCompleted -= OnAudioChunkCompleted;
            audioPlayer.OnAllAudioCompleted -= OnAllAudioCompleted;
            audioPlayer.OnPlaybackStateChanged -= OnAudioPlaybackStateChanged;
            audioPlayer.OnAudioError -= (error) => OnErrorReceived?.Invoke(error);
        }
        
        // NPCConversationManager 이벤트 해제
        if (conversationManager != null)
        {
            conversationManager.OnMessageAdded -= OnMessageAdded;
            conversationManager.OnNPCInfoChanged -= OnNPCInfoChanged;
            conversationManager.OnQuestStatusChanged -= OnQuestStatusChanged;
        }
    }
    
    #endregion
    
    #region 공개 API 메서드
    
    /// <summary>
    /// 녹음을 시작합니다
    /// </summary>
    /// <returns>시작 성공 여부</returns>
    public bool StartRecording()
    {
        if (IsSystemBusy)
        {
            NotifyStatusChanged("시스템이 사용 중입니다. 잠시 기다려주세요.");
            return false;
        }
        
        if (audioRecorder == null)
        {
            NotifyStatusChanged("오디오 녹음기가 초기화되지 않았습니다.");
            return false;
        }
        
        // 새로운 녹음 시작 시 기존 오디오 정지
        StopAudioPlayback();
        
        bool success = audioRecorder.StartRecording();
        if (success)
        {
            NotifyStatusChanged("🎤 녹음 중... (중지 버튼을 눌러 완료)");
        }
        
        return success;
    }
    
    /// <summary>
    /// 녹음을 중지합니다
    /// </summary>
    /// <returns>중지 성공 여부</returns>
    public bool StopRecording()
    {
        if (audioRecorder == null)
        {
            NotifyStatusChanged("오디오 녹음기가 초기화되지 않았습니다.");
            return false;
        }
        
        bool success = audioRecorder.StopRecording();
        if (success)
        {
            NotifyStatusChanged("🔄 음성을 처리하고 있습니다...");
        }
        
        return success;
    }
    
    /// <summary>
    /// 녹음을 취소합니다
    /// </summary>
    public void CancelRecording()
    {
        if (audioRecorder != null)
        {
            audioRecorder.CancelRecording();
            NotifyStatusChanged("녹음이 취소되었습니다.");
        }
    }

    /// <summary>
    /// NPC 선제 대화를 시작합니다
    /// </summary>
    /// <param name="initialMessage">초기 메시지</param>
    /// <returns>시작 성공 여부</returns>
    public bool StartInitiateChat(string initialMessage = "When a player approaches an NPC")
    {
        if (IsSystemBusy)
        {
            NotifyStatusChanged("시스템이 사용 중입니다. 잠시 기다려주세요.");
            return false;
        }
        
        if (serverClient == null || conversationManager == null)
        {
            NotifyStatusChanged("필요한 컴포넌트가 초기화되지 않았습니다.");
            return false;
        }
        
        // 새로운 대화 시작 시 기존 오디오 정지
        StopAudioPlayback();
        
        NotifyStatusChanged("🤖 NPC가 말을 걸어오고 있습니다...");
        
        // 서버에 선제 대화 요청
        StartCoroutine(serverClient.SendInitiateChatRequest(
            initialMessage,
            conversationManager.CurrentNPC,
            conversationManager.ConversationHistory,
            conversationManager.CurrentQuest,
            conversationManager.MemoryKey,
            language,
            useThinking
        ));
        
        return true;
    }
    
    /// <summary>
    /// 모든 오디오 재생을 중지합니다
    /// </summary>
    public void StopAudioPlayback()
    {
        if (audioPlayer != null)
        {
            audioPlayer.StopAllAudio();
        }
    }
    
    /// <summary>
    /// 텍스트 메시지를 NPC에게 전송합니다
    /// </summary>
    /// <param name="textMessage">플레이어가 보낸 텍스트 메시지</param>
    /// <returns>전송 성공 여부</returns>
    public bool SendTextToNPC(string textMessage)
    {
        if (IsSystemBusy)
        {
            NotifyStatusChanged("시스템이 사용 중입니다. 잠시 기다려주세요.");
            return false;
        }
        
        if (string.IsNullOrEmpty(textMessage))
        {
            NotifyStatusChanged("전송할 텍스트가 없습니다.");
            return false;
        }
        
        if (serverClient == null || conversationManager == null)
        {
            NotifyStatusChanged("필요한 컴포넌트가 초기화되지 않았습니다.");
            return false;
        }
        
        // 새로운 대화 시작 시 기존 오디오 정지
        StopAudioPlayback();
        
        NotifyStatusChanged("📤 텍스트를 전송하고 있습니다...");
        
        // 대화 히스토리에 플레이어 메시지 추가
        conversationManager.AddMessage(ConversationSenders.PLAYER, textMessage, conversationManager.CurrentQuest?.id);
        
        // 서버에 텍스트 대화 요청
        StartCoroutine(serverClient.SendChatTextRequest(
            textMessage,
            conversationManager.CurrentNPC,
            conversationManager.ConversationHistory,
            conversationManager.CurrentQuest,
            conversationManager.MemoryKey,
            language,
            useThinking
        ));
        
        return true;
    }
    
    /// <summary>
    /// 시스템을 리셋합니다
    /// </summary>
    public void ResetSystem()
    {
        // 모든 활성 작업 중지
        CancelRecording();
        StopAudioPlayback();
        
        if (serverClient != null)
        {
            serverClient.AbortCurrentRequest();
        }
        
        if (audioPlayer != null)
        {
            audioPlayer.ResetAudioSystem();
        }
        
        if (conversationManager != null)
        {
            conversationManager.EndCurrentSession();
            conversationManager.StartNewSession();
        }
        
        NotifyStatusChanged("시스템이 리셋되었습니다.");
        Debug.Log("[NPCChatManager] 시스템 리셋 완료");
    }
    
    #endregion
    
    #region 이벤트 핸들러
    
    /// <summary>
    /// 녹음 완료 이벤트 핸들러
    /// </summary>
    private void OnRecordingCompleted(AudioClip recordedClip)
    {
        OnRecordingStateChanged?.Invoke(false);
        
        if (recordedClip == null)
        {
            NotifyStatusChanged("녹음된 오디오가 없습니다.");
            return;
        }
        
        // 오디오를 WAV로 변환
        byte[] wavData = WavUtility.FromAudioClip(recordedClip);
        
        if (wavData == null || wavData.Length == 0)
        {
            NotifyStatusChanged("오디오 변환에 실패했습니다.");
            return;
        }
        
        // Base64로 인코딩
        string base64Audio = Convert.ToBase64String(wavData);
        
        // 서버에 음성 대화 요청
        if (serverClient != null && conversationManager != null)
        {
            StartCoroutine(serverClient.SendChatAudioRequest(
                base64Audio,
                conversationManager.CurrentNPC,
                conversationManager.ConversationHistory,
                conversationManager.CurrentQuest,
                conversationManager.MemoryKey,
                language,
                useThinking
            ));
        }
        
        // 녹음된 클립 정리
        DestroyImmediate(recordedClip);
    }
    
    /// <summary>
    /// 마이크 상태 변경 이벤트 핸들러
    /// </summary>
    private void OnMicrophoneStatusChanged(bool isAvailable)
    {
        if (isAvailable)
        {
            NotifyStatusChanged("마이크가 준비되었습니다.");
        }
        else
        {
            NotifyStatusChanged("마이크를 사용할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 서버 오류 이벤트 핸들러
    /// </summary>
    private void OnServerErrorReceived(string error)
    {
        // 메모리 삭제 요청 중 오류 발생 시 로컬 정리
        if (isClearingMemory)
        {
            Debug.LogWarning($"[NPCChatManager] 메모리 삭제 요청 중 서버 오류 발생: {error}");
            NotifyStatusChanged("❌ 서버 오류 발생 - 로컬 정리만 수행합니다");
            
            ClearLocalMemory();
            isClearingMemory = false;
        }
        else
        {
            // 일반적인 서버 오류 처리
            OnErrorReceived?.Invoke(error);
        }
    }
    
    /// <summary>
    /// SSE 메시지 수신 이벤트 핸들러
    /// </summary>
    private void OnSSEMessageReceived(string messageType, string jsonData)
    {
        try
        {
            switch (messageType)
            {
                case ResponseTypes.NPC_METADATA:
                    ProcessMetadataResponse(jsonData);
                    break;
                    
                case ResponseTypes.NPC_TEXT:
                    ProcessTextResponse(jsonData);
                    break;
                    
                case ResponseTypes.NPC_AUDIO:
                    ProcessAudioResponse(jsonData);
                    break;
                    
                case ResponseTypes.NPC_COMPLETE:
                    ProcessCompleteResponse(jsonData);
                    break;
                    
                case ResponseTypes.NPC_ERROR:
                    ProcessErrorResponse(jsonData);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[NPCChatManager] SSE 메시지 처리 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 메타데이터 응답 처리
    /// </summary>
    private void ProcessMetadataResponse(string jsonData)
    {
        var response = JsonUtility.FromJson<NPCMetadataResponse>(jsonData);
        
        if (!string.IsNullOrEmpty(response.transcribed_text))
        {
            OnTranscriptionReceived?.Invoke(response.transcribed_text);
            
            // 대화 기록에 플레이어 메시지 추가
            conversationManager?.AddMessage(ConversationSenders.PLAYER, response.transcribed_text, response.quest_id);
        }
    }
    
    /// <summary>
    /// 텍스트 응답 처리
    /// </summary>
    private void ProcessTextResponse(string jsonData)
    {
        var response = JsonUtility.FromJson<NPCTextResponse>(jsonData);
        
        OnNPCTextReceived?.Invoke(response.npc_text);
        
        // 대화 기록에 NPC 메시지 추가
        conversationManager?.AddMessage(ConversationSenders.NPC, response.npc_text, CurrentQuest?.id);
    }
    
    /// <summary>
    /// 오디오 응답 처리
    /// </summary>
    private void ProcessAudioResponse(string jsonData)
    {
        var response = JsonUtility.FromJson<NPCAudioResponse>(jsonData);
        
        // Base64 오디오 디코딩
        byte[] audioBytes = Convert.FromBase64String(response.audio_data);
        
        // AudioClip 생성
        AudioClip audioClip = WavUtility.ToAudioClip(audioBytes, $"NPCAudio_{response.sentence_id}");
        
        if (audioClip != null)
        {
            // 오디오 플레이어에 추가 (NPC 텍스트와 함께)
            audioPlayer?.AddAudioChunk(response.sentence_id, audioClip, response.npc_text);
            
            Debug.Log($"[NPCChatManager] 오디오 청크 처리 완료: 문장 {response.sentence_id}, 텍스트: '{response.npc_text}'");
        }
        else
        {
            Debug.LogError($"[NPCChatManager] 오디오 클립 생성 실패: 문장 {response.sentence_id}");
        }
    }
    
    /// <summary>
    /// 완료 응답 처리
    /// </summary>
    private void ProcessCompleteResponse(string jsonData)
    {
        // 메모리 삭제 완료 응답인지 확인
        if (jsonData.Contains("memory_cleared"))
        {
            ProcessMemoryClearedResponse(jsonData);
            return;
        }
        
        var response = JsonUtility.FromJson<NPCCompleteResponse>(jsonData);
        
        NotifyStatusChanged($"✅ NPC 대화 완료 ({response.total_sentences}개 문장)");
        
        if (response.quest_complete)
        {
            NotifyStatusChanged("🎉 퀘스트 완료!");
            OnQuestCompleted?.Invoke(conversationManager.CurrentQuest.id);
        }
    }
    
    /// <summary>
    /// 메모리 삭제 완료 응답 처리
    /// </summary>
    private void ProcessMemoryClearedResponse(string jsonData)
    {
        try
        {
            var response = JsonUtility.FromJson<MemoryClearedResponse>(jsonData);
            
            if (response.success)
            {
                NotifyStatusChanged($"✅ 서버 메모리 삭제 완료 - 로컬 정리 중...");
                Debug.Log($"[NPCChatManager] 서버 메모리 삭제 성공 - 메모리 키: {response.memory_key}");
                
                // 서버 메모리 삭제 성공 후 로컬 정리
                ClearLocalMemory();
                
                NotifyStatusChanged($"✅ NPC 대화 기록이 완전히 삭제되었습니다");
            }
            else
            {
                NotifyStatusChanged($"❌ 서버 메모리 삭제 실패 - 로컬 정리만 진행합니다");
                Debug.LogError($"[NPCChatManager] 서버 메모리 삭제 실패 - 메모리 키: {response.memory_key}");
                
                // 서버 삭제 실패해도 로컬 정리는 수행
                ClearLocalMemory();
            }
        }
        catch (Exception e)
        {
            NotifyStatusChanged($"❌ 메모리 삭제 응답 처리 중 오류 - 로컬 정리만 진행합니다");
            Debug.LogError($"[NPCChatManager] 메모리 삭제 응답 처리 오류: {e.Message}");
            
            // 응답 처리 실패해도 로컬 정리는 수행
            ClearLocalMemory();
        }
        finally
        {
            // 메모리 삭제 완료 플래그 해제
            isClearingMemory = false;
        }
    }
    
    /// <summary>
    /// 오류 응답 처리
    /// </summary>
    private void ProcessErrorResponse(string jsonData)
    {
        var response = JsonUtility.FromJson<NPCErrorResponse>(jsonData);
        
        NotifyStatusChanged($"❌ 오류: {response.error}");
        OnErrorReceived?.Invoke(response.error);
        
        // 오류 시 시스템 정리
        StopAudioPlayback();
    }
    
    /// <summary>
    /// 오디오 청크 시작 이벤트 핸들러
    /// </summary>
    private void OnAudioChunkStarted(StreamingAudioPlayer.AudioChunk chunk)
    {
        NotifyStatusChanged($"🔊 재생 중: 문장 {chunk.sentenceId}");
    }
    
    /// <summary>
    /// 오디오 청크 완료 이벤트 핸들러
    /// </summary>
    private void OnAudioChunkCompleted(StreamingAudioPlayer.AudioChunk chunk)
    {
        Debug.Log($"[NPCChatManager] 오디오 청크 재생 완료: 문장 {chunk.sentenceId}");
    }
    
    /// <summary>
    /// 모든 오디오 완료 이벤트 핸들러
    /// </summary>
    private void OnAllAudioCompleted(int totalChunks)
    {
        NotifyStatusChanged($"✅ 모든 음성 재생 완료 (총 {totalChunks}개 청크)");
    }
    
    /// <summary>
    /// 메시지 추가 이벤트 핸들러
    /// </summary>
    private void OnMessageAdded(ConversationMessage message)
    {
        Debug.Log($"[NPCChatManager] 대화 메시지 추가: {message.sender} - {message.message}");
    }
    
    /// <summary>
    /// NPC 정보 변경 이벤트 핸들러
    /// </summary>
    private void OnNPCInfoChanged(NPCInfo npcInfo)
    {
        Debug.Log($"[NPCChatManager] NPC 정보 변경: {npcInfo.name}");
    }
    
    /// <summary>
    /// 퀘스트 상태 변경 이벤트 핸들러
    /// </summary>
    private void OnQuestStatusChanged(NPCSystem.QuestInfo questInfo)
    {
        if (questInfo != null)
        {
            Debug.Log($"[NPCChatManager] 퀘스트 상태 변경: {questInfo.name} - {questInfo.status}");
        }
    }
    
    #endregion
    
    #region 유틸리티 메서드
    
    /// <summary>
    /// 상태 변경을 알립니다
    /// </summary>
    private void NotifyStatusChanged(string message)
    {
        OnStatusChanged?.Invoke(message);
        Debug.Log($"[NPCChatManager] {message}");
    }
    
    /// <summary>
    /// 시스템 상태 정보를 가져옵니다
    /// </summary>
    public string GetSystemStatus()
    {
        if (IsRecording)
        {
            return "녹음 중";
        }
        
        if (IsProcessing)
        {
            return "서버 처리 중";
        }
        
        if (IsPlayingAudio)
        {
            return "음성 재생 중";
        }
        
        return "준비됨";
    }
    
    /// <summary>
    /// NPC 정보를 설정합니다
    /// </summary>
    public void SetNPCInfo(NPCInfo npcInfo)
    {
        conversationManager?.SetNPCInfo(npcInfo);
    }
    
    /// <summary>
    /// 퀘스트 정보를 설정합니다
    /// </summary>
    public void SetQuestInfo(NPCSystem.QuestInfo questInfo)
    {
        conversationManager?.SetQuestInfo(questInfo);
    }
    
    /// <summary>
    /// 서버 URL을 설정합니다
    /// </summary>
    public void SetServerUrl(string url)
    {
        serverUrl = url;
        serverClient?.SetServerUrl(url);
    }
    
    /// <summary>
    /// 녹음 시작 시도 (UI 호환성을 위한 래퍼 메서드)
    /// </summary>
    /// <returns>시작 성공 여부</returns>
    public bool TryStartRecording()
    {
        return StartRecording();
    }
    
    /// <summary>
    /// 녹음 중지 시도 (UI 호환성을 위한 래퍼 메서드)
    /// </summary>
    /// <returns>중지 성공 여부</returns>
    public bool TryStopRecording()
    {
        return StopRecording();
    }
    
    /// <summary>
    /// NPC 선제 대화 시작 시도 (UI 호환성을 위한 래퍼 메서드)
    /// </summary>
    /// <param name="initialMessage">초기 메시지</param>
    /// <returns>시작 성공 여부</returns>
    public bool TryStartInitiateChat(string initialMessage = "When a player approaches an NPC")
    {
        return StartInitiateChat(initialMessage);
    }
    
    /// <summary>
    /// NPC 선제 대화 시작 (UI 호환성을 위한 래퍼 메서드)
    /// </summary>
    /// <param name="situation">상황 설명</param>
    /// <returns>시작 성공 여부</returns>
    public bool StartNPCInitiateChat(string situation)
    {
        return StartInitiateChat(situation);
    }
    
    /// <summary>
    /// 오디오 버퍼 상태를 가져옵니다
    /// </summary>
    /// <returns>오디오 버퍼 상태 문자열</returns>
    public string GetAudioBufferStatus()
    {
        if (audioPlayer == null)
        {
            return "오디오 플레이어가 없습니다.";
        }
        
        return audioPlayer.GetAudioSystemStatus();
    }
    
    /// <summary>
    /// 오디오 시스템을 리셋합니다 (UI 호환성을 위한 래퍼 메서드)
    /// </summary>
    public void ResetAudioSystem()
    {
        ResetSystem();
    }
    
    /// <summary>
    /// NPC 대화 기록을 삭제합니다
    /// </summary>
    /// <param name="reason">삭제 사유</param>
    public void ClearNPCMemory(string reason = "메모리 삭제 요청")
    {
        if (IsSystemBusy)
        {
            Debug.LogWarning("[NPCChatManager] 시스템이 바쁜 상태에서는 메모리를 삭제할 수 없습니다.");
            OnErrorReceived?.Invoke("시스템이 바쁜 상태에서는 메모리를 삭제할 수 없습니다.");
            return;
        }
        
        NotifyStatusChanged("🗑️ NPC 메모리 삭제 중...");
        
        // 메모리 키 생성 (기기 고유 ID)
        string memoryKey = SystemInfo.deviceUniqueIdentifier;
        
        // 서버에 메모리 삭제 요청
        if (serverClient != null)
        {
            StartCoroutine(ClearMemorySequence(memoryKey, reason));
        }
        else
        {
            Debug.LogError("[NPCChatManager] 서버 클라이언트가 없습니다.");
            OnErrorReceived?.Invoke("서버 클라이언트가 없습니다.");
            
            // 서버 클라이언트가 없어도 로컬 정리는 수행
            ClearLocalMemory();
        }
        
        Debug.Log($"[NPCChatManager] NPC 메모리 삭제 요청 - 사유: {reason}");
    }
    
    /// <summary>
    /// 메모리 삭제 순서 제어 코루틴
    /// </summary>
    /// <param name="memoryKey">메모리 키</param>
    /// <param name="reason">삭제 사유</param>
    /// <returns>코루틴</returns>
    private IEnumerator ClearMemorySequence(string memoryKey, string reason)
    {
        isClearingMemory = true;
        
        try
        {
            // 서버에 메모리 삭제 요청
            // 응답은 ProcessMemoryClearedResponse에서 처리됨
            yield return StartCoroutine(serverClient.SendClearMemoryRequest(memoryKey, reason));
            
            Debug.Log("[NPCChatManager] 메모리 삭제 요청 완료");
        }
        finally
        {
            // 타임아웃 등으로 응답을 받지 못한 경우 로컬 정리
            if (isClearingMemory)
            {
                Debug.LogWarning("[NPCChatManager] 서버 응답을 받지 못함 - 로컬 정리만 수행");
                NotifyStatusChanged("⚠️ 서버 응답 없음 - 로컬 정리만 수행합니다");
                
                // 로컬 정리 (서버 연결 건드리지 않음)
                ClearLocalMemory();
                isClearingMemory = false;
                
                NotifyStatusChanged("✅ 로컬 메모리 정리 완료");
            }
        }
    }
    
    /// <summary>
    /// 로컬 메모리 정리
    /// </summary>
    private void ClearLocalMemory()
    {
        // 로컬 대화 히스토리 삭제
        if (conversationManager != null)
        {
            conversationManager.ClearConversationHistory();
            Debug.Log("[NPCChatManager] 로컬 대화 히스토리 삭제 완료");
        }
        
        // 오디오 시스템만 리셋 (서버 연결은 건드리지 않음)
        ResetAudioSystemOnly();
        
        Debug.Log("[NPCChatManager] 로컬 메모리 정리 완료");
    }
    
    /// <summary>
    /// 오디오 시스템만 리셋합니다 (서버 연결은 유지)
    /// </summary>
    private void ResetAudioSystemOnly()
    {
        // 오디오 녹음 중단
        if (audioRecorder != null && audioRecorder.IsRecording)
        {
            audioRecorder.CancelRecording();
            Debug.Log("[NPCChatManager] 녹음 중단됨");
        }
        
        // 오디오 재생 중단
        if (audioPlayer != null)
        {
            audioPlayer.StopAllAudio();
            audioPlayer.ResetAudioSystem();
            Debug.Log("[NPCChatManager] 오디오 재생 중단됨");
        }
        
        // 상태 알림 (서버 연결 상태는 건드리지 않음)
        NotifyStatusChanged("오디오 시스템 정리 완료");
        
        Debug.Log("[NPCChatManager] 오디오 시스템 정리 완료");
    }
    
    #endregion
} 