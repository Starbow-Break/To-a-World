using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TTSSystem;

/// <summary>
/// Unity 실시간 TTS 시스템을 위한 UI 컨트롤러
/// 
/// 주요 기능:
/// 1. 텍스트 입력을 통한 TTS 요청
/// 2. 음성 녹음을 통한 대화형 TTS
/// 3. 언어 및 캐릭터 선택
/// 4. 실시간 상태 표시 및 진행률 관리
/// 5. 완전한 이벤트 기반 UI 업데이트
/// 
/// 사용 방법:
/// ```csharp
/// // UI 컨트롤러 가져오기
/// var uiController = FindObjectOfType<TTSUIController>();
/// 
/// // 텍스트 설정
/// uiController.SetInputText("안녕하세요!");
/// 
/// // 언어 및 캐릭터 설정
/// uiController.SetLanguage(TTSConstants.Languages.Korean);
/// uiController.SetCharacter(TTSConstants.Character.FriendlyAssistant);
/// ```
/// </summary>
public class TTSUIController : MonoBehaviour
{
    #region Events

    public event Action OnStartRecording;
    public event Action OnStopRecording;
    
    #endregion
    
    #region UI References
    
    [Header("UI 참조")]
    [SerializeField]
    [Tooltip("텍스트 입력 필드")]
    public TMP_InputField inputTextField;
    
    [SerializeField]
    [Tooltip("생성 시작 버튼")]
    public Button generateButton;
    
    [SerializeField]
    [Tooltip("중지 버튼")]
    public Button stopButton;
    
    [SerializeField]
    [Tooltip("녹음 시작 버튼")]
    public Button recordStartButton;
    
    [SerializeField]
    [Tooltip("녹음 중지 버튼")]
    public Button recordStopButton;
    
    [SerializeField]
    [Tooltip("캐릭터 선택 드롭다운")]
    public TMP_Dropdown characterDropdown;
    
    [SerializeField]
    [Tooltip("언어 선택 드롭다운")]
    public TMP_Dropdown languageDropdown;
    
    #endregion
    
    #region Status Display
    
    [Header("상태 표시")]
    [SerializeField]
    [Tooltip("현재 상태 텍스트")]
    public TextMeshProUGUI statusText;
    
    [SerializeField]
    [Tooltip("생성된 텍스트 표시")]
    public TextMeshProUGUI generatedText;
    
    [SerializeField]
    [Tooltip("진행률 슬라이더")]
    public Slider progressSlider;
    
    [SerializeField]
    [Tooltip("현재 처리 중인 문장 표시")]
    public TextMeshProUGUI currentSentenceText;
    
    [SerializeField]
    [Tooltip("총 문장 수 표시")]
    public TextMeshProUGUI sentenceCountText;
    
    #endregion
    
    #region TTS System
    
    [Header("TTS 시스템")]
    [SerializeField]
    [Tooltip("TTS 클라이언트 참조")]
    public UnityRealtimeTTSClient ttsClient;
    
    #endregion
    
    #region Settings
    
    [Header("UI 설정")]
    [SerializeField]
    [Tooltip("사용 가능한 캐릭터 목록")]
    public CharacterOption[] availableCharacters = {
        new CharacterOption("친근한 도우미", TTSConstants.Characters.FriendlyAssistant),
        new CharacterOption("전문 안내원", TTSConstants.Characters.ProfessionalGuide),
        new CharacterOption("지혜로운 선생님", TTSConstants.Characters.WiseTeacher),
        new CharacterOption("활기찬 진행자", TTSConstants.Characters.EnergeticHost)
    };
    
    [SerializeField]
    [Tooltip("사용 가능한 언어 목록")]
    public LanguageOption[] availableLanguages = {
        new LanguageOption("한국어", TTSConstants.Languages.Korean),
        new LanguageOption("English", TTSConstants.Languages.English),
        new LanguageOption("日本語", TTSConstants.Languages.Japanese),
        new LanguageOption("中文", TTSConstants.Languages.Chinese)
    };
    
    [Header("음성 녹음 설정")]
    [SerializeField]
    [Tooltip("녹음 샘플링 레이트")]
    public int recordingSampleRate = 44100;
    
    [SerializeField]
    [Tooltip("최대 녹음 시간 (초)")]
    public int maxRecordingTime = 30;
    
    #endregion
    
    #region Private Fields
    
    /// <summary>총 문장 수</summary>
    private int totalSentences = 0;
    
    /// <summary>완료된 문장 수</summary>
    private int completedSentences = 0;
    
    /// <summary>현재 생성 중인지 여부</summary>
    private bool isGenerating = false;
    
