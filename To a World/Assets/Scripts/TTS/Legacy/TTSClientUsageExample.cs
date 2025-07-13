using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TTSSystem;

/// <summary>
/// Unity 실시간 TTS 클라이언트 사용 예제
/// 
/// 이 클래스는 UnityRealtimeTTSClient의 모든 기능을 사용하는 방법을 보여줍니다.
/// 실제 프로젝트에서 참고용으로 사용하거나 테스트 목적으로 활용할 수 있습니다.
/// 
/// 설정 방법:
/// 1. GameObject에 이 스크립트를 추가
/// 2. UnityRealtimeTTSClient 컴포넌트도 같이 추가
/// 3. UI 요소들을 Inspector에서 연결
/// 4. 서버 URL 설정
/// 5. 플레이 버튼을 눌러 테스트
/// 
/// 필요한 UI 요소들:
/// - InputField: 텍스트 입력용
/// - Button: TTS 요청 버튼
/// - Text: 상태 표시용
/// - Slider: 볼륨 조절용
/// - Toggle: 옵션 설정용
/// </summary>
public class TTSClientUsageExample : MonoBehaviour
{
    #region UI References
    
    [Header("UI 컴포넌트 연결")]
    [SerializeField]
    [Tooltip("텍스트 입력 필드")]
    private InputField textInputField;
    
    [SerializeField]
    [Tooltip("TTS 시작 버튼")]
    private Button startTTSButton;
    
    [SerializeField]
    [Tooltip("TTS 중지 버튼")]
    private Button stopTTSButton;
    
    [SerializeField]
    [Tooltip("상태 표시 텍스트")]
    private Text statusText;
    
    [SerializeField]
    [Tooltip("생성된 텍스트 표시")]
    private Text generatedText;
    
    [SerializeField]
    [Tooltip("볼륨 조절 슬라이더")]
    private Slider volumeSlider;
    
    [SerializeField]
    [Tooltip("디버그 로그 표시 토글")]
    private Toggle debugToggle;
    
    [SerializeField]
    [Tooltip("언어 선택 드롭다운")]
    private Dropdown languageDropdown;
    
    [SerializeField]
    [Tooltip("캐릭터 선택 드롭다운")]
    private Dropdown characterDropdown;
    
    #endregion
    
    #region TTS Client Reference
    
    /// <summary>
    /// TTS 클라이언트 참조
    /// Inspector에서 직접 연결하거나 자동으로 찾습니다
    /// </summary>
    [Header("TTS 클라이언트")]
    [SerializeField]
    private UnityRealtimeTTSClient ttsClient;
    
    #endregion
    
    #region Private Fields
    
    /// <summary>
    /// 현재 재생 중인 문장 수
    /// </summary>
    private int currentSentenceCount = 0;
    
    /// <summary>
    /// 총 처리된 문장 수
    /// </summary>
    private int totalSentenceCount = 0;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Unity Start 메서드
    /// 초기 설정 및 이벤트 연결
    /// </summary>
    void Start()
    {
        InitializeTTSClient();
        SetupUI();
        SetupEventListeners();
        UpdateStatus("준비 완료 - TTS를 시작하려면 텍스트를 입력하고 버튼을 클릭하세요.");
    }
    
