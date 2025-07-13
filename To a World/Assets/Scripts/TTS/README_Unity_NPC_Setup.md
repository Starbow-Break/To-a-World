# Unity6 NPC 대화 시스템 설정 가이드

## 📋 개요

이 가이드는 Unity6에서 FastAPI 기반 NPC 대화 시스템을 연동하는 방법을 설명합니다.
- **STT(Speech-to-Text)**: 플레이어 음성을 텍스트로 변환
- **TTS(Text-to-Speech)**: NPC 응답을 음성으로 변환
- **실시간 스트리밍**: SSE를 통한 실시간 대화 처리
- **NPC 선제 대화**: NPC가 먼저 말을 걸어오는 기능
- **관심사 분리**: NPCChatManager(비즈니스 로직) + NPCChatUI(UI 관리)

## 🛠️ 필요한 파일들

다음 스크립트들을 Unity 프로젝트에 추가해야 합니다:

```
Assets/Scripts/NPC/
├── NPCDataModels.cs      # 데이터 모델 정의
├── NPCChatManager.cs     # 메인 대화 매니저
├── WavUtility.cs         # 오디오 처리 유틸리티
└── NPCChatUI.cs          # UI 예제 스크립트
```

## 🚀 설치 및 설정

### 1단계: 스크립트 임포트

1. 위 4개의 C# 스크립트를 Unity 프로젝트의 `Assets/Scripts/NPC/` 폴더에 복사
2. Unity에서 스크립트가 정상적으로 컴파일되는지 확인

### 2단계: 씬 설정

#### GameManager 오브젝트 생성
```
1. 빈 GameObject 생성 → "NPCChatManager"로 이름 변경
2. NPCChatManager.cs 스크립트 추가
3. AudioSource 컴포넌트 추가 (자동으로 추가됨)
```

#### UI Canvas 설정
```
1. UI → Canvas 생성
2. Canvas 하위에 다음 UI 요소들 생성:

메인 패널 (MainPanel):
├── RecordButton (Button) - 음성 녹음 시작
├── StopRecordButton (Button) - 음성 녹음 종료
├── InitiateButton (Button) - NPC 선제 대화
├── StatusText (Text) - 상태 표시
├── NPCDialogueText (Text) - NPC 대화 내용
├── TranscribedText (Text) - 전사된 플레이어 음성
```

### 3단계: 컴포넌트 연결

1. **NPCChatManager 설정** (비즈니스 로직 담당):
   ```
   - Server Url: http://localhost:8000 (또는 실제 서버 주소)
   - Timeout Seconds: 30
   - NPC Audio Source: AudioSource 컴포넌트
   - Sample Rate: 24000
   - NPC 설정 정보 (현재 NPC, 퀘스트 등)
   ```

2. **NPCChatUI 설정** (UI 관리 담당):
   ```
   - NPC Chat Manager: NPCChatManager 오브젝트 연결
   - Record Button: 녹음 시작 버튼
   - Stop Record Button: 녹음 종료 버튼
   - 모든 UI 요소들을 해당 필드에 연결
   - 매니저의 이벤트를 구독하여 UI 자동 업데이트
   ```

## 🏗️ 구조 설계

### 역할 분리
- **NPCChatManager**: 순수한 비즈니스 로직만 담당
  - 음성 녹음/처리, 서버 통신, 데이터 관리
  - UI와 독립적으로 동작 가능
  - 이벤트 기반으로 상태 변화 알림

- **NPCChatUI**: UI 관리만 담당
  - 매니저의 이벤트 구독
  - 버튼 상태 관리, 텍스트 업데이트
  - 사용자 입력을 매니저로 전달

### 장점
1. **재사용성**: NPCChatManager를 다양한 UI에서 활용 가능
2. **테스트 용이성**: 비즈니스 로직과 UI 분리로 단위 테스트 가능
3. **유지보수성**: 각 클래스의 책임이 명확하게 분리됨
4. **확장성**: 새로운 UI나 기능 추가 시 기존 코드 영향 최소화

### 4단계: 프로젝트 설정

#### Build Settings
```
- Platform: PC, Mac & Linux Standalone (또는 원하는 플랫폼)
- Microphone 권한 활성화 (모바일의 경우)
```

#### Player Settings
```
- Internet Access: Required
- Microphone Usage: Enabled
```

## 🎮 사용 방법

### 기본 사용법

#### 1. 음성 대화 (STT → TTS)
```csharp
// 1. 녹음 시작 버튼 클릭 → 마이크로 음성 녹음 시작
// 2. 녹음 종료 버튼 클릭 → 녹음 중지 및 서버 전송
// 3. 서버에서 STT 처리 (음성 → 텍스트)
// 4. NPC 응답 생성
// 5. TTS로 음성 재생
```