    /// <summary>현재 녹음 중인지 여부</summary>
    private bool isRecording = false;
    
    /// <summary>녹음된 오디오 클립</summary>
    private AudioClip recordedClip;
    
    /// <summary>마이크 장치 이름</summary>
    private string microphoneDevice;
    
    /// <summary>마지막 생성된 텍스트 누적</summary>
    private StringBuilder generatedTextBuilder = new StringBuilder();
    
    /// <summary>마이크 미리 초기화 여부</summary>
    private bool isMicrophonePreWarmed = false;

    /// <summary>마이크 초기화 진행 중 여부</summary>
    private bool isMicrophoneInitializing = false;

    /// <summary>마이크 권한 확인 완료 여부</summary>
    private bool microphonePermissionChecked = false;

    /// <summary>최적화된 녹음 설정</summary>
    private readonly int optimizedSampleRate = 22050;
    private readonly int preWarmDuration = 1;

    /// <summary>마이크 초기화 오류 메시지</summary>
    private string microphoneError = "";

    /// <summary>녹음 시작 오류 메시지</summary>
    private string recordingStartError = "";

    private NpcInfo currentNpc = null;

    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Unity Start 메서드
    /// UI 초기화 및 이벤트 설정
    /// </summary>
    void Start()
    {
        InitializeUI();
        SetupEventListeners();
        ValidateReferences();
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc += OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc += OnExitedNpc;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc -= OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc -= OnExitedNpc;
    }
    
    /// <summary>
    /// Unity OnDestroy 메서드
    /// 리소스 정리 및 이벤트 구독 해제
    /// </summary>
    void OnDestroy()
    {
        CleanupResources();
        UnsubscribeEvents();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// UI 컴포넌트들을 초기화합니다
    /// </summary>
    private void InitializeUI()
    {
        SetupCharacterDropdown();
        SetupLanguageDropdown();
        SetupInitialState();
        InitializeMicrophone();
        SetDefaultValues();
    }
    
    /// <summary>
    /// 캐릭터 드롭다운을 설정합니다
    /// </summary>
    private void SetupCharacterDropdown()
    {
        if (characterDropdown != null)
        {
            characterDropdown.ClearOptions();
            List<string> characterOptions = new List<string>();
            
            foreach (var character in availableCharacters)
            {
                characterOptions.Add(character.displayName);
            }
            
            characterDropdown.AddOptions(characterOptions);
        }
    }
    
    /// <summary>
    /// 언어 드롭다운을 설정합니다
    /// </summary>
    private void SetupLanguageDropdown()
    {
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            List<string> languageOptions = new List<string>();
            
            foreach (var language in availableLanguages)
            {
                languageOptions.Add(language.displayName);
            }
            
            languageDropdown.AddOptions(languageOptions);
        }
    }
    
    /// <summary>
    /// UI의 초기 상태를 설정합니다
    /// </summary>
    private void SetupInitialState()
    {
        UpdateStatusText("준비됨", Color.white);
        
        if (generatedText != null)
            generatedText.text = "";
            
        if (currentSentenceText != null)
            currentSentenceText.text = "";
            
        if (sentenceCountText != null)
            sentenceCountText.text = "0 / 0";
            
        if (progressSlider != null)
            progressSlider.value = 0;
            
        if (stopButton != null)
            stopButton.interactable = false;
            
        if (recordStopButton != null)
            recordStopButton.interactable = false;
    }
    
    /// <summary>
    /// 이벤트 리스너들을 설정합니다
    /// </summary>
    private void SetupEventListeners()
    {
        SubscribeToTTSEvents();
        SetupButtonEvents();
    }
    
    /// <summary>
    /// TTS 클라이언트 이벤트를 구독합니다
    /// </summary>
    private void SubscribeToTTSEvents()
    {
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
    /// 버튼 이벤트들을 설정합니다
    /// </summary>
    private void SetupButtonEvents()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
            
        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopButtonClicked);
            
        if (recordStartButton != null)
            recordStartButton.onClick.AddListener(OnRecordStartButtonClicked);
            