    /// <summary>
    /// Unity OnDestroy 메서드
    /// 이벤트 정리
    /// </summary>
    void OnDestroy()
    {
        CleanupEventListeners();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// TTS 클라이언트를 초기화합니다
    /// </summary>
    private void InitializeTTSClient()
    {
        // TTS 클라이언트가 연결되지 않았으면 자동으로 찾기
        if (ttsClient == null)
        {
            ttsClient = FindObjectOfType<UnityRealtimeTTSClient>();
            
            if (ttsClient == null)
            {
                // TTS 클라이언트가 없으면 동적으로 생성
                GameObject ttsObject = new GameObject("TTS_Client");
                ttsClient = ttsObject.AddComponent<UnityRealtimeTTSClient>();
                Debug.Log("TTS 클라이언트를 자동으로 생성했습니다.");
            }
        }
        
        // 기본 설정
        ttsClient.serverUrl = "http://localhost:8000";
        ttsClient.defaultLanguage = TTSConstants.Languages.Korean;
        ttsClient.defaultCharacter = TTSConstants.Characters.FriendlyAssistant;
        ttsClient.enableDebugLogs = true;
    }
    
    /// <summary>
    /// UI 요소들을 초기화합니다
    /// </summary>
    private void SetupUI()
    {
        // 기본 텍스트 설정
        if (textInputField != null)
        {
            textInputField.text = "안녕하세요! 실시간 TTS 테스트입니다.";
        }
        
        // 언어 드롭다운 설정
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "한국어 (ko)",
                "영어 (en)",
                "일본어 (ja)",
                "중국어 (zh)",
                "스페인어 (es)"
            });
            languageDropdown.value = 0; // 한국어 기본 선택
        }
        
        // 캐릭터 드롭다운 설정
        if (characterDropdown != null)
        {
            characterDropdown.ClearOptions();
            characterDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "친근한 도우미 (friendly_assistant)",
                "전문 가이드 (professional_guide)",
                "캐주얼 친구 (casual_friend)",
                "현명한 선생님 (wise_teacher)",
                "활기찬 호스트 (energetic_host)"
            });
            characterDropdown.value = 0; // 친근한 도우미 기본 선택
        }
        
        // 볼륨 슬라이더 설정
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = 1f;
        }
        
        // 디버그 토글 설정
        if (debugToggle != null)
        {
            debugToggle.isOn = true;
        }
    }
    
    /// <summary>
    /// 이벤트 리스너들을 설정합니다
    /// </summary>
    private void SetupEventListeners()
    {
        // UI 이벤트
        if (startTTSButton != null)
            startTTSButton.onClick.AddListener(OnStartTTSClicked);
        
        if (stopTTSButton != null)
            stopTTSButton.onClick.AddListener(OnStopTTSClicked);
        
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        
        if (debugToggle != null)
            debugToggle.onValueChanged.AddListener(OnDebugToggleChanged);
        
        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        
        if (characterDropdown != null)
            characterDropdown.onValueChanged.AddListener(OnCharacterChanged);
        
        // TTS 클라이언트 이벤트
        if (ttsClient != null)
        {
            ttsClient.OnTextGenerated += OnTextGenerated;
            ttsClient.OnSentenceCompleted += OnSentenceCompleted;
            ttsClient.OnAllCompleted += OnAllCompleted;
            ttsClient.OnError += OnError;
            ttsClient.OnRequestStarted += OnRequestStarted;
            ttsClient.OnRequestCompleted += OnRequestCompleted;
        }
    }
    
    /// <summary>
    /// 이벤트 리스너들을 정리합니다
    /// </summary>
    private void CleanupEventListeners()
    {
        // UI 이벤트 정리
        if (startTTSButton != null)
            startTTSButton.onClick.RemoveListener(OnStartTTSClicked);
        
        if (stopTTSButton != null)
            stopTTSButton.onClick.RemoveListener(OnStopTTSClicked);
        
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        
        if (debugToggle != null)
            debugToggle.onValueChanged.RemoveListener(OnDebugToggleChanged);
        
        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        
        if (characterDropdown != null)
            characterDropdown.onValueChanged.RemoveListener(OnCharacterChanged);
        
        // TTS 클라이언트 이벤트 정리
        if (ttsClient != null)
        {
            ttsClient.OnTextGenerated -= OnTextGenerated;
            ttsClient.OnSentenceCompleted -= OnSentenceCompleted;
            ttsClient.OnAllCompleted -= OnAllCompleted;
            ttsClient.OnError -= OnError;
            ttsClient.OnRequestStarted -= OnRequestStarted;
            ttsClient.OnRequestCompleted -= OnRequestCompleted;
        }
    }
    
    #endregion
    
    #region UI Event Handlers
    
    /// <summary>
    /// TTS 시작 버튼 클릭 이벤트 핸들러
    /// </summary>
    private void OnStartTTSClicked()
    {
        if (ttsClient == null)
        {
            UpdateStatus("오류: TTS 클라이언트가 없습니다.");
            return;
        }
        
        string inputText = textInputField != null ? textInputField.text : "테스트 텍스트입니다.";
        
        if (string.IsNullOrEmpty(inputText.Trim()))
        {
            UpdateStatus("오류: 입력 텍스트가 비어있습니다.");
            return;
        }
        
        // 현재 설정된 언어와 캐릭터 가져오기
        string language = GetSelectedLanguage();
        string character = GetSelectedCharacter();
        
        UpdateStatus($"TTS 요청 시작: '{inputText}' (언어: {language}, 캐릭터: {character})");
        
        // 비동기 TTS 요청 시작
        ttsClient.StartRealtimeTTSAsync(inputText, character, language);
    }
    
    /// <summary>
    /// TTS 중지 버튼 클릭 이벤트 핸들러
    /// </summary>
    private void OnStopTTSClicked()
    {
        if (ttsClient != null)
        {
            ttsClient.CancelCurrentRequest();
            ttsClient.StopAllAudio();
            UpdateStatus("TTS가 중지되었습니다.");
        }
    }
    
    /// <summary>
    /// 볼륨 슬라이더 변경 이벤트 핸들러
    /// </summary>
    /// <param name="volume">새로운 볼륨 값</param>
    private void OnVolumeChanged(float volume)
    {
        // 모든 AudioSource의 볼륨 조절
        if (ttsClient != null)
        {
            AudioSource[] audioSources = ttsClient.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in audioSources)
            {
                source.volume = volume;
            }
        }
    }
    
    /// <summary>
    /// 디버그 토글 변경 이벤트 핸들러
    /// </summary>
    /// <param name="isEnabled">디버그 로그 활성화 여부</param>
    private void OnDebugToggleChanged(bool isEnabled)
    {
        if (ttsClient != null)
        {
            ttsClient.enableDebugLogs = isEnabled;
        }
    }
    
    /// <summary>
    /// 언어 드롭다운 변경 이벤트 핸들러
    /// </summary>
    /// <param name="index">선택된 언어 인덱스</param>
    private void OnLanguageChanged(int index)
    {
        string language = GetSelectedLanguage();
        if (ttsClient != null)
        {
            ttsClient.defaultLanguage = language;
        }
        UpdateStatus($"언어가 {language}로 변경되었습니다.");
    }
    
    /// <summary>
    /// 캐릭터 드롭다운 변경 이벤트 핸들러
    /// </summary>
    /// <param name="index">선택된 캐릭터 인덱스</param>
    private void OnCharacterChanged(int index)
    {
        string character = GetSelectedCharacter();
        if (ttsClient != null)
        {
            ttsClient.SetDefaultCharacter(character);
        }
        UpdateStatus($"캐릭터가 {character}로 변경되었습니다.");
    }
    
    #endregion
    
    #region TTS Event Handlers
    
    /// <summary>
    /// 텍스트 생성 완료 이벤트 핸들러
    /// </summary>
    /// <param name="text">생성된 텍스트</param>
    private void OnTextGenerated(string text)
    {
        if (generatedText != null)
        {
            generatedText.text += text + " ";
        }
        Debug.Log($"생성된 텍스트: {text}");
    }
    
    /// <summary>
    /// 문장 재생 완료 이벤트 핸들러
    /// </summary>
    /// <param name="sentenceId">완료된 문장 ID</param>
    /// <param name="text">완료된 문장 텍스트</param>
    private void OnSentenceCompleted(int sentenceId, string text)
    {
        currentSentenceCount++;
        UpdateStatus($"문장 {sentenceId} 재생 완료 ({currentSentenceCount}/{totalSentenceCount})");
        Debug.Log($"문장 {sentenceId} 재생 완료: {text}");
    }
    
    /// <summary>
    /// 전체 TTS 완료 이벤트 핸들러
    /// </summary>
    /// <param name="totalSentences">총 처리된 문장 수</param>
    private void OnAllCompleted(int totalSentences)
    {
        totalSentenceCount = totalSentences;
        UpdateStatus($"TTS 완료! 총 {totalSentences}개 문장 처리됨");
        Debug.Log($"전체 TTS 완료: {totalSentences}개 문장");
        
        // 완료 후 UI 초기화
        StartCoroutine(ResetUIAfterDelay(2f));
    }
    
    /// <summary>
    /// TTS 오류 이벤트 핸들러
    /// </summary>
    /// <param name="error">오류 메시지</param>
    private void OnError(string error)
    {
        UpdateStatus($"오류 발생: {error}");
        Debug.LogError($"TTS 오류: {error}");
        
        // 오류 발생 시 UI 초기화
        StartCoroutine(ResetUIAfterDelay(3f));
    }
    
    /// <summary>
    /// TTS 요청 시작 이벤트 핸들러
    /// </summary>
    private void OnRequestStarted()
    {
        currentSentenceCount = 0;
        totalSentenceCount = 0;
        
        if (generatedText != null)
        {
            generatedText.text = "";
        }
        
        // 시작 버튼 비활성화, 중지 버튼 활성화
        if (startTTSButton != null)
            startTTSButton.interactable = false;
        
        if (stopTTSButton != null)
            stopTTSButton.interactable = true;
        
        UpdateStatus("TTS 요청 처리 중...");
    }
    
    /// <summary>
    /// TTS 요청 완료 이벤트 핸들러
    /// </summary>
    private void OnRequestCompleted()
    {
        // 시작 버튼 활성화, 중지 버튼 비활성화
        if (startTTSButton != null)
            startTTSButton.interactable = true;
        
        if (stopTTSButton != null)
            stopTTSButton.interactable = false;
        
        UpdateStatus("TTS 요청 완료");
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// 상태 텍스트를 업데이트합니다
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"상태 업데이트: {message}");
    }
    
    /// <summary>
    /// 선택된 언어 코드를 반환합니다
    /// </summary>
    /// <returns>언어 코드</returns>
    private string GetSelectedLanguage()
    {
        if (languageDropdown == null) return TTSConstants.Languages.Korean;
        
        switch (languageDropdown.value)
        {
            case 0: return TTSConstants.Languages.Korean;
            case 1: return TTSConstants.Languages.English;
            case 2: return TTSConstants.Languages.Japanese;
            case 3: return TTSConstants.Languages.Chinese;
            case 4: return TTSConstants.Languages.Spanish;
            default: return TTSConstants.Languages.Korean;
        }
    }
    
    /// <summary>
    /// 선택된 캐릭터 이름을 반환합니다
    /// </summary>
    /// <returns>캐릭터 이름</returns>
    private string GetSelectedCharacter()
    {
        if (characterDropdown == null) return TTSConstants.Characters.FriendlyAssistant;
        
        switch (characterDropdown.value)
        {
            case 0: return TTSConstants.Characters.FriendlyAssistant;
            case 1: return TTSConstants.Characters.ProfessionalGuide;
            case 2: return TTSConstants.Characters.CasualFriend;
            case 3: return TTSConstants.Characters.WiseTeacher;
            case 4: return TTSConstants.Characters.EnergeticHost;
            default: return TTSConstants.Characters.FriendlyAssistant;
        }
    }
    
    /// <summary>
    /// 지정된 시간 후 UI를 초기화합니다
    /// </summary>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns>코루틴</returns>
    private IEnumerator ResetUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 생성된 텍스트 초기화 (선택사항)
        // if (generatedText != null)
        // {
        //     generatedText.text = "";
        // }
        
        UpdateStatus("다음 TTS 요청을 위해 준비되었습니다.");
    }
    
    #endregion
    
    #region Public Methods (For External Use)
    
    /// <summary>
    /// 외부에서 직접 TTS를 실행할 수 있는 메서드
    /// </summary>
    /// <param name="text">변환할 텍스트</param>
    /// <param name="language">언어 (선택사항)</param>
    /// <param name="character">캐릭터 (선택사항)</param>
    public void ExecuteTTS(string text, string language = null, string character = null)
    {
        if (ttsClient == null)
        {
            Debug.LogError("TTS 클라이언트가 초기화되지 않았습니다.");
            return;
        }
        
        language = language ?? ttsClient.defaultLanguage;
        character = character ?? ttsClient.defaultCharacter;
        
        ttsClient.StartRealtimeTTSAsync(text, character, language);
    }
    
    /// <summary>
    /// TTS 클라이언트 설정을 변경하는 메서드
    /// </summary>
    /// <param name="serverUrl">서버 URL</param>
    /// <param name="defaultLanguage">기본 언어</param>
    /// <param name="defaultCharacter">기본 캐릭터</param>
    public void ConfigureTTS(string serverUrl = null, string defaultLanguage = null, string defaultCharacter = null)
    {
        if (ttsClient == null) return;
        
        if (!string.IsNullOrEmpty(serverUrl))
            ttsClient.SetServerUrl(serverUrl);
        
        if (!string.IsNullOrEmpty(defaultLanguage))
            ttsClient.defaultLanguage = defaultLanguage;
        
        if (!string.IsNullOrEmpty(defaultCharacter))
            ttsClient.SetDefaultCharacter(defaultCharacter);
    }
    
    #endregion
} 