#### 2. 음성 녹음 제어
```csharp
// 녹음 시작 버튼
// - 녹음 중이 아닐 때만 활성화
// - 클릭하면 녹음 시작 및 버튼 비활성화

// 녹음 종료 버튼
// - 녹음 중일 때만 활성화
// - 클릭하면 녹음 종료 및 서버 전송 시작
```

#### 3. NPC 선제 대화
```csharp
// 스크립트에서 호출
NPCChatUI npcUI = FindObjectOfType<NPCChatUI>();
npcUI.TriggerNPCInitiate("플레이어가 상점에 들어왔을 때");
```

### 고급 사용법

#### 매니저 직접 사용 (스크립트에서)
```csharp
// NPCChatManager 참조 얻기
NPCChatManager npcManager = FindObjectOfType<NPCChatManager>();

// 녹음 시작
bool recordStarted = npcManager.TryStartRecording();

// 녹음 종료
bool recordStopped = npcManager.TryStopRecording();

// NPC 선제 대화 시작
bool initiateStarted = npcManager.TryStartInitiateChat("플레이어가 상점에 들어왔을 때");

// 상태 확인
bool isRecording = npcManager.IsRecording;
bool isProcessing = npcManager.IsProcessing;
```

#### 이벤트 구독 (커스텀 UI에서)
```csharp
public class CustomNPCUI : MonoBehaviour
{
    private NPCChatManager npcManager;
    
    void Start()
    {
        npcManager = FindObjectOfType<NPCChatManager>();
        
        // 이벤트 구독
        npcManager.OnStatusChanged += OnStatusChanged;
        npcManager.OnNPCTextReceived += OnNPCTextReceived;
        npcManager.OnRecordingStateChanged += OnRecordingStateChanged;
        npcManager.OnProcessingStateChanged += OnProcessingStateChanged;
    }
    
    private void OnStatusChanged(string status)
    {
        // 상태 메시지 UI 업데이트
    }
    
    private void OnRecordingStateChanged(bool isRecording)
    {
        // 녹음 상태에 따른 UI 변경
    }
}
```

#### NPC 정보 변경
```csharp
NPCInfo npcInfo = new NPCInfo
{
    name = "상점주인 김철수",
    gender = NPCGender.male,
    personality = "친근하지만 약간 까칠한",
    background = "마을에서 30년간 무기점을 운영하는 베테랑 상인",
    age = 45,
    voice_style = "깊고 안정적인"
};

npcChatManager.SetNPCInfo(npcInfo);
```

#### 퀘스트 설정
```csharp
QuestInfo quest = new QuestInfo
{
    id = "quest_001",
    name = "잃어버린 검 찾기",
    description = "마을 근처에서 잃어버린 가보 검을 찾아주세요",
    completion_condition = "플레이어가 검을 찾아서 가져왔을 때",
    reward = "골드 1000개와 경험치 500",
    status = QuestStatus.active
};

npcChatManager.SetQuestInfo(quest);
```

## 🔧 API 세부 사항

### npc_chat_audio API
```
POST /npc/chat_audio
Content-Type: application/json

Request Body:
{
    "audio_data": "base64_encoded_wav_data",
    "npc": { NPC 정보 },
    "quest": { 퀘스트 정보 (선택사항) },
    "conversation_history": [ 대화 기록 ],
    "memory_key": "device_unique_id",
    "language": "ko",
    "use_thinking": false,
    "audio_format": "wav"
}

Response: SSE 스트리밍
- npc_metadata: 메타데이터
- npc_text: 각 문장 텍스트
- npc_audio: 각 문장 오디오 (Base64)
- npc_complete: 완료 신호
- npc_error: 오류 발생
```

### npc_initiate_chat API
```
POST /npc/initiate_chat
Content-Type: application/json

Request Body:
{
    "npc": { NPC 정보 },
    "quest": { 퀘스트 정보 (선택사항) },
    "initial_message": "NPC가 말을 걸어오는 상황",
    "conversation_history": [ 대화 기록 ],
    "memory_key": "device_unique_id",
    "language": "ko",
    "use_thinking": false
}

Response: SSE 스트리밍 (위와 동일)
```

## 🎯 실제 게임 연동 예제