        if (recordStopButton != null)
            recordStopButton.onClick.AddListener(OnRecordStopButtonClicked);
    }
    
    /// <summary>
    /// 참조들의 유효성을 검사합니다
    /// </summary>
    private void ValidateReferences()
    {
        if (ttsClient == null)
        {
            Debug.LogError("[TTSUIController] TTS 클라이언트가 설정되지 않았습니다!");
            UpdateStatusText("TTS 클라이언트 누락", Color.red);
        }
        
        if (inputTextField == null)
        {
            Debug.LogWarning("[TTSUIController] 입력 텍스트 필드가 설정되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// 기본값들을 설정합니다
    /// </summary>
    private void SetDefaultValues()
    {
        if (inputTextField != null)
            inputTextField.text = "안녕하세요! 실시간 음성 합성 테스트입니다.";
        
        // 기본 언어를 한국어로 설정
        SetLanguage(TTSConstants.Languages.Korean);
        
        // 기본 캐릭터를 친근한 도우미로 설정
        SetCharacter(TTSConstants.Characters.FriendlyAssistant);
    }
    
    #endregion
    
    #region Button Event Handlers
    
    /// <summary>
    /// 생성 버튼 클릭 이벤트 처리
    /// </summary>
    public void OnGenerateButtonClicked()
    {
        if (isGenerating || isRecording)
        {
            UpdateStatusText("이미 처리 중입니다", Color.yellow);
            return;
        }
            
        string inputText = inputTextField?.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(inputText))
        {
            UpdateStatusText("텍스트를 입력해주세요", Color.red);
            return;
        }
        
        // 선택된 캐릭터와 언어 가져오기
        string selectedCharacter = GetSelectedCharacter();
        string selectedLanguage = GetSelectedLanguage();
        
        StartGeneration(inputText, selectedCharacter, selectedLanguage);
    }
    
    /// <summary>
    /// 중지 버튼 클릭 이벤트 처리
    /// </summary>
    public void OnStopButtonClicked()
    {
        if (ttsClient != null)
        {
            ttsClient.CancelCurrentRequest();
            ttsClient.StopAllAudio();
        }
        
        ResetUIState();
        UpdateStatusText("중지됨", Color.yellow);
    }
    
    /// <summary>
    /// 녹음 시작 버튼 클릭 이벤트 처리
    /// </summary>
    public void OnRecordStartButtonClicked()
    {
        if (isRecording || isGenerating)
        {
            UpdateStatusText("이미 처리 중입니다", Color.yellow);
            return;
        }

        if (currentNpc == null) return;
        StartRecording();
    }
    
    /// <summary>
    /// 녹음 중지 버튼 클릭 이벤트 처리
    /// </summary>
    public void OnRecordStopButtonClicked()
    {
        if (!isRecording)
            return;
            
        StopRecording();
    }
    
    #endregion
    
    #region TTS Event Handlers
    
    /// <summary>
    /// 요청 시작 이벤트 처리
    /// </summary>
    private void OnRequestStarted()
    {
        isGenerating = true;
        UpdateGenerationUIState(true);
        UpdateStatusText("요청 시작됨", Color.blue);
        
        Debug.Log("[TTSUIController] TTS 요청이 시작되었습니다.");
    }
    
    /// <summary>
    /// 요청 완료 이벤트 처리
    /// </summary>
    private void OnRequestCompleted()
    {
        // 모든 처리가 완료되면 UI 상태 리셋
        // 단, OnAllCompleted에서 이미 처리했다면 중복 방지
        if (isGenerating && completedSentences == 0)
        {
            ResetUIState();
            UpdateStatusText("요청 완료", Color.green);
        }
        
        Debug.Log("[TTSUIController] TTS 요청이 완료되었습니다.");
    }
    
    /// <summary>
    /// 텍스트 생성 이벤트 처리
    /// </summary>
    /// <param name="text">생성된 텍스트</param>
    private void OnTextGenerated(string text)
    {
        // 생성된 텍스트 누적
        generatedTextBuilder.AppendLine(text);
        
        if (generatedText != null)
        {
            generatedText.text = generatedTextBuilder.ToString();
        }
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = $"생성: {text}";
        }
        
        Debug.Log($"[TTSUIController] 텍스트 생성: {text}");
    }
    
    /// <summary>
    /// 문장 완료 이벤트 처리
    /// </summary>
    /// <param name="sentenceId">완료된 문장 ID</param>
    /// <param name="text">완료된 문장 텍스트</param>
    private void OnSentenceCompleted(int sentenceId, string text)
    {
        completedSentences++;
        UpdateProgress();
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = $"재생 완료: 문장 {sentenceId}";
        }
        
        UpdateStatusText($"진행 중 ({completedSentences}/{totalSentences})", Color.green);
        
        Debug.Log($"[TTSUIController] 문장 {sentenceId} 완료: {text}");
    }
    
    /// <summary>
    /// 전체 완료 이벤트 처리
    /// </summary>
    /// <param name="totalSentenceCount">총 문장 수</param>
    private void OnAllCompleted(int totalSentenceCount)
    {
        totalSentences = totalSentenceCount;
        completedSentences = totalSentenceCount;
        UpdateProgress();
        
        UpdateStatusText($"완료! (총 {totalSentenceCount}개 문장)", Color.green);
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = "모든 문장 처리 완료";
        }
        
        ResetUIState();
        
        Debug.Log($"[TTSUIController] 모든 생성 완료: {totalSentenceCount}개 문장");
    }
    
    /// <summary>
    /// 오류 이벤트 처리
    /// </summary>
    /// <param name="errorMessage">오류 메시지</param>
    private void OnError(string errorMessage)
    {
        UpdateStatusText($"오류: {errorMessage}", Color.red);
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = "오류 발생";
        }
        
        ResetUIState();
        
        Debug.LogError($"[TTSUIController] TTS 오류: {errorMessage}");
    }
    
    #endregion
    
    #region Npc Handlers

    private void OnEnteredNpc(Npc npc)
    {
        if (currentNpc == null || npc.Info != currentNpc)
        {
            currentNpc = npc.Info;
        }
    }

    private void OnExitedNpc(Npc npc)
    {
        if (npc.Info == currentNpc)
        {
            currentNpc = null;
        }
    }
    
    #endregion
    
    #region Generation Control
    
    /// <summary>
    /// 텍스트 생성을 시작합니다
    /// </summary>
    /// <param name="text">생성할 텍스트</param>
    /// <param name="character">사용할 캐릭터</param>
    /// <param name="language">사용할 언어</param>
    private void StartGeneration(string text, string character, string language)
    {
        isGenerating = true;
        totalSentences = 0;
        completedSentences = 0;
        generatedTextBuilder.Clear();
        
        UpdateGenerationUIState(true);
        UpdateStatusText("생성 요청 중...", Color.blue);
        
        if (generatedText != null)
            generatedText.text = "";
            
        if (currentSentenceText != null)
            currentSentenceText.text = "요청 준비 중...";
        
        UpdateProgress();
        
        // 비동기 TTS 시작
        if (ttsClient != null)
        {
            ttsClient.StartRealtimeTTSAsync(text, character, language);
            Debug.Log($"[TTSUIController] TTS 시작 - 언어: {language}, 캐릭터: {character}");
        }
        else
        {
            OnError("TTS 클라이언트가 설정되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// UI 상태를 리셋합니다
    /// </summary>
    private void ResetUIState()
    {
        isGenerating = false;
        UpdateGenerationUIState(false);
        UpdateMicrophoneUIState(false);
    }
    
    /// <summary>
    /// 생성 관련 UI 상태를 업데이트합니다
    /// </summary>
    /// <param name="isGenerating">생성 중인지 여부</param>
    private void UpdateGenerationUIState(bool isGenerating)
    {
        if (generateButton != null)
            generateButton.interactable = !isGenerating;
            
        if (stopButton != null)
            stopButton.interactable = isGenerating;
            
        if (recordStartButton != null)
            recordStartButton.interactable = !isGenerating && !this.isRecording;
    }
    
    /// <summary>
    /// 마이크 관련 UI 상태를 업데이트합니다
    /// </summary>
    /// <param name="isRecording">녹음 중인지 여부</param>
    private void UpdateMicrophoneUIState(bool isRecording)
    {
        if (recordStartButton != null)
            recordStartButton.interactable = !isRecording && !isGenerating;
            
        if (recordStopButton != null)
            recordStopButton.interactable = isRecording;
            
        if (generateButton != null)
            generateButton.interactable = !isRecording && !isGenerating;
    }
    
    /// <summary>
    /// 진행률을 업데이트합니다
    /// </summary>
    private void UpdateProgress()
    {
        float progress = totalSentences > 0 ? (float)completedSentences / totalSentences : 0f;
        
        if (progressSlider != null)
            progressSlider.value = progress;
            
        if (sentenceCountText != null)
            sentenceCountText.text = $"{completedSentences} / {totalSentences}";
    }
    
    #endregion
    
    #region Recording Control
    
    /// <summary>
/// 마이크를 초기화합니다 (최적화됨)
/// </summary>
private void InitializeMicrophone()
{
    if (Microphone.devices.Length > 0)
    {
#if UNITY_EDITOR
        foreach (var microphone in Microphone.devices)
        {
            if (microphone.Contains("Oculus") || microphone.Contains("Quest"))
            {
                microphoneDevice = microphone;
                break;
            }
        }
#else
        microphoneDevice = Microphone.devices[0];
#endif
        
        UpdateStatusText($"마이크 준비됨: {microphoneDevice}", Color.white);
        Debug.Log($"[TTSUIController] 마이크 장치 초기화: {microphoneDevice}");
        
        // 마이크 미리 초기화 시작 (비동기)
        StartCoroutine(PreWarmMicrophoneCoroutine());
    }
    else
    {
        UpdateStatusText("마이크 장치를 찾을 수 없습니다", Color.red);
        
        if (recordStartButton != null)
            recordStartButton.interactable = false;
            
        Debug.LogWarning("[TTSUIController] 마이크 장치를 찾을 수 없습니다.");
    }
}

/// <summary>
/// 마이크를 미리 초기화합니다 (렉 방지용)
/// </summary>
private IEnumerator PreWarmMicrophoneCoroutine()
{
    if (isMicrophonePreWarmed || string.IsNullOrEmpty(microphoneDevice))
        yield break;
        
    isMicrophoneInitializing = true;
    microphoneError = "";
    
    // 여러 프레임에 걸쳐 처리하여 렉 방지
    yield return new WaitForEndOfFrame();
    
    Debug.Log("[TTSUIController] 마이크 미리 초기화 시작...");
    
    // try-catch 블록에서 마이크 초기화 수행 (yield 없음)
    AudioClip preWarmClip = null;
    bool initSuccess = false;
    
    try
    {
        preWarmClip = Microphone.Start(microphoneDevice, false, preWarmDuration, optimizedSampleRate);
        initSuccess = true;
    }
    catch (System.Exception e)
    {
        microphoneError = e.Message;
        initSuccess = false;
    }
    
    // 결과에 따른 처리 (yield 사용 가능)
    if (initSuccess && preWarmClip != null)
    {
        // 몇 프레임 대기
        yield return new WaitForSeconds(0.1f);
        
        // 마이크 중지
        try
        {
            Microphone.End(microphoneDevice);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[TTSUIController] 마이크 중지 중 경고: {e.Message}");
        }
        
        // 메모리 정리
        if (preWarmClip != null)
        {
            DestroyImmediate(preWarmClip);
        }
        
        isMicrophonePreWarmed = true;
        microphonePermissionChecked = true;
        
        Debug.Log("[TTSUIController] 마이크 미리 초기화 완료");
        UpdateStatusText("마이크 최적화 완료", Color.green);
    }
    else
    {
        Debug.LogError($"[TTSUIController] 마이크 미리 초기화 실패: {microphoneError}");
        UpdateStatusText("마이크 초기화 실패", Color.red);
        
        if (recordStartButton != null)
            recordStartButton.interactable = false;
    }
    
    isMicrophoneInitializing = false;
}

/// <summary>
/// 녹음을 시작합니다 (최적화됨)
/// </summary>
private void StartRecording()
{
    if (string.IsNullOrEmpty(microphoneDevice))
    {
        UpdateStatusText("마이크 장치가 없습니다", Color.red);
        return;
    }
    
    // 이미 초기화 중이거나 녹음 중이면 대기
    if (isMicrophoneInitializing)
    {
        UpdateStatusText("마이크 초기화 중...", Color.yellow);
        StartCoroutine(WaitForMicrophoneAndStartCoroutine());
        return;
    }
    
    // 비동기로 녹음 시작하여 렉 방지
    StartCoroutine(StartRecordingCoroutine());
    OnStartRecording?.Invoke();
}

/// <summary>
/// 마이크 초기화 완료 대기 후 녹음 시작
/// </summary>
private IEnumerator WaitForMicrophoneAndStartCoroutine()
{
    // 마이크 초기화 완료까지 대기
    while (isMicrophoneInitializing)
    {
        yield return new WaitForSeconds(0.1f);
    }
    
    // 초기화 완료 후 녹음 시작
    yield return StartCoroutine(StartRecordingCoroutine());
}

/// <summary>
/// 비동기로 녹음을 시작합니다 (렉 방지)
/// </summary>
private IEnumerator StartRecordingCoroutine()
{
    if (isRecording)
        yield break;
        
    // UI 상태 먼저 업데이트 (즉시 반응성 제공)
    isRecording = true;
    UpdateMicrophoneUIState(true);
    UpdateStatusText("녹음 준비 중...", Color.yellow);
    
    if (currentSentenceText != null)
    {
        currentSentenceText.text = "마이크 시작 중...";
    }
    
    // 한 프레임 대기하여 UI 업데이트 반영
    yield return new WaitForEndOfFrame();
    
    // 마이크가 미리 초기화되지 않았다면 빠른 초기화
    if (!isMicrophonePreWarmed)
    {
        yield return StartCoroutine(QuickMicrophoneInitCoroutine());
    }
    
    // 실제 녹음 시작
    yield return StartCoroutine(StartActualRecordingCoroutine());
}

/// <summary>
/// 빠른 마이크 초기화
/// </summary>
private IEnumerator QuickMicrophoneInitCoroutine()
{
    Debug.Log("[TTSUIController] 마이크 즉시 초기화 중...");
    
    AudioClip dummyClip = null;
    bool quickInitSuccess = false;
    
    // try-catch 블록에서 더미 녹음 시작 (yield 없음)
    try
    {
        dummyClip = Microphone.Start(microphoneDevice, false, 1, optimizedSampleRate);
        quickInitSuccess = true;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[TTSUIController] 빠른 초기화 실패: {e.Message}");
        quickInitSuccess = false;
    }
    
    if (quickInitSuccess)
    {
        yield return new WaitForSeconds(0.05f); // 50ms만 대기
        
        // 더미 녹음 중지
        try
        {
            Microphone.End(microphoneDevice);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[TTSUIController] 더미 녹음 중지 중 경고: {e.Message}");
        }
        
        if (dummyClip != null)
            DestroyImmediate(dummyClip);
            
        yield return new WaitForSeconds(0.1f); // 추가 안정화 대기
    }
    else
    {
        // 빠른 초기화 실패 시에도 계속 진행 (메인 녹음에서 재시도)
        yield return new WaitForSeconds(0.1f);
    }
}

/// <summary>
/// 실제 녹음 시작
/// </summary>
private IEnumerator StartActualRecordingCoroutine()
{
    UpdateStatusText("녹음 시작 중...", Color.orange);
    yield return new WaitForEndOfFrame();
    
    recordingStartError = "";
    bool recordingSuccess = false;
    
    // try-catch 블록에서 실제 녹음 시작 (yield 없음)
    try
    {
        recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingTime, recordingSampleRate);
        recordingSuccess = (recordedClip != null);
        
        if (!recordingSuccess)
        {
            recordingStartError = "AudioClip 생성 실패";
        }
    }
    catch (System.Exception e)
    {
        recordingStartError = e.Message;
        recordingSuccess = false;
    }
    
    // 결과에 따른 처리 (yield 사용 가능)
    if (recordingSuccess)
    {
        // 녹음 상태 업데이트
        UpdateStatusText("녹음 중...", Color.red);
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = "음성을 녹음하고 있습니다";
        }
        
        // 최대 녹음 시간 후 자동 종료
        StartCoroutine(AutoStopRecording());
        
        Debug.Log("[TTSUIController] 음성 녹음 시작 완료");
    }
    else
    {
        Debug.LogError($"[TTSUIController] 녹음 시작 실패: {recordingStartError}");
        
        // 오류 발생 시 상태 복구
        isRecording = false;
        UpdateMicrophoneUIState(false);
        UpdateStatusText($"녹음 시작 실패: {recordingStartError}", Color.red);
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = "녹음 시작 오류";
        }
    }
}

