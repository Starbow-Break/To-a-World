# NPC 대화 시스템 (NPCChatManager) 가이드

## 📋 목차
1. [개요](#개요)
2. [시스템 구조](#시스템-구조)
3. [초기 세팅](#초기-세팅)
4. [기본 사용법](#기본-사용법)
5. [음성 녹음 관리](#음성-녹음-관리)
6. [텍스트 대화 기능](#텍스트-대화-기능)
7. [서버 통신](#서버-통신)
8. [오디오 재생](#오디오-재생)
9. [퀘스트 시스템](#퀘스트-시스템)
10. [대화 기록 관리](#대화-기록-관리)
11. [외부 API 활용](#외부-api-활용)
12. [트러블슈팅](#트러블슈팅)
13. [API 레퍼런스](#api-레퍼런스)

---

## 개요

NPCChatManager는 Unity에서 NPC와의 실시간 음성 대화를 가능하게 하는 종합적인 시스템입니다. 음성 녹음부터 서버 통신, 텍스트 음성 변환, 퀘스트 관리까지 모든 기능을 통합 관리합니다.

### 주요 기능
- 🎙️ **음성 녹음**: 마이크를 통한 실시간 음성 녹음
- 📝 **텍스트 대화**: 텍스트 입력을 통한 NPC와의 대화
- 🌐 **서버 통신**: SSE 스트리밍을 통한 실시간 서버 통신
- 🔊 **오디오 재생**: 순차적 음성 재생 시스템
- 📚 **대화 관리**: 대화 히스토리 및 상태 관리
- 🗑️ **메모리 관리**: 대화 기록 삭제 및 관리 기능
- 🎯 **퀘스트 시스템**: 퀘스트 설정 및 완료 판단
- 🎮 **외부 API**: 다른 시스템에서 쉽게 활용 가능

---

## 시스템 구조

### 컴포넌트 구성
```
NPCChatManager (메인 매니저)
├── AudioRecorder (음성 녹음)
├── NPCServerClient (서버 통신)
├── StreamingAudioPlayer (오디오 재생)
├── NPCConversationManager (대화 관리)
└── WavUtility (오디오 변환 유틸리티)
```

### 데이터 모델
- **NPCInfo**: NPC 정보 (이름, 성격, 배경, 음성 스타일 등)
- **QuestInfo**: 퀘스트 정보 (ID, 이름, 설명, 완료 조건 등)
- **ConversationMessage**: 대화 메시지 정보

---

## 초기 세팅

### 1. GameObject 생성 및 컴포넌트 추가

```csharp
// GameObject 생성
GameObject npcChatObject = new GameObject("NPCChatManager");

// NPCChatManager 컴포넌트 추가
NPCChatManager chatManager = npcChatObject.AddComponent<NPCChatManager>();
```

### 2. 인스펙터에서 기본 설정

```csharp
[Header("=== 기본 설정 ===")]
public string serverUrl = "http://localhost:8000";  // 서버 URL
public string language = "en";                       // 언어 설정
public bool useThinking = false;                     // AI 사고 과정 사용 여부
public bool autoSetupComponents = true;              // 자동 컴포넌트 설정
```

### 3. 오디오 소스 설정

```csharp
// 오디오 소스가 자동으로 생성되지만, 필요시 수동 설정
AudioSource audioSource = GetComponent<AudioSource>();
if (audioSource == null)
{
    audioSource = gameObject.AddComponent<AudioSource>();
}
```

### 4. 마이크 권한 설정

```csharp
// 마이크 권한 요청 (필요한 경우)
#if UNITY_ANDROID || UNITY_IOS
    if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
    {
        Application.RequestUserAuthorization(UserAuthorization.Microphone);
    }
#endif
```

---

## 기본 사용법

### 1. 기본 초기화

```csharp
using UnityEngine;
using NPCSystem;

public class NPCChatExample : MonoBehaviour
{
    public NPCChatManager chatManager;
    
    void Start()
    {
        // 이벤트 구독
        chatManager.OnStatusChanged += OnStatusChanged;
        chatManager.OnNPCTextReceived += OnNPCTextReceived;
        chatManager.OnTranscriptionReceived += OnTranscriptionReceived;
        chatManager.OnQuestCompleted += OnQuestCompleted;
        chatManager.OnErrorReceived += OnErrorReceived;
        
        // 기본 NPC 설정
        SetupDefaultNPC();
    }
    
    void SetupDefaultNPC()
    {
        NPCInfo npcInfo = new NPCInfo
        {
            name = "Kay",
            gender = NPCGender.FEMALE,
            personality = "친근하고 도움이 되는 비행기 승무원",
            background = "항공사 객실 승무원으로 10년 경력",
            age = 28,
            voice_style = "natural"
        };
        
        chatManager.SetNPCInfo(npcInfo);
    }
}
```

### 2. 간단한 대화 시작

```csharp
public class SimpleChatController : MonoBehaviour
{
    public NPCChatManager chatManager;
    public Button recordButton;
    public Button stopButton;
    
    void Start()
    {
        recordButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
    }
    
    void StartRecording()
    {
        if (chatManager.TryStartRecording())
        {
            Debug.Log("녹음 시작");
        }
    }
    
    void StopRecording()
    {
        if (chatManager.TryStopRecording())
        {
            Debug.Log("녹음 중지 및 서버 전송");
        }
    }
}
```

---

## 음성 녹음 관리

### 1. 녹음 시작

```csharp
// 기본 녹음 시작
bool success = chatManager.StartRecording();

// 안전한 녹음 시작 (상태 체크 포함)
bool success = chatManager.TryStartRecording();
```

### 2. 녹음 중지

```csharp
// 녹음 중지 (자동으로 서버에 전송)
bool success = chatManager.StopRecording();

// 안전한 녹음 중지
bool success = chatManager.TryStopRecording();
```

### 3. 녹음 취소

```csharp
// 녹음 취소 (서버에 전송하지 않음)
chatManager.CancelRecording();
```

### 4. 녹음 상태 확인

```csharp
public class RecordingStatusUI : MonoBehaviour
{
    public Text statusText;
    public NPCChatManager chatManager;
    
    void Update()
    {
        if (chatManager.IsRecording)
        {
            statusText.text = "🎙️ 녹음 중...";
        }
        else if (chatManager.IsProcessing)
        {
            statusText.text = "🤖 처리 중...";
        }
        else if (chatManager.IsPlayingAudio)
        {
            statusText.text = "🔊 재생 중...";
        }
        else
        {
            statusText.text = "✅ 준비됨";
        }
    }
}
```

---

## 텍스트 대화 기능

음성 녹음 외에도 텍스트 입력을 통해 NPC와 대화할 수 있습니다. 텍스트를 입력하면 서버에서 NPC 응답을 생성하고 음성으로 변환하여 재생합니다.

### 1. 기본 텍스트 대화

```csharp
// 텍스트 메시지를 NPC에게 전송
string userMessage = "안녕하세요, 오늘 날씨가 좋네요.";
bool success = chatManager.SendTextToNPC(userMessage);

if (success)
{
    Debug.Log("텍스트 메시지 전송 성공");
}
else
{
    Debug.Log("텍스트 메시지 전송 실패");
}
```

### 2. UI와 연동한 텍스트 대화

```csharp
public class TextChatController : MonoBehaviour
{
    [Header("UI 참조")]
    public TMP_InputField textInputField;
    public Button sendButton;
    public Button clearButton;
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    void Start()
    {
        SetupUI();
    }
    
    void SetupUI()
    {
        sendButton.onClick.AddListener(SendTextMessage);
        clearButton.onClick.AddListener(ClearTextInput);
        
        // 엔터 키로도 전송 가능
        textInputField.onSubmit.AddListener(OnTextInputSubmit);
    }
    
    void SendTextMessage()
    {
        string inputText = textInputField.text.Trim();
        
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("입력된 텍스트가 없습니다.");
            return;
        }
        
        if (inputText.Length > 1000)
        {
            Debug.LogWarning("텍스트가 너무 깁니다. (최대 1000자)");
            return;
        }
        
        // 텍스트 전송
        bool success = chatManager.SendTextToNPC(inputText);
        
        if (success)
        {
            // 입력 필드 초기화
            textInputField.text = "";
            textInputField.ActivateInputField();
        }
    }
    
    void ClearTextInput()
    {
        textInputField.text = "";
        textInputField.ActivateInputField();
    }
    
    void OnTextInputSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text.Trim()))
        {
            SendTextMessage();
        }
    }
}
```

### 3. 음성과 텍스트 혼합 대화

```csharp
public class HybridChatController : MonoBehaviour
{
    public NPCChatManager chatManager;
    public Button voiceButton;
    public Button textButton;
    public TMP_InputField textInput;
    
    void Start()
    {
        voiceButton.onClick.AddListener(ToggleVoiceChat);
        textButton.onClick.AddListener(SendTextChat);
    }
    
    void ToggleVoiceChat()
    {
        if (chatManager.IsRecording)
        {
            chatManager.TryStopRecording();
        }
        else
        {
            chatManager.TryStartRecording();
        }
    }
    
    void SendTextChat()
    {
        string text = textInput.text.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            chatManager.SendTextToNPC(text);
            textInput.text = "";
        }
    }
}
```

### 4. 텍스트 대화 이벤트 처리

```csharp
public class TextChatEventHandler : MonoBehaviour
{
    public NPCChatManager chatManager;
    public Text playerMessageDisplay;
    public Text npcMessageDisplay;
    
    void Start()
    {
        // 기존 이벤트들과 동일하게 처리
        chatManager.OnNPCTextReceived += OnNPCTextReceived;
        chatManager.OnStatusChanged += OnStatusChanged;
        chatManager.OnErrorReceived += OnErrorReceived;
    }
    
    void OnNPCTextReceived(string npcText)
    {
        npcMessageDisplay.text = $"NPC: {npcText}";
        Debug.Log($"NPC 응답: {npcText}");
    }
    
    void OnStatusChanged(string status)
    {
        if (status.Contains("텍스트"))
        {
            Debug.Log($"텍스트 대화 상태: {status}");
        }
    }
    
    void OnErrorReceived(string error)
    {
        Debug.LogError($"텍스트 대화 오류: {error}");
    }
}
```

### 5. 텍스트 대화 고급 기능

```csharp
public class AdvancedTextChat : MonoBehaviour
{
    public NPCChatManager chatManager;
    
    // 미리 정의된 메시지로 빠른 대화
    void SendQuickMessage(string messageType)
    {
        string message = "";
        
        switch (messageType)
        {
            case "greeting":
                message = "안녕하세요!";
                break;
            case "question":
                message = "도움이 필요합니다.";
                break;
            case "thanks":
                message = "감사합니다!";
                break;
            case "goodbye":
                message = "안녕히 계세요.";
                break;
        }
        
        if (!string.IsNullOrEmpty(message))
        {
            chatManager.SendTextToNPC(message);
        }
    }
    
    // 텍스트 입력 검증
    bool ValidateTextInput(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("텍스트를 입력해주세요.");
            return false;
        }
        
        if (text.Length > 1000)
        {
            Debug.LogWarning("텍스트가 너무 깁니다. (최대 1000자)");
            return false;
        }
        
        if (chatManager.IsSystemBusy)
        {
            Debug.LogWarning("시스템이 바쁩니다. 잠시 후 다시 시도해주세요.");
            return false;
        }
        
        return true;
    }
}
```

---

## 서버 통신

### 1. 서버 URL 설정

```csharp
// 서버 URL 설정
chatManager.SetServerUrl("http://your-server.com:8000");
```

### 2. 음성 대화 요청

```csharp
// 음성 녹음 후 자동으로 서버에 전송되지만, 수동으로도 가능
// (일반적으로 StopRecording() 시 자동 처리됨)
```

### 3. 텍스트 대화 요청

```csharp
// 텍스트 메시지 전송
string message = "안녕하세요, NPC님!";
bool success = chatManager.SendTextToNPC(message);

if (success)
{
    Debug.Log("텍스트 메시지 전송 성공");
}
```

### 4. 대화 기록 삭제 요청

```csharp
// 모든 대화 기록 삭제
chatManager.ClearNPCMemory();

// 사유와 함께 대화 기록 삭제
chatManager.ClearNPCMemory("사용자가 요청한 메모리 초기화");
```

### 5. NPC 선제 대화 시작

```csharp
// NPC가 먼저 대화를 시작하는 경우
bool success = chatManager.StartInitiateChat("플레이어가 NPC에게 접근했을 때");

// 상황별 선제 대화
chatManager.StartNPCInitiateChat("플레이어가 상점에 들어왔을 때");
```

### 6. 서버 응답 처리

```csharp
void OnStatusChanged(string status)
{
    Debug.Log($"시스템 상태: {status}");
}

void OnNPCTextReceived(string npcText)
{
    Debug.Log($"NPC 메시지: {npcText}");
    // UI에 텍스트 표시
}

void OnTranscriptionReceived(string transcription)
{
    Debug.Log($"음성 인식 결과: {transcription}");
    // 플레이어 말 내용 표시
}

void OnErrorReceived(string error)
{
    Debug.LogError($"오류 발생: {error}");
    // 오류 처리 로직
}
```

---

## 오디오 재생

### 1. 순차 재생 시스템

```csharp
// 서버에서 받은 오디오는 자동으로 순차 재생됩니다
// 수동으로 재생 제어 가능

// 오디오 재생 중지
chatManager.StopAudioPlayback();

// 오디오 시스템 초기화
chatManager.ResetAudioSystem();
```

### 2. 오디오 재생 이벤트

```csharp
public class AudioPlaybackController : MonoBehaviour
{
    public NPCChatManager chatManager;
    
    void Start()
    {
        chatManager.OnAudioPlaybackStateChanged += OnAudioPlaybackChanged;
    }
    
    void OnAudioPlaybackChanged(bool isPlaying)
    {
        if (isPlaying)
        {
            Debug.Log("오디오 재생 시작");
        }
        else
        {
            Debug.Log("오디오 재생 완료");
        }
    }
}
```

### 3. 오디오 버퍼 상태 확인

```csharp
// 오디오 버퍼 상태 확인
string bufferStatus = chatManager.GetAudioBufferStatus();
Debug.Log($"오디오 버퍼 상태: {bufferStatus}");
```

---

## 퀘스트 시스템

### 1. 퀘스트 설정

```csharp
// 새 퀘스트 생성
QuestInfo quest = new QuestInfo
{
    id = "quest_001",
    name = "첫 번째 미션",
    description = "NPC와 대화하여 정보를 얻으세요",
    completion_condition = "특정 키워드 언급",
    reward = "경험치 100",
    difficulty = "easy",
    status = QuestStatus.ACTIVE
};

// 퀘스트 설정
chatManager.SetQuestInfo(quest);
```

### 2. 퀘스트 완료 이벤트

```csharp
void OnQuestCompleted(bool isCompleted)
{
    if (isCompleted)
    {
        Debug.Log("퀘스트 완료!");
        QuestInfo currentQuest = chatManager.CurrentQuest;
        Debug.Log($"완료된 퀘스트: {currentQuest.name}");
        
        // 보상 지급 등 후속 처리
        GiveReward(currentQuest.reward);
    }
}

void GiveReward(string reward)
{
    Debug.Log($"보상 지급: {reward}");
}
```

### 3. 퀘스트 상태 확인

```csharp
// 현재 퀘스트 정보 확인
QuestInfo currentQuest = chatManager.CurrentQuest;
if (currentQuest != null)
{
    Debug.Log($"현재 퀘스트: {currentQuest.name} ({currentQuest.status})");
}
```

---

## 대화 기록 관리

NPC와의 대화 기록을 관리하고 필요시 삭제할 수 있는 기능을 제공합니다. 서버와 로컬 모두에서 대화 기록을 안전하게 관리할 수 있습니다.

### 1. 대화 기록 삭제

```csharp
// 기본 대화 기록 삭제
chatManager.ClearNPCMemory();

// 삭제 사유와 함께 대화 기록 삭제
chatManager.ClearNPCMemory("사용자 요청으로 인한 초기화");
```

### 2. UI를 통한 대화 기록 삭제

```csharp
public class MemoryManagementUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Button clearMemoryButton;
    public Text statusText;
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    void Start()
    {
        SetupUI();
        SubscribeToEvents();
    }
    
    void SetupUI()
    {
        clearMemoryButton.onClick.AddListener(OnClearMemoryButtonClicked);
        clearMemoryButton.interactable = true;
    }
    
    void SubscribeToEvents()
    {
        chatManager.OnStatusChanged += OnStatusChanged;
        chatManager.OnErrorReceived += OnErrorReceived;
    }
    
    void OnClearMemoryButtonClicked()
    {
        // 확인 다이얼로그 (에디터에서만)
        #if UNITY_EDITOR
        if (Application.isEditor)
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("메모리 삭제 확인", 
                "정말로 NPC 대화 기록을 모두 삭제하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.", 
                "삭제", "취소"))
            {
                return;
            }
        }
        #endif
        
        // 메모리 삭제 실행
        try
        {
            chatManager.ClearNPCMemory("사용자 요청으로 인한 메모리 삭제");
            
            // UI 초기화
            ClearDisplayedMessages();
            
            statusText.text = "대화 기록 삭제 중...";
        }
        catch (System.Exception ex)
        {
            statusText.text = "메모리 삭제 중 오류가 발생했습니다.";
            Debug.LogError($"메모리 삭제 오류: {ex.Message}");
        }
    }
    
    void ClearDisplayedMessages()
    {
        // 화면에 표시된 대화 내용 초기화
        Text[] messageTexts = FindObjectsOfType<Text>();
        foreach (Text text in messageTexts)
        {
            if (text.name.Contains("NPC") || text.name.Contains("Player"))
            {
                text.text = "";
            }
        }
    }
    
    void OnStatusChanged(string status)
    {
        if (status.Contains("메모리") || status.Contains("삭제"))
        {
            statusText.text = status;
        }
    }
    
    void OnErrorReceived(string error)
    {
        if (error.Contains("메모리"))
        {
            statusText.text = $"오류: {error}";
        }
    }
}
```

### 3. 자동 메모리 관리

```csharp
public class AutoMemoryManager : MonoBehaviour
{
    [Header("자동 삭제 설정")]
    public bool autoDeleteEnabled = false;
    public float autoDeleteIntervalHours = 24f; // 24시간마다
    public int maxConversationCount = 100; // 최대 대화 수
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    private float lastDeleteTime;
    
    void Start()
    {
        lastDeleteTime = Time.time;
        
        if (autoDeleteEnabled)
        {
            InvokeRepeating(nameof(CheckAutoDelete), autoDeleteIntervalHours * 3600f, autoDeleteIntervalHours * 3600f);
        }
    }
    
    void CheckAutoDelete()
    {
        if (chatManager != null && autoDeleteEnabled)
        {
            // 시간 기반 자동 삭제
            if (Time.time - lastDeleteTime >= autoDeleteIntervalHours * 3600f)
            {
                chatManager.ClearNPCMemory("자동 메모리 정리 - 시간 기반");
                lastDeleteTime = Time.time;
                Debug.Log("[AutoMemoryManager] 시간 기반 자동 메모리 삭제 실행");
            }
            
            // 대화 수 기반 자동 삭제 (NPCConversationManager 접근 필요시)
            // if (conversationCount > maxConversationCount)
            // {
            //     chatManager.ClearNPCMemory("자동 메모리 정리 - 대화 수 초과");
            // }
        }
    }
    
    // 수동으로 자동 삭제 실행
    public void TriggerAutoDelete()
    {
        if (chatManager != null)
        {
            chatManager.ClearNPCMemory("수동 트리거된 자동 삭제");
            Debug.Log("[AutoMemoryManager] 수동 자동 삭제 실행");
        }
    }
}
```

### 4. 메모리 삭제 상태 모니터링

```csharp
public class MemoryStatusMonitor : MonoBehaviour
{
    [Header("UI 참조")]
    public Text memoryStatusText;
    public Button refreshButton;
    public Slider memoryUsageSlider;
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    void Start()
    {
        refreshButton.onClick.AddListener(RefreshMemoryStatus);
        
        // 주기적으로 상태 업데이트
        InvokeRepeating(nameof(UpdateMemoryStatus), 1f, 5f);
    }
    
    void UpdateMemoryStatus()
    {
        if (chatManager == null) return;
        
        // 시스템 상태 확인
        string systemStatus = chatManager.GetSystemStatus();
        bool isSystemBusy = chatManager.IsSystemBusy;
        
        // 상태 텍스트 업데이트
        string statusText = $"시스템 상태: {systemStatus}\n";
        statusText += $"시스템 사용 중: {(isSystemBusy ? "예" : "아니오")}\n";
        
        // 현재 NPC 정보
        if (chatManager.CurrentNPC != null)
        {
            statusText += $"현재 NPC: {chatManager.CurrentNPC.name}\n";
        }
        
        // 현재 퀘스트 정보
        if (chatManager.CurrentQuest != null)
        {
            statusText += $"현재 퀘스트: {chatManager.CurrentQuest.name}\n";
        }
        
        memoryStatusText.text = statusText;
        
        // 메모리 사용량 시뮬레이션 (실제 구현 시 적절한 값으로 대체)
        float memoryUsage = UnityEngine.Random.Range(0.1f, 0.8f);
        memoryUsageSlider.value = memoryUsage;
    }
    
    void RefreshMemoryStatus()
    {
        UpdateMemoryStatus();
        Debug.Log("[MemoryStatusMonitor] 메모리 상태 새로고침");
    }
}
```

### 5. 선택적 대화 기록 관리

```csharp
public class SelectiveMemoryManager : MonoBehaviour
{
    [Header("선택적 삭제 설정")]
    public bool preserveQuestDialogues = true; // 퀘스트 관련 대화 보존
    public bool preserveImportantKeywords = true; // 중요 키워드 포함 대화 보존
    public string[] importantKeywords = {"퀘스트", "미션", "보상", "중요"};
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    // 선택적 메모리 삭제 (향후 서버 API 지원 시 구현 가능)
    public void ClearNonEssentialMemory()
    {
        // 현재는 전체 삭제만 지원하지만, 향후 선택적 삭제 API 개발 시 사용
        Debug.Log("[SelectiveMemoryManager] 선택적 메모리 삭제 - 현재는 전체 삭제로 처리");
        
        string reason = "선택적 메모리 정리 - 비필수 대화 삭제";
        
        if (preserveQuestDialogues)
        {
            reason += " (퀘스트 대화 보존)";
        }
        
        if (preserveImportantKeywords)
        {
            reason += " (중요 키워드 대화 보존)";
        }
        
        chatManager.ClearNPCMemory(reason);
    }
    
    // 메모리 삭제 전 백업 (로컬 파일로 저장)
    public void BackupMemoryBeforeDelete()
    {
        try
        {
            // 현재 대화 상태를 로컬 파일로 백업
            string backupData = $"Backup Time: {System.DateTime.Now}\n";
            backupData += $"NPC: {chatManager.CurrentNPC?.name ?? "Unknown"}\n";
            backupData += $"Quest: {chatManager.CurrentQuest?.name ?? "None"}\n";
            
            string filePath = Application.persistentDataPath + "/npc_conversation_backup.txt";
            System.IO.File.WriteAllText(filePath, backupData);
            
            Debug.Log($"[SelectiveMemoryManager] 메모리 백업 완료: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SelectiveMemoryManager] 메모리 백업 실패: {ex.Message}");
        }
    }
}
```

### 6. 메모리 삭제 이벤트 처리

```csharp
public class MemoryDeleteEventHandler : MonoBehaviour
{
    [Header("이벤트 처리 설정")]
    public bool showDeleteNotification = true;
    public float notificationDuration = 3f;
    
    [Header("UI 참조")]
    public GameObject notificationPanel;
    public Text notificationText;
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    void Start()
    {
        chatManager.OnStatusChanged += OnStatusChanged;
        chatManager.OnErrorReceived += OnErrorReceived;
        
        // 알림 패널 초기 상태
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    void OnStatusChanged(string status)
    {
        if (status.Contains("메모리") && status.Contains("삭제"))
        {
            if (showDeleteNotification)
            {
                ShowNotification(status, false);
            }
            
            Debug.Log($"[MemoryDeleteEventHandler] 메모리 삭제 상태 변경: {status}");
        }
    }
    
    void OnErrorReceived(string error)
    {
        if (error.Contains("메모리"))
        {
            if (showDeleteNotification)
            {
                ShowNotification($"메모리 삭제 오류: {error}", true);
            }
            
            Debug.LogError($"[MemoryDeleteEventHandler] 메모리 삭제 오류: {error}");
        }
    }
    
    void ShowNotification(string message, bool isError)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationPanel.SetActive(true);
            notificationText.text = message;
            notificationText.color = isError ? Color.red : Color.white;
            
            // 일정 시간 후 알림 숨기기
            CancelInvoke(nameof(HideNotification));
            Invoke(nameof(HideNotification), notificationDuration);
        }
    }
    
    void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
}
```

---

## 외부 API 활용

### 1. 게임 시스템과 연동

```csharp
public class GameManager : MonoBehaviour
{
    public NPCChatManager chatManager;
    public PlayerController player;
    public QuestManager questManager;
    
    void Start()
    {
        // 이벤트 구독
        chatManager.OnQuestCompleted += OnQuestCompleted;
        chatManager.OnNPCTextReceived += OnNPCResponse;
        
        // 플레이어 접근 시 NPC 대화 시작
        player.OnNPCApproach += StartNPCConversation;
    }
    
    void StartNPCConversation(NPCInfo npcInfo)
    {
        // NPC 정보 설정
        chatManager.SetNPCInfo(npcInfo);
        
        // 현재 활성 퀘스트가 있다면 설정
        QuestInfo activeQuest = questManager.GetActiveQuest();
        if (activeQuest != null)
        {
            chatManager.SetQuestInfo(activeQuest);
        }
        
        // NPC 선제 대화 시작
        chatManager.StartNPCInitiateChat("플레이어가 NPC에게 접근했을 때");
    }
    
    void OnQuestCompleted(bool isCompleted)
    {
        if (isCompleted)
        {
            questManager.CompleteQuest(chatManager.CurrentQuest.id);
        }
    }
}
```

### 2. UI 시스템과 연동

```csharp
public class NPCChatUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Button recordButton;
    public Button stopButton;
    public Text npcNameText;
    public Text npcMessageText;
    public Text playerMessageText;
    public Text questDescriptionText;
    public Image recordingIndicator;
    
    [Header("NPCChatManager 참조")]
    public NPCChatManager chatManager;
    
    void Start()
    {
        SetupUI();
        SubscribeToEvents();
    }
    
    void SetupUI()
    {
        recordButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        
        stopButton.interactable = false;
    }
    
    void SubscribeToEvents()
    {
        chatManager.OnRecordingStateChanged += OnRecordingStateChanged;
        chatManager.OnNPCTextReceived += OnNPCTextReceived;
        chatManager.OnTranscriptionReceived += OnTranscriptionReceived;
        chatManager.OnStatusChanged += OnStatusChanged;
    }
    
    void StartRecording()
    {
        if (chatManager.TryStartRecording())
        {
            recordButton.interactable = false;
            stopButton.interactable = true;
            recordingIndicator.color = Color.red;
        }
    }
    
    void StopRecording()
    {
        if (chatManager.TryStopRecording())
        {
            recordButton.interactable = true;
            stopButton.interactable = false;
            recordingIndicator.color = Color.gray;
        }
    }
    
    void OnRecordingStateChanged(bool isRecording)
    {
        recordingIndicator.gameObject.SetActive(isRecording);
    }
    
    void OnNPCTextReceived(string npcText)
    {
        npcMessageText.text = npcText;
        
        // NPC 이름 표시
        if (chatManager.CurrentNPC != null)
        {
            npcNameText.text = chatManager.CurrentNPC.name;
        }
    }
    
    void OnTranscriptionReceived(string transcription)
    {
        playerMessageText.text = transcription;
    }
    
    void OnStatusChanged(string status)
    {
        Debug.Log($"상태: {status}");
    }
}
```

### 3. 커스텀 이벤트 처리

```csharp
public class CustomNPCHandler : MonoBehaviour
{
    public NPCChatManager chatManager;
    
    void Start()
    {
        chatManager.OnNPCTextReceived += CheckForSpecialKeywords;
    }
    
    void CheckForSpecialKeywords(string npcText)
    {
        // 특정 키워드 감지 시 특별한 동작
        if (npcText.Contains("비밀") || npcText.Contains("secret"))
        {
            TriggerSecretEvent();
        }
        
        if (npcText.Contains("아이템") || npcText.Contains("item"))
        {
            ShowItemShop();
        }
    }
    
    void TriggerSecretEvent()
    {
        Debug.Log("비밀 이벤트 발생!");
        // 비밀 이벤트 처리 로직
    }
    
    void ShowItemShop()
    {
        Debug.Log("아이템 상점 표시");
        // 아이템 상점 UI 표시
    }
}
```

---

## 트러블슈팅

### 1. 마이크 문제

```csharp
// 마이크 장치 확인
if (!chatManager.GetComponent<AudioRecorder>().IsMicrophoneAvailable)
{
    Debug.LogError("마이크를 사용할 수 없습니다.");
    
    // 마이크 재초기화 시도
    chatManager.GetComponent<AudioRecorder>().InitializeMicrophone();
}

// 사용 가능한 마이크 장치 목록
string[] devices = chatManager.GetComponent<AudioRecorder>().GetAvailableMicrophoneDevices();
Debug.Log($"사용 가능한 마이크: {string.Join(", ", devices)}");
```

### 2. 서버 연결 문제

```csharp
// 서버 연결 테스트
StartCoroutine(TestServerConnection());

IEnumerator TestServerConnection()
{
    yield return chatManager.GetComponent<NPCServerClient>().TestServerConnection();
    
    if (!chatManager.GetComponent<NPCServerClient>().IsServerConnected)
    {
        Debug.LogError("서버에 연결할 수 없습니다.");
        // 오류 처리 로직
    }
}
```

### 3. 오디오 재생 문제

```csharp
// 오디오 시스템 상태 확인
string audioStatus = chatManager.GetAudioBufferStatus();
Debug.Log($"오디오 상태: {audioStatus}");

// 오디오 시스템 초기화
if (chatManager.IsPlayingAudio && /* 문제가 있는 경우 */)
{
    chatManager.ResetAudioSystem();
}
```

### 4. 시스템 상태 확인

```csharp
// 전체 시스템 상태 확인
string systemStatus = chatManager.GetSystemStatus();
Debug.Log($"시스템 상태: {systemStatus}");

// 시스템 완전 초기화
if (/* 문제가 있는 경우 */)
{
    chatManager.ResetSystem();
}
```

### 5. 텍스트 대화 문제

```csharp
// 텍스트 전송 실패 시 확인 사항
if (!chatManager.SendTextToNPC("테스트 메시지"))
{
    // 시스템 상태 확인
    if (chatManager.IsSystemBusy)
    {
        Debug.LogWarning("시스템이 바쁩니다. 잠시 후 다시 시도하세요.");
    }
    
    // 서버 연결 상태 확인
    var serverClient = chatManager.GetComponent<NPCServerClient>();
    if (!serverClient.IsServerConnected)
    {
        Debug.LogError("서버에 연결되지 않았습니다.");
        StartCoroutine(serverClient.TestServerConnection());
    }
}

// 텍스트 길이 제한 확인
string longText = "매우 긴 텍스트...";
if (longText.Length > 1000)
{
    Debug.LogWarning("텍스트가 너무 깁니다. 1000자 이하로 입력해주세요.");
    longText = longText.Substring(0, 1000);
}
```

### 6. 메모리 삭제 문제

```csharp
// 메모리 삭제 실패 시 확인
try
{
    chatManager.ClearNPCMemory("테스트 삭제");
}
catch (System.Exception ex)
{
    Debug.LogError($"메모리 삭제 실패: {ex.Message}");
    
    // 로컬 정리만 시도
    if (chatManager.GetComponent<NPCConversationManager>() != null)
    {
        chatManager.GetComponent<NPCConversationManager>().ClearConversationHistory();
        Debug.Log("로컬 대화 히스토리만 삭제되었습니다.");
    }
}

// 메모리 삭제 상태 모니터링
chatManager.OnStatusChanged += (status) => {
    if (status.Contains("메모리") && status.Contains("삭제"))
    {
        Debug.Log($"메모리 삭제 상태: {status}");
    }
};
```

---

## API 레퍼런스

### NPCChatManager 주요 메서드

#### 녹음 관련
- `bool StartRecording()` - 녹음 시작
- `bool StopRecording()` - 녹음 중지 및 서버 전송
- `void CancelRecording()` - 녹음 취소
- `bool TryStartRecording()` - 안전한 녹음 시작
- `bool TryStopRecording()` - 안전한 녹음 중지

#### 대화 관련
- `bool StartInitiateChat(string initialMessage)` - 선제 대화 시작
- `bool StartNPCInitiateChat(string situation)` - NPC 상황별 대화 시작
- `bool SendTextToNPC(string textMessage)` - 텍스트 메시지를 NPC에게 전송
- `void StopAudioPlayback()` - 오디오 재생 중지

#### 설정 관련
- `void SetNPCInfo(NPCInfo npcInfo)` - NPC 정보 설정
- `void SetQuestInfo(QuestInfo questInfo)` - 퀘스트 정보 설정
- `void SetServerUrl(string url)` - 서버 URL 설정

#### 시스템 관리
- `void ResetSystem()` - 시스템 초기화
- `void ResetAudioSystem()` - 오디오 시스템 초기화
- `void ClearNPCMemory(string reason = "메모리 삭제 요청")` - NPC 대화 기록 삭제
- `string GetSystemStatus()` - 시스템 상태 조회
- `string GetAudioBufferStatus()` - 오디오 버퍼 상태 조회

#### 상태 프로퍼티
- `bool IsRecording` - 녹음 중 여부
- `bool IsProcessing` - 서버 처리 중 여부
- `bool IsPlayingAudio` - 오디오 재생 중 여부
- `bool IsSystemBusy` - 시스템 사용 중 여부
- `NPCInfo CurrentNPC` - 현재 NPC 정보
- `QuestInfo CurrentQuest` - 현재 퀘스트 정보

### 주요 이벤트

#### 상태 변경 이벤트
- `OnStatusChanged(string status)` - 시스템 상태 변경
- `OnRecordingStateChanged(bool isRecording)` - 녹음 상태 변경
- `OnProcessingStateChanged(bool isProcessing)` - 처리 상태 변경
- `OnAudioPlaybackStateChanged(bool isPlaying)` - 오디오 재생 상태 변경

#### 대화 이벤트
- `OnNPCTextReceived(string npcText)` - NPC 텍스트 수신
- `OnTranscriptionReceived(string transcription)` - 음성 인식 결과 수신
- `OnQuestCompleted(bool isCompleted)` - 퀘스트 완료
- `OnErrorReceived(string error)` - 오류 발생

---

## 라이선스 및 지원

이 시스템은 Unity 프로젝트에서 자유롭게 사용할 수 있습니다. 
문제가 발생하거나 개선 사항이 있으면 개발팀에 문의해주세요.

### 요구사항
- Unity 2021.3 이상
- .NET Framework 4.7.1 이상
- 마이크 접근 권한
- 인터넷 연결 (서버 통신용)

### 지원 서버
- Python FastAPI 서버
- SSE (Server-Sent Events) 지원
- WAV 오디오 형식 지원

---

**Happy Coding! 🚀** 