### 1. 트리거 기반 NPC 대화
```csharp
public class NPCTrigger : MonoBehaviour
{
    [SerializeField] private NPCChatUI npcChatUI;
    [SerializeField] private string npcName = "상점주인";
    [SerializeField] private string triggerMessage = "플레이어가 상점에 들어왔을 때";
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // NPC 정보 설정
            NPCInfo npcInfo = new NPCInfo
            {
                name = npcName,
                gender = NPCGender.male,
                personality = "친근한 상점주인",
                background = "마을의 무기 상점을 운영하는 중년 남성"
            };
            
            npcChatUI.GetComponent<NPCChatManager>().SetNPCInfo(npcInfo);
            
            // NPC가 먼저 말을 걸어옴
            npcChatUI.TriggerNPCInitiate(triggerMessage);
        }
    }
}
```

### 2. 퀘스트 연동 대화
```csharp
public class QuestManager : MonoBehaviour
{
    [SerializeField] private NPCChatManager npcChatManager;
    
    public void StartQuest(string questId)
    {
        QuestInfo quest = GetQuestById(questId);
        npcChatManager.SetQuestInfo(quest);
        
        // 퀘스트 시작 대화
        NPCChatUI npcUI = FindObjectOfType<NPCChatUI>();
        npcUI.TriggerNPCInitiate("플레이어에게 새로운 퀘스트를 제안하고 싶을 때");
    }
    
    private QuestInfo GetQuestById(string questId)
    {
        // 퀘스트 데이터베이스에서 퀘스트 정보 로드
        return new QuestInfo
        {
            id = questId,
            name = "드래곤 토벌",
            description = "마을을 위협하는 드래곤을 처치해주세요",
            completion_condition = "드래곤을 처치하고 증거품을 가져올 때",
            reward = "전설급 무기와 골드 10000개"
        };
    }
}
```

## 🔍 트러블슈팅

### 자주 발생하는 문제들

#### 1. 마이크 권한 문제
```
문제: 마이크에 접근할 수 없습니다
해결책:
- Windows: Unity Hub → 프로젝트 설정 → Microphone 권한 확인
- 모바일: Player Settings → XR Settings → Microphone Usage Description 설정
```

#### 2. 서버 연결 실패
```
문제: API 요청이 실패합니다
해결책:
1. 서버가 실행 중인지 확인
2. 방화벽 설정 확인
3. 서버 URL이 정확한지 확인 (http://localhost:8000)
4. CORS 설정이 되어있는지 서버에서 확인
```

#### 3. 오디오 재생 문제
```
문제: NPC 음성이 재생되지 않습니다
해결책:
1. AudioSource 컴포넌트가 추가되었는지 확인
2. Audio Listener가 씬에 있는지 확인
3. 시스템 볼륨이 켜져있는지 확인
4. WAV 디코딩 오류가 없는지 콘솔 로그 확인
```

#### 4. JSON 직렬화 오류
```
문제: 데이터 전송 시 오류가 발생합니다
해결책:
1. 모든 직렬화되는 클래스에 [Serializable] 속성 확인
2. Null 값이 있는 필드 확인
3. Unity JsonUtility 한계 (중첩 객체 지원 제한) 고려
```

## 📱 플랫폼별 고려사항

### Windows/Mac/Linux
- 마이크 권한이 자동으로 요청됨
- 네트워크 설정이 간단함

### Android
```csharp
// AndroidManifest.xml에 권한 추가 필요
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.INTERNET" />
```

### iOS
```csharp
// Info.plist에 권한 설명 추가
<key>NSMicrophoneUsageDescription</key>
<string>음성 대화를 위해 마이크 접근이 필요합니다.</string>
```

## 🎨 커스터마이징 팁

### UI 스타일링
```csharp
// 버튼 색상 변경
recordButton.GetComponent<Image>().color = Color.red;

// 텍스트 효과 추가
npcDialogueText.GetComponent<Outline>().effectColor = Color.black;
```

### 오디오 효과
```csharp
// 음성에 효과 추가
npcAudioSource.pitch = 1.2f; // 목소리 톤 변경
npcAudioSource.volume = 0.8f; // 볼륨 조절
```

### 애니메이션 연동
```csharp
// NPC 말할 때 애니메이션 재생
private void OnNPCTextReceived(string npcText)
{
    Animator npcAnimator = GetComponent<Animator>();
    npcAnimator.SetTrigger("StartTalking");
}
```

## 📞 지원 및 문의

문제가 발생하거나 추가 기능이 필요한 경우:
1. Unity 콘솔 로그 확인
2. 서버 로그 확인
3. 네트워크 연결 상태 확인
4. API 응답 포맷 확인

---

**주의**: 이 시스템을 실제 게임에 적용하기 전에 충분한 테스트를 진행하고, 사용자의 개인정보 보호에 유의하시기 바랍니다. 