/// <summary>
/// 녹음을 중지합니다 (안전한 버전)
/// </summary>
private void StopRecording()
{
    if (!isRecording)
        return;
        
    StartCoroutine(StopRecordingCoroutine());
    OnStopRecording?.Invoke();
}

/// <summary>
/// 안전한 녹음 중지 코루틴
/// </summary>
private IEnumerator StopRecordingCoroutine()
{
    isRecording = false;
    UpdateMicrophoneUIState(false);
    UpdateStatusText("녹음 중지 중...", Color.yellow);
    
    yield return new WaitForEndOfFrame();
    
    // try-catch 블록에서 마이크 중지 (yield 없음)
    bool stopSuccess = false;
    string stopError = "";
    
    try
    {
        Microphone.End(microphoneDevice);
        stopSuccess = true;
    }
    catch (System.Exception e)
    {
        stopError = e.Message;
        stopSuccess = false;
    }
    
    // 결과 처리 (yield 사용 가능)
    if (stopSuccess)
    {
        UpdateStatusText("녹음 완료, 처리 중...", Color.blue);
        
        if (currentSentenceText != null)
        {
            currentSentenceText.text = "녹음된 음성을 서버로 전송 중";
        }
        
        Debug.Log("[TTSUIController] 음성 녹음 완료");
        
        // 녹음된 오디오를 서버로 전송
        yield return StartCoroutine(ProcessRecordedAudio());
    }
    else
    {
        Debug.LogError($"[TTSUIController] 녹음 중지 실패: {stopError}");
        UpdateStatusText($"녹음 중지 오류: {stopError}", Color.red);
        
        // 상태 리셋
        ResetUIState();
    }
}

    
    /// <summary>
    /// 자동 녹음 중지 코루틴
    /// </summary>
    private IEnumerator AutoStopRecording()
    {
        yield return new WaitForSeconds(maxRecordingTime);
        
        if (isRecording)
        {
            Debug.Log("[TTSUIController] 최대 녹음 시간 도달, 자동 중지");
            StopRecording();
        }
    }
    
    /// <summary>
    /// 녹음된 오디오를 처리합니다
    /// </summary>
    private IEnumerator ProcessRecordedAudio()
    {
        if (recordedClip == null)
        {
            UpdateStatusText("녹음된 오디오가 없습니다", Color.red);
            ResetUIState();
            yield break;
        }
        
        // 녹음된 오디오를 WAV 바이트로 변환
        byte[] wavData = AudioClipToWAV(recordedClip);
        
        if (wavData != null && wavData.Length > 0)
        {
            // 선택된 캐릭터와 언어 가져오기
            string selectedCharacter = currentNpc.Character;
            string selectedLanguage = currentNpc.Language;
            
            // TTS 클라이언트로 음성 데이터 전송
            if (ttsClient != null)
            {
                StartVoiceGeneration(wavData, selectedCharacter, selectedLanguage);
            }
            else
            {
                OnError("TTS 클라이언트가 설정되지 않았습니다.");
            }
        }
        else
        {
            UpdateStatusText("오디오 변환 실패", Color.red);
            ResetUIState();
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 음성 생성을 시작합니다
    /// </summary>
    /// <param name="audioData">오디오 데이터</param>
    /// <param name="character">사용할 캐릭터</param>
    /// <param name="language">사용할 언어</param>
    private void StartVoiceGeneration(byte[] audioData, string character, string language)
    {
        isGenerating = true;
        totalSentences = 0;
        completedSentences = 0;
        generatedTextBuilder.Clear();
        
        UpdateGenerationUIState(true);
        UpdateStatusText("음성 생성 중...", Color.blue);
        
        if (generatedText != null)
            generatedText.text = "";
            
        if (currentSentenceText != null)
            currentSentenceText.text = "음성 데이터 처리 중...";
        
        UpdateProgress();
        
        // 비동기 TTS 시작 (음성 데이터 전송)
        ttsClient.StartRealtimeTTSWithAudioAsync(audioData, character, language);
        
        Debug.Log($"[TTSUIController] 음성 TTS 시작 - 언어: {language}, 캐릭터: {character}, 데이터 크기: {audioData.Length} bytes");
    }
    
    #endregion
    
    #region Audio Processing
    
    /// <summary>
    /// AudioClip을 WAV 바이트 배열로 변환합니다
    /// </summary>
    /// <param name="clip">변환할 AudioClip</param>
    /// <returns>WAV 바이트 배열</returns>
    private byte[] AudioClipToWAV(AudioClip clip)
    {
        try
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            // WAV 헤더 생성
            int sampleRate = clip.frequency;
            int channels = clip.channels;
            int bitDepth = 16;
            
            using (MemoryStream stream = new MemoryStream())
            {
                // WAV 헤더 작성
                WriteWAVHeader(stream, samples.Length, sampleRate, channels, bitDepth);
                
                // 오디오 데이터 작성 (16비트 PCM)
                foreach (float sample in samples)
                {
                    short intSample = (short)(sample * short.MaxValue);
                    byte[] bytes = BitConverter.GetBytes(intSample);
                    stream.Write(bytes, 0, 2);
                }
                
                return stream.ToArray();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TTSUIController] AudioClip to WAV 변환 오류: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// WAV 헤더를 작성합니다
    /// </summary>
    /// <param name="stream">출력 스트림</param>
    /// <param name="sampleCount">샘플 수</param>
    /// <param name="sampleRate">샘플링 레이트</param>
    /// <param name="channels">채널 수</param>
    /// <param name="bitDepth">비트 깊이</param>
    private void WriteWAVHeader(MemoryStream stream, int sampleCount, int sampleRate, int channels, int bitDepth)
    {
        int byteRate = sampleRate * channels * bitDepth / 8;
        int blockAlign = channels * bitDepth / 8;
        int dataSize = sampleCount * blockAlign;
        int fileSize = 36 + dataSize;
        
        // RIFF 헤더
        stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(fileSize), 0, 4);
        stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        
        // fmt 청크
        stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // 청크 크기
        stream.Write(BitConverter.GetBytes((short)1), 0, 2); // PCM 포맷
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((short)blockAlign), 0, 2);
        stream.Write(BitConverter.GetBytes((short)bitDepth), 0, 2);
        
        // data 청크
        stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(dataSize), 0, 4);
    }
    
    #endregion
    
    #region Public API Methods
    
    /// <summary>
    /// 입력 텍스트를 설정합니다
    /// </summary>
    /// <param name="text">설정할 텍스트</param>
    public void SetInputText(string text)
    {
        if (inputTextField != null)
            inputTextField.text = text;
    }
    
    /// <summary>
    /// 캐릭터를 설정합니다
    /// </summary>
    /// <param name="characterCode">캐릭터 코드</param>
    public void SetCharacter(string characterCode)
    {
        for (int i = 0; i < availableCharacters.Length; i++)
        {
            if (availableCharacters[i].code == characterCode)
            {
                if (characterDropdown != null)
                    characterDropdown.value = i;
                break;
            }
        }
    }
    
    /// <summary>
    /// 언어를 설정합니다
    /// </summary>
    /// <param name="languageCode">언어 코드</param>
    public void SetLanguage(string languageCode)
    {
        for (int i = 0; i < availableLanguages.Length; i++)
        {
            if (availableLanguages[i].code == languageCode)
            {
                if (languageDropdown != null)
                    languageDropdown.value = i;
                break;
            }
        }
    }
    
    /// <summary>
    /// 현재 생성 중인지 확인합니다
    /// </summary>
    /// <returns>생성 중이면 true</returns>
    public bool IsGenerating()
    {
        return isGenerating;
    }
    
    /// <summary>
    /// 현재 녹음 중인지 확인합니다
    /// </summary>
    /// <returns>녹음 중이면 true</returns>
    public bool IsRecording()
    {
        return isRecording;
    }
    
    /// <summary>
    /// TTS 클라이언트를 설정합니다
    /// </summary>
    /// <param name="client">TTS 클라이언트</param>
    public void SetTTSClient(UnityRealtimeTTSClient client)
    {
        // 기존 이벤트 구독 해제
        UnsubscribeEvents();
        
        ttsClient = client;
        
        // 새 이벤트 구독
        SubscribeToTTSEvents();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// 선택된 캐릭터 코드를 가져옵니다
    /// </summary>
    /// <returns>캐릭터 코드</returns>
    private string GetSelectedCharacter()
    {
        if (characterDropdown != null && characterDropdown.value < availableCharacters.Length)
        {
            return availableCharacters[characterDropdown.value].code;
        }
        return TTSConstants.Characters.FriendlyAssistant; // 기본값
    }
    
    /// <summary>
    /// 선택된 언어 코드를 가져옵니다
    /// </summary>
    /// <returns>언어 코드</returns>
    private string GetSelectedLanguage()
    {
        if (languageDropdown != null && languageDropdown.value < availableLanguages.Length)
        {
            return availableLanguages[languageDropdown.value].code;
        }
        return TTSConstants.Languages.English; // 기본값
    }
    
    /// <summary>
    /// 상태 텍스트를 업데이트합니다
    /// </summary>
    /// <param name="message">메시지</param>
    /// <param name="color">텍스트 색상</param>
    private void UpdateStatusText(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }
    
    /// <summary>
    /// 리소스를 정리합니다
    /// </summary>
    private void CleanupResources()
    {
        // 녹음 중지
        if (isRecording)
        {
            Microphone.End(microphoneDevice);
        }
        
        // 생성된 텍스트 빌더 정리
        generatedTextBuilder?.Clear();
    }
    
    /// <summary>
    /// 이벤트 구독을 해제합니다
    /// </summary>
    private void UnsubscribeEvents()
    {
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
}

#region Supporting Classes

/// <summary>
/// 캐릭터 옵션 데이터 클래스
/// </summary>
[System.Serializable]
public class CharacterOption
{
    [Tooltip("UI에 표시될 이름")]
    public string displayName;
    
    [Tooltip("TTS 시스템에서 사용할 코드")]
    public string code;
    
    public CharacterOption(string displayName, string code)
    {
        this.displayName = displayName;
        this.code = code;
    }
}

/// <summary>
/// 언어 옵션 데이터 클래스
/// </summary>
[System.Serializable]
public class LanguageOption
{
    [Tooltip("UI에 표시될 이름")]
    public string displayName;
    
    [Tooltip("TTS 시스템에서 사용할 코드")]
    public string code;
    
    public LanguageOption(string displayName, string code)
    {
        this.displayName = displayName;
        this.code = code;
    }
}

#endregion 