using UnityEngine;
using UnityEngine.UI;
using NPCSystem;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// NPC 대화 시스템 UI 예제
/// 
/// NPCChatManager와 연동하여 간단한 대화 인터페이스를 제공합니다.
/// 실제 게임에서는 이 스크립트를 참고하여 게임에 맞는 UI를 구성하세요.
/// </summary>
public class NPCChatUI : MonoBehaviour
{
    [Header("=== UI 패널들 ===")]
    [SerializeField] private GameObject mainPanel;
    
    [Header("=== 메인 UI 요소들 ===")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Button stopRecordButton;
    [SerializeField] private Button initiateButton;
    [SerializeField] private Button stopAudioButton; // 오디오 재생 중지 버튼 추가
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text npcDialogueText;
    [SerializeField] private TMP_Text transcribedText;
    
    [Header("=== 텍스트 입력 UI ===")]
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private Button sendTextButton;
    [SerializeField] private Button clearTextButton;
    [SerializeField] private Button clearMemoryButton;
    

    
    [Header("=== NPC 대화 매니저 ===")]
    [SerializeField] private NPCChatManager npcChatManager;

    void Start()
    {
        InitializeUI();
        SetupEventListeners();
        SetQuest("1", "Ordering food on an airplane", "You have to pick your meal. Choose between a burger and a pizza.", "Choose between a hamburger and a pizza.");
    }

    public void OnClickSetQuest()
    {
        SetQuest("1", "Ordering food on an airplane", "You have to pick your meal. Choose between a burger and a pizza.", "Choose between a hamburger and a pizza.");
    }

    private void InitializeUI()
    {
        // NPC 대화 매니저 찾기
        if (npcChatManager == null)
        {
            npcChatManager = FindFirstObjectByType<NPCChatManager>();
            if (npcChatManager == null)
            {
                Debug.LogError("NPCChatManager를 찾을 수 없습니다!");
                return;
            }
        }
        
        // 초기 패널 상태 설정
        ShowMainPanel();
        
        // 초기 버튼 상태 설정
        InitializeButtonStates();
        
        Debug.Log("NPCChatUI 초기화 완료");
    }

    private void SetupEventListeners()
    {
        // 메인 UI 버튼들
        if (recordButton != null)
            recordButton.onClick.AddListener(OnRecordButtonClicked);
        
        if (stopRecordButton != null)
            stopRecordButton.onClick.AddListener(OnStopRecordButtonClicked);
        
        if (initiateButton != null)
            initiateButton.onClick.AddListener(OnInitiateButtonClicked);
        
        if (stopAudioButton != null)
            stopAudioButton.onClick.AddListener(OnStopAudioButtonClicked);
        
        // 텍스트 입력 UI 버튼들
        if (sendTextButton != null)
            sendTextButton.onClick.AddListener(OnSendTextButtonClicked);
        
        if (clearTextButton != null)
            clearTextButton.onClick.AddListener(OnClearTextButtonClicked);
        
        if (clearMemoryButton != null)
            clearMemoryButton.onClick.AddListener(OnClearMemoryButtonClicked);
        
        // 입력 필드 엔터 키 처리
        if (textInputField != null)
        {
            textInputField.onSubmit.AddListener(OnTextInputSubmit);
        }
        
        // NPC 대화 매니저 이벤트 구독
        if (npcChatManager != null)
        {
            npcChatManager.OnStatusChanged += OnStatusChanged;
            npcChatManager.OnNPCTextReceived += OnNPCTextReceived;
            npcChatManager.OnTranscriptionReceived += OnTranscriptionReceived;
            npcChatManager.OnErrorReceived += OnErrorReceived;
            npcChatManager.OnRecordingStateChanged += OnRecordingStateChanged;
            npcChatManager.OnProcessingStateChanged += OnProcessingStateChanged;
            npcChatManager.OnAudioPlaybackStateChanged += OnAudioPlaybackStateChanged;
        }
    }



    #region UI 이벤트 핸들러들

    private void OnRecordButtonClicked()
    {
        if (npcChatManager != null)
        {
            npcChatManager.TryStartRecording();
        }
    }

    private void OnStopRecordButtonClicked()
    {
        if (npcChatManager != null)
        {
            npcChatManager.TryStopRecording();
        }
    }

    private void OnInitiateButtonClicked()
    {
        if (npcChatManager != null)
        {
            npcChatManager.TryStartInitiateChat();
        }
    }

    private void OnStopAudioButtonClicked()
    {
        if (npcChatManager != null)
        {
            npcChatManager.StopAudioPlayback();
            UpdateStatusText("오디오 재생이 중지되었습니다.");
        }
    }

    private void OnSendTextButtonClicked()
    {
        SendTextToNPC();
    }

    private void OnClearTextButtonClicked()
    {
        if (textInputField != null)
        {
            textInputField.text = "";
            textInputField.ActivateInputField();
        }
    }

    private void OnTextInputSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text.Trim()))
        {
            SendTextToNPC();
        }
    }

    private void OnClearMemoryButtonClicked()
    {
        ClearNPCMemory();
    }

    #endregion

    #region 텍스트 전송 기능

    /// <summary>
    /// 텍스트 입력 필드의 내용을 NPC에게 전송합니다
    /// </summary>
    private void SendTextToNPC()
    {
        if (textInputField == null || npcChatManager == null)
        {
            UpdateStatusText("❌ 텍스트 입력 필드 또는 NPC 매니저가 없습니다.");
            return;
        }

        string inputText = textInputField.text.Trim();
        
        if (string.IsNullOrEmpty(inputText))
        {
            UpdateStatusText("❌ 텍스트를 입력해주세요.");
            return;
        }

        if (inputText.Length > 1000)
        {
            UpdateStatusText("❌ 텍스트가 너무 깁니다. (최대 1000자)");
            return;
        }

        // 전송 중 상태 표시
        UpdateStatusText("📤 텍스트를 전송 중...");
        
        // 텍스트 전송 (NPCChatManager에 새로운 메서드 필요)
        try
        {
            npcChatManager.SendTextToNPC(inputText);
        }
        catch (System.Exception ex)
        {
            UpdateStatusText("❌ 텍스트 전송 기능이 아직 구현되지 않았습니다.");
            Debug.LogWarning($"NPCChatManager에 SendTextToNPC 메서드가 없습니다: {ex.Message}");
        }
        
        // 플레이어 메시지 표시
        if (transcribedText != null)
        {
            transcribedText.text = $"플레이어: {inputText}";
        }
        
        // 입력 필드 초기화
        textInputField.text = "";
        textInputField.ActivateInputField();
    }

    /// <summary>
    /// NPC 대화 기록을 삭제합니다
    /// </summary>
    private void ClearNPCMemory()
    {
        if (npcChatManager == null)
        {
            UpdateStatusText("❌ NPC 매니저가 없습니다.");
            return;
        }

        // 메모리 삭제 확인 다이얼로그
        #if UNITY_EDITOR
        if (Application.isEditor)
        {
            if (!EditorUtility.DisplayDialog("메모리 삭제 확인", 
                "정말로 NPC 대화 기록을 모두 삭제하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.", 
                "삭제", "취소"))
            {
                return;
            }
        }
        #endif

        // 메모리 삭제 중 상태 표시
        UpdateStatusText("🗑️ NPC 대화 기록 삭제 중...");
        
        try
        {
            // NPCChatManager의 메모리 삭제 메서드 호출
            npcChatManager.ClearNPCMemory("사용자 요청으로 인한 메모리 삭제");
            
            // 화면 초기화
            if (npcDialogueText != null)
            {
                npcDialogueText.text = "";
            }
            
            if (transcribedText != null)
            {
                transcribedText.text = "";
            }
            
            UpdateStatusText("✅ NPC 대화 기록이 삭제되었습니다.");
        }
        catch (System.Exception ex)
        {
            UpdateStatusText("❌ 메모리 삭제 중 오류가 발생했습니다.");
            Debug.LogError($"메모리 삭제 오류: {ex.Message}");
        }
    }

    #endregion

    #region NPC 대화 매니저 이벤트 핸들러들

    private void OnStatusChanged(string status)
    {
        UpdateStatusText(status);
    }

    private void OnNPCTextReceived(string npcText)
    {
        if (npcDialogueText != null)
        {
            npcDialogueText.text = npcText;
        }
    }

    private void OnTranscriptionReceived(string transcribedText)
    {
        if (this.transcribedText != null)
        {
            this.transcribedText.text = $"플레이어: {transcribedText}";
        }
    }

    private void OnQuestCompleted(bool completed)
    {
        if (completed)
        {
            UpdateStatusText("🎉 퀘스트 완료!");
            ShowQuestCompleteEffect();
        }
    }

    private void OnErrorReceived(string error)
    {
        UpdateStatusText($"❌ 오류: {error}");
        Debug.LogError($"NPC 대화 오류: {error}");
    }

    private void OnRecordingStateChanged(bool isRecording)
    {
        // 녹음 상태에 따른 버튼 상태 업데이트
        if (recordButton != null)
        {
            recordButton.interactable = !isRecording;
            recordButton.GetComponentInChildren<TMP_Text>().text = isRecording ? "녹음 중" : "음성 대화";
        }
        
        if (stopRecordButton != null)
        {
            stopRecordButton.interactable = isRecording;
            stopRecordButton.GetComponentInChildren<TMP_Text>().text = isRecording ? "녹음 종료" : "녹음 중지";
        }
    }

    private void OnProcessingStateChanged(bool isProcessing)
    {
        // 처리 중일 때 모든 버튼 비활성화 (녹음 관련 제외)
        if (initiateButton != null)
        {
            initiateButton.interactable = !isProcessing;
        }
    }

    private void OnAudioPlaybackStateChanged(bool isPlayingAudio)
    {
        // 오디오 재생 상태에 따른 버튼 상태 업데이트
        if (stopAudioButton != null)
        {
            stopAudioButton.interactable = isPlayingAudio;
            
            // 텍스트 컴포넌트 찾기 (TMP_Text 또는 Text)
            var tmpText = stopAudioButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = isPlayingAudio ? "오디오 중지" : "오디오 재생 중지";
            }
            else
            {
                var text = stopAudioButton.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = isPlayingAudio ? "오디오 중지" : "오디오 재생 중지";
                }
            }
        }
        
        // 상태 텍스트 업데이트 (개선된 정보 제공)
        if (isPlayingAudio)
        {
            if (npcChatManager != null)
            {
                string bufferStatus = npcChatManager.GetAudioBufferStatus();
                UpdateStatusText($"🔊 NPC 음성 재생 중... ({bufferStatus})");
            }
            else
            {
                UpdateStatusText("🔊 NPC 음성 재생 중...");
            }
        }
    }


    #endregion

    #region UI 패널 관리

    private void ShowMainPanel()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    private void InitializeButtonStates()
    {
        // 초기 상태: 녹음 중이 아니므로 녹음 시작 버튼 활성화, 종료 버튼 비활성화
        if (recordButton != null)
        {
            recordButton.interactable = true;
            recordButton.GetComponentInChildren<TMP_Text>().text = "음성 대화";
        }
        
        if (stopRecordButton != null)
        {
            stopRecordButton.interactable = false;
            stopRecordButton.GetComponentInChildren<TMP_Text>().text = "녹음 중지";
        }
        
        // 오디오 재생 중지 버튼 초기 상태
        if (stopAudioButton != null)
        {
            stopAudioButton.interactable = false;
            
            // 텍스트 컴포넌트 찾기 (TMP_Text 또는 Text)
            var tmpText = stopAudioButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = "오디오 재생 중지";
            }
            else
            {
                var text = stopAudioButton.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = "오디오 재생 중지";
                }
            }
        }
        
        // 텍스트 입력 관련 버튼 초기 상태
        if (sendTextButton != null)
        {
            sendTextButton.interactable = true;
            var tmpText = sendTextButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = "텍스트 전송";
            }
        }
        
        if (clearTextButton != null)
        {
            clearTextButton.interactable = true;
            var tmpText = clearTextButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = "입력 초기화";
            }
        }
        
        // 텍스트 입력 필드 초기화
        if (textInputField != null)
        {
            textInputField.text = "";
            textInputField.placeholder.GetComponent<TMP_Text>().text = "NPC에게 전송할 메시지를 입력하세요...";
        }
        
        // 메모리 삭제 버튼 초기 상태
        if (clearMemoryButton != null)
        {
            clearMemoryButton.interactable = true;
            var tmpText = clearMemoryButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = "대화 기록 삭제";
            }
        }
    }



    #endregion



    #region 유틸리티 메소드들

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }



    private void ShowQuestCompleteEffect()
    {
        // 퀘스트 완료 효과 (예: 파티클, 사운드 등)
        Debug.Log("🎉 퀘스트 완료 효과 재생!");
        
        // 여기에 파티클 시스템이나 애니메이션 등을 추가할 수 있습니다.
        // 예: Instantiate(questCompleteEffectPrefab, transform.position, Quaternion.identity);
    }

    /// <summary>
    /// 특정 상황에서 NPC가 말을 걸어오도록 트리거
    /// </summary>
    /// <param name="situation">상황 설명</param>
    public void TriggerNPCInitiate(string situation)
    {
        if (npcChatManager != null)
        {
            npcChatManager.StartNPCInitiateChat(situation);
        }
    }

    /// <summary>
    /// 퀘스트 정보 설정
    /// </summary>
    /// <param name="questId">퀘스트 ID</param>
    /// <param name="questName">퀘스트 이름</param>
    /// <param name="description">퀘스트 설명</param>
    /// <param name="completionCondition">완료 조건</param>
    public void SetQuest(string questId, string questName, string description, string completionCondition)
    {
        if (npcChatManager != null)
        {
            NPCSystem.QuestInfo quest = new NPCSystem.QuestInfo
            {
                id = questId,
                name = questName,
                description = description,
                completion_condition = completionCondition,
                status = NPCSystem.QuestStatus.ACTIVE
            };
            
            npcChatManager.SetQuestInfo(quest);
            UpdateStatusText($"퀘스트 설정: {questName}");
        }
    }

    /// <summary>
    /// 오디오 시스템 상태 리셋 (디버깅용)
    /// </summary>
    public void ResetAudioSystem()
    {
        if (npcChatManager != null)
        {
            npcChatManager.ResetAudioSystem();
            UpdateStatusText("오디오 시스템이 리셋되었습니다.");
        }
    }

    /// <summary>
    /// 오디오 버퍼 상태 확인 (디버깅용)
    /// </summary>
    public void CheckAudioBufferStatus()
    {
        if (npcChatManager != null)
        {
            string status = npcChatManager.GetAudioBufferStatus();
            UpdateStatusText($"버퍼 상태: {status}");
            Debug.Log($"오디오 버퍼 상태: {status}");
        }
    }



    #endregion

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (npcChatManager != null)
        {
            npcChatManager.OnStatusChanged -= OnStatusChanged;
            npcChatManager.OnNPCTextReceived -= OnNPCTextReceived;
            npcChatManager.OnTranscriptionReceived -= OnTranscriptionReceived;
            npcChatManager.OnErrorReceived -= OnErrorReceived;
            npcChatManager.OnRecordingStateChanged -= OnRecordingStateChanged;
            npcChatManager.OnProcessingStateChanged -= OnProcessingStateChanged;
            npcChatManager.OnAudioPlaybackStateChanged -= OnAudioPlaybackStateChanged;
        }
        
        // 버튼 이벤트 리스너 해제
        if (recordButton != null)
            recordButton.onClick.RemoveListener(OnRecordButtonClicked);
        if (stopRecordButton != null)
            stopRecordButton.onClick.RemoveListener(OnStopRecordButtonClicked);
        if (initiateButton != null)
            initiateButton.onClick.RemoveListener(OnInitiateButtonClicked);
        if (stopAudioButton != null)
            stopAudioButton.onClick.RemoveListener(OnStopAudioButtonClicked);
        if (sendTextButton != null)
            sendTextButton.onClick.RemoveListener(OnSendTextButtonClicked);
        if (clearTextButton != null)
            clearTextButton.onClick.RemoveListener(OnClearTextButtonClicked);
        if (clearMemoryButton != null)
            clearMemoryButton.onClick.RemoveListener(OnClearMemoryButtonClicked);
        if (textInputField != null)
            textInputField.onSubmit.RemoveListener(OnTextInputSubmit);
    }
} 