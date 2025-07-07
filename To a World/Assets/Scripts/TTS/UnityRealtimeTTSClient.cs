using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;
using TTSSystem;

namespace TTSSystem
{
    /// <summary>
    /// Unity 실시간 TTS (Text-to-Speech) 클라이언트
    /// 
    /// 주요 기능:
    /// 1. 실시간 텍스트 음성 변환 (스트리밍)
    /// 2. 음성 입력을 통한 대화형 TTS
    /// 3. UniTask 기반 비동기 처리로 게임 성능 최적화
    /// 4. 다중 오디오 소스를 통한 순차 재생
    /// 5. 요청 취소 및 상태 관리
    /// 
    /// 사용 방법:
    /// ```csharp
    /// // 1. 컴포넌트 설정
    /// var ttsClient = GetComponent<UnityRealtimeTTSClient>();
    /// ttsClient.serverUrl = "http://your-server:8000";
    /// ttsClient.defaultLanguage = "ko";
    /// ttsClient.defaultCharacter = "friendly_assistant";
    /// 
    /// // 2. 이벤트 구독
    /// ttsClient.OnTextGenerated += (text) => Debug.Log($"생성된 텍스트: {text}");
    /// ttsClient.OnRequestStarted += () => Debug.Log("TTS 요청 시작");
    /// ttsClient.OnRequestCompleted += () => Debug.Log("TTS 요청 완료");
    /// 
    /// // 3. TTS 요청
    /// ttsClient.StartRealtimeTTSAsync("안녕하세요", "friendly_assistant", "ko");
    /// ```
    /// 
    /// 주의사항:
    /// - 서버 URL이 올바르게 설정되어야 합니다
    /// - 인터넷 연결이 필요합니다
    /// - 오디오 재생을 위한 AudioSource가 필요합니다
    /// </summary>
    public class UnityRealtimeTTSClient : MonoBehaviour
    {
        #region Inspector Fields
        
        [Header("서버 설정")]
        [SerializeField]
        [Tooltip("TTS 서버의 URL 주소 (예: http://localhost:8000)")]
        public string serverUrl = "http://localhost:8000";
        
        [Header("TTS 설정")]
        [SerializeField]
        [Tooltip("기본 사용 언어 (ko: 한국어, en: 영어, ja: 일본어 등)")]
        public string defaultLanguage = "en";
        
        [SerializeField]
        [Tooltip("기본 캐릭터 이름 (음성 스타일 결정)")]
        public string defaultCharacter = "friendly_assistant";
        
        [Header("오디오 설정")]
        [SerializeField]
        [Tooltip("오디오 재생에 사용할 AudioSource (선택사항)")]
        public AudioSource audioSource;
        
        [SerializeField]
        [Tooltip("동시 재생 가능한 오디오 수 (성능과 품질의 균형)")]
        [Range(1, 10)]
        public int maxConcurrentAudio = 3;
        
        [Header("디버그")]
        [SerializeField]
        [Tooltip("디버그 로그 출력 여부")]
        public bool enableDebugLogs = true;
        
        [Header("비동기 처리 설정")]
        [SerializeField]
        [Tooltip("비동기 처리 프레임 간격 (낮을수록 빠르지만 CPU 사용량 증가)")]
        [Range(1, 10)]
        public int asyncProcessingFrameInterval = 1;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// 오디오 재생 대기열
        /// 즉시 재생할 수 없는 오디오들을 저장
        /// </summary>
        private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
        
        /// <summary>
        /// 다중 오디오 재생을 위한 AudioSource 리스트
        /// 여러 문장을 동시에 또는 순차적으로 재생하기 위해 사용
        /// </summary>
        private List<AudioSource> audioSources = new List<AudioSource>();
        
        /// <summary>
        /// 현재 사용 중인 AudioSource 인덱스
        /// 라운드 로빈 방식으로 AudioSource 선택
        /// </summary>
        private int currentAudioIndex = 0;
        
        /// <summary>
        /// 문장별 오디오 버퍼
        /// 문장 ID를 키로 하여 순차 재생을 위해 오디오를 저장
        /// </summary>
        private Dictionary<int, AudioClip> audioBuffers = new Dictionary<int, AudioClip>();
        
        /// <summary>
        /// 다음에 재생할 문장 ID
        /// 순차 재생을 위한 카운터
        /// </summary>
        private int nextSentenceToPlay = 1;
        
        /// <summary>
        /// 현재 순차 재생 중인지 여부
        /// 중복 재생 방지를 위한 플래그
        /// </summary>
        private bool isPlayingSequentially = false;
        
        /// <summary>
        /// 비동기 작업 취소를 위한 토큰 소스
        /// 새로운 요청이 들어올 때 기존 요청을 취소하기 위해 사용
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;
        
        /// <summary>
        /// 현재 요청 처리 중인지 여부
        /// 중복 요청 방지 및 상태 관리
        /// </summary>
        private bool isProcessingRequest = false;
        
        /// <summary>
        /// 백그라운드 작업 대기열
        /// 메인 스레드에서 처리할 작업들을 저장
        /// </summary>
        private Queue<UniTask> backgroundTasks = new Queue<UniTask>();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 텍스트가 생성되었을 때 발생하는 이벤트
        /// </summary>
        /// <param name="text">생성된 텍스트</param>
        public event System.Action<string> OnTextGenerated;
        
        /// <summary>
        /// 문장 재생이 완료되었을 때 발생하는 이벤트
        /// </summary>
        /// <param name="sentenceId">완료된 문장 ID</param>
        /// <param name="text">완료된 문장 텍스트</param>
        public event System.Action<int, string> OnSentenceCompleted;
        
        /// <summary>
        /// 전체 TTS 처리가 완료되었을 때 발생하는 이벤트
        /// </summary>
        /// <param name="totalSentences">총 처리된 문장 수</param>
        public event System.Action<int> OnAllCompleted;
        
        /// <summary>
        /// 오류가 발생했을 때 발생하는 이벤트
        /// </summary>
        /// <param name="error">오류 메시지</param>
        public event System.Action<string> OnError;
        
        /// <summary>
        /// 요청이 시작되었을 때 발생하는 이벤트
        /// UI 업데이트나 로딩 표시에 사용
        /// </summary>
        public event System.Action OnRequestStarted;
        
        /// <summary>
        /// 요청이 완료되었을 때 발생하는 이벤트
        /// UI 업데이트나 로딩 해제에 사용
        /// </summary>
        public event System.Action OnRequestCompleted;
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Unity Start 메서드
        /// 컴포넌트 초기화 및 필요한 설정 수행
        /// </summary>
        void Start()
        {
            InitializeAudioSources();
            cancellationTokenSource = new CancellationTokenSource();
            StartCoroutine(ProcessBackgroundTasks());
        }
        
        /// <summary>
        /// Unity OnDestroy 메서드
        /// 리소스 정리 및 작업 취소
        /// </summary>
        void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
        
        /// <summary>
        /// Unity Update 메서드
        /// 오디오 재생 대기열 처리
        /// </summary>
        private void Update()
        {
            // 큐에 있는 오디오 처리
            if (audioQueue.Count > 0)
            {
                AudioSource availableSource = GetAvailableAudioSource();
                if (availableSource != null && !availableSource.isPlaying)
                {
                    AudioClip clipToPlay = audioQueue.Dequeue();
                    availableSource.clip = clipToPlay;
                    availableSource.Play();
                    DebugLog($"큐에서 오디오 재생: {clipToPlay.name}");
                }
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// 오디오 소스들을 초기화합니다
        /// 다중 오디오 재생을 위해 여러 AudioSource를 미리 생성
        /// </summary>
        private void InitializeAudioSources()
        {
            // 기존 AudioSource가 있으면 리스트에 추가
            if (audioSource != null)
            {
                audioSources.Add(audioSource);
            }
            
            // 부족한 만큼 추가 AudioSource 생성
            for (int i = audioSources.Count; i < maxConcurrentAudio; i++)
            {
                GameObject audioObj = new GameObject($"RealtimeAudio_{i}");
                audioObj.transform.SetParent(transform);
                
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.volume = 1.0f;
                
                audioSources.Add(source);
            }
            
            DebugLog($"{audioSources.Count}개의 AudioSource 초기화 완료");
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// 실시간 TTS 요청을 시작합니다 (호환성 유지용)
        /// 내부적으로 비동기 버전을 호출합니다
        /// </summary>
        /// <param name="text">음성으로 변환할 텍스트</param>
        /// <param name="characterName">사용할 캐릭터 이름 (선택사항)</param>
        /// <param name="language">사용할 언어 (선택사항)</param>
        public void StartRealtimeTTS(string text, string characterName = null, string language = null)
        {
            // 비동기 버전 사용
            StartRealtimeTTSAsync(text, characterName, language);
        }
        
        /// <summary>
        /// 음성 입력을 통한 실시간 TTS 요청을 시작합니다 (호환성 유지용)
        /// 내부적으로 비동기 버전을 호출합니다
        /// </summary>
        /// <param name="audioData">입력 음성 데이터 (WAV 형식)</param>
        /// <param name="characterName">사용할 캐릭터 이름 (선택사항)</param>
        /// <param name="language">사용할 언어 (선택사항)</param>
        public void StartRealtimeTTSWithAudio(byte[] audioData, string characterName = null, string language = null)
        {
            // 비동기 버전 사용
            StartRealtimeTTSWithAudioAsync(audioData, characterName, language);
        }
        
        /// <summary>
        /// 비동기 방식으로 실시간 TTS 요청을 시작합니다
        /// 게임 성능에 미치는 영향을 최소화하며 백그라운드에서 처리
        /// </summary>
        /// <param name="text">음성으로 변환할 텍스트</param>
        /// <param name="characterName">사용할 캐릭터 이름 (선택사항)</param>
        /// <param name="language">사용할 언어 (선택사항)</param>
        public void StartRealtimeTTSAsync(string text, string characterName = null, string language = null)
        {
            // 유효성 검사
            if (string.IsNullOrEmpty(text))
            {
                DebugLog("텍스트가 비어있습니다.");
                OnError?.Invoke("텍스트가 비어있습니다.");
                return;
            }
            
            // 이미 처리 중이면 기존 요청 취소
            if (isProcessingRequest)
            {
                DebugLog("이미 요청을 처리 중입니다. 현재 요청을 취소하고 새 요청을 시작합니다.");
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
            }
            
            // 이전 상태 초기화
            audioBuffers.Clear();
            nextSentenceToPlay = 1;
            isPlayingSequentially = false;
            StopAllAudio();
            
            // 비동기 처리 시작
            ProcessRealtimeTTSAsync(text, characterName, language, cancellationTokenSource.Token).Forget();
        }
        
        /// <summary>
        /// 비동기 방식으로 음성 입력을 통한 실시간 TTS 요청을 시작합니다
        /// 음성 인식 + TTS 처리를 연계하여 대화형 경험 제공
        /// </summary>
        /// <param name="audioData">입력 음성 데이터 (WAV 형식)</param>
        /// <param name="characterName">사용할 캐릭터 이름 (선택사항)</param>
        /// <param name="language">사용할 언어 (선택사항)</param>
        public void StartRealtimeTTSWithAudioAsync(byte[] audioData, string characterName = null, string language = null)
        {
            // 유효성 검사
            if (audioData == null || audioData.Length == 0)
            {
                DebugLog("오디오 데이터가 비어있습니다.");
                OnError?.Invoke("오디오 데이터가 비어있습니다.");
                return;
            }
            
            // 이미 처리 중이면 기존 요청 취소
            if (isProcessingRequest)
            {
                DebugLog("이미 요청을 처리 중입니다. 현재 요청을 취소하고 새 요청을 시작합니다.");
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
            }
            
            // 이전 상태 초기화
            audioBuffers.Clear();
            nextSentenceToPlay = 1;
            isPlayingSequentially = false;
            StopAllAudio();
            
            // 비동기 처리 시작
            ProcessRealtimeTTSWithAudioAsync(audioData, characterName, language, cancellationTokenSource.Token).Forget();
        }
        
        /// <summary>
        /// 현재 요청 처리 상태를 확인합니다
        /// </summary>
        /// <returns>처리 중이면 true, 아니면 false</returns>
        public bool IsProcessingRequest()
        {
            return isProcessingRequest;
        }
        
        /// <summary>
        /// 현재 진행 중인 요청을 취소합니다
        /// </summary>
        public void CancelCurrentRequest()
        {
            if (isProcessingRequest)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                DebugLog("현재 요청을 취소했습니다.");
            }
        }
        
        /// <summary>
        /// 모든 오디오 재생을 중지합니다
        /// </summary>
        public void StopAllAudio()
        {
            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    source.Stop();
                    source.clip = null;
                }
            }
            audioQueue.Clear();
            audioBuffers.Clear();
            DebugLog("모든 오디오 중지됨");
        }
        
        /// <summary>
        /// 현재 오디오가 재생 중인지 확인합니다
        /// </summary>
        /// <returns>재생 중이면 true, 아니면 false</returns>
        public bool IsPlaying()
        {
            foreach (AudioSource source in audioSources)
            {
                if (source != null && source.isPlaying)
                    return true;
            }
            return audioQueue.Count > 0;
        }
        
        /// <summary>
        /// 서버 URL을 설정합니다
        /// </summary>
        /// <param name="newUrl">새로운 서버 URL</param>
        public void SetServerUrl(string newUrl)
        {
            serverUrl = newUrl;
            DebugLog($"서버 URL 변경: {newUrl}");
        }
        
        /// <summary>
        /// 기본 캐릭터를 설정합니다
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        public void SetDefaultCharacter(string characterName)
        {
            defaultCharacter = characterName;
            DebugLog($"기본 캐릭터 변경: {characterName}");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 사용 가능한 AudioSource를 찾습니다.
        /// 재생 중이지 않은 AudioSource를 먼저 선택하고, 없으면 라운드 로빈으로 선택합니다.
        /// </summary>
        /// <returns>사용 가능한 AudioSource 또는 null</returns>
        private AudioSource GetAvailableAudioSource()
        {
            // 재생 중이지 않은 AudioSource 찾기
            foreach (AudioSource source in audioSources)
            {
                if (!source.isPlaying)
                    return source;
            }
            
            // 모두 사용 중이면 가장 오래된 것 사용 (라운드 로빈)
            AudioSource selectedSource = audioSources[currentAudioIndex];
            currentAudioIndex = (currentAudioIndex + 1) % audioSources.Count;
            return selectedSource;
        }
        
        /// <summary>
        /// WAV 데이터를 파싱하여 AudioClipData로 변환합니다 (메인 스레드 호출 불필요)
        /// </summary>
        /// <param name="wavData">WAV 데이터</param>
        /// <param name="clipName">AudioClip 이름</param>
        /// <returns>파싱된 AudioClipData 또는 null</returns>
        private AudioClipData ParseWAVData(byte[] wavData, string clipName)
        {
            try
            {
                // WAV 헤더 파싱
                int sampleRate = BitConverter.ToInt32(wavData, 24);
                int channels = BitConverter.ToInt16(wavData, 22);
                int dataStart = 44; // 표준 WAV 헤더 크기
                
                // 오디오 데이터 추출 (16비트 PCM 가정)
                int sampleCount = (wavData.Length - dataStart) / 2 / channels;
                float[] samples = new float[sampleCount * channels];
                
                for (int i = 0; i < sampleCount * channels; i++)
                {
                    int byteIndex = dataStart + i * 2;
                    short sample = BitConverter.ToInt16(wavData, byteIndex);
                    samples[i] = sample / 32768.0f; // 16비트를 float으로 정규화
                }
                
                return new AudioClipData
                {
                    clipName = clipName,
                    samples = samples,
                    sampleCount = sampleCount,
                    channels = channels,
                    sampleRate = sampleRate
                };
            }
            catch (Exception e)
            {
                DebugLog($"WAV 파싱 오류: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// AudioClipData에서 AudioClip을 생성합니다 (메인 스레드에서만 호출)
        /// </summary>
        /// <param name="clipData">파싱된 오디오 데이터</param>
        /// <returns>생성된 AudioClip 또는 null</returns>
        private AudioClip CreateAudioClipFromData(AudioClipData clipData)
        {
            try
            {
                // AudioClip 생성 (메인 스레드에서만 가능)
                AudioClip audioClip = AudioClip.Create(clipData.clipName, clipData.sampleCount, clipData.channels, clipData.sampleRate, false);
                audioClip.SetData(clipData.samples, 0);
                
                return audioClip;
            }
            catch (Exception e)
            {
                DebugLog($"AudioClip 생성 오류: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// WAV 데이터를 AudioClip으로 변환합니다 (호환성용 - 직접 사용 권장하지 않음)
        /// </summary>
        /// <param name="wavData">WAV 데이터</param>
        /// <param name="clipName">AudioClip 이름</param>
        /// <returns>변환된 AudioClip 또는 null</returns>
        private AudioClip WAVToAudioClip(byte[] wavData, string clipName)
        {
            // 메인 스레드에서만 사용 가능 - 새로운 방식 사용 권장
            AudioClipData clipData = ParseWAVData(wavData, clipName);
            return clipData != null ? CreateAudioClipFromData(clipData) : null;
        }
        
        /// <summary>
        /// 디버그 로그를 출력합니다
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[RealtimeTTS] {message}");
            }
        }
        
        #endregion
        
        #region Async Processing Methods
        
        /// <summary>
        /// 비동기 방식으로 실시간 TTS 요청을 처리합니다
        /// </summary>
        /// <param name="text">처리할 텍스트</param>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="language">언어</param>
        /// <param name="cancellationToken">취소 토큰</param>
        private async UniTask ProcessRealtimeTTSAsync(string text, string characterName, string language, CancellationToken cancellationToken)
        {
            try
            {
                isProcessingRequest = true;
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnRequestStarted?.Invoke());
                
                string url = $"{serverUrl}/generate_speech_realtime";
                
                // 요청 데이터 구성
                TTSRealtimeRequest request = new TTSRealtimeRequest
                {
                    text = text,
                    language = language ?? defaultLanguage,
                    character_name = characterName ?? defaultCharacter,
                    use_thinking = true
                };
                
                string jsonData = JsonConvert.SerializeObject(request);
                DebugLog($"실시간 TTS 요청: {text.Substring(0, Mathf.Min(50, text.Length))}...");
                
                await ProcessStreamingRequestAsync(url, jsonData, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                DebugLog("TTS 요청이 취소되었습니다.");
            }
            catch (Exception e)
            {
                DebugLog($"TTS 처리 오류: {e.Message}");
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnError?.Invoke(e.Message));
            }
            finally
            {
                isProcessingRequest = false;
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnRequestCompleted?.Invoke());
            }
        }
        
        /// <summary>
        /// 비동기 방식으로 음성 입력을 통한 실시간 TTS 요청을 처리합니다
        /// </summary>
        /// <param name="audioData">오디오 데이터</param>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="language">언어</param>
        /// <param name="cancellationToken">취소 토큰</param>
        private async UniTask ProcessRealtimeTTSWithAudioAsync(byte[] audioData, string characterName, string language, CancellationToken cancellationToken)
        {
            try
            {
                isProcessingRequest = true;
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnRequestStarted?.Invoke());
                
                string url = $"{serverUrl}/generate_speech_realtime_audio";
                
                // 오디오 데이터를 Base64로 인코딩
                string audioBase64 = System.Convert.ToBase64String(audioData);
                
                // 요청 데이터 구성 (JSON 형태)
                var request = new
                {
                    audio_data = audioBase64,
                    language = language ?? defaultLanguage,
                    character_name = characterName ?? defaultCharacter,
                    use_thinking = true,
                    audio_format = "wav"
                };
                
                string jsonData = JsonConvert.SerializeObject(request);
                DebugLog($"실시간 TTS 요청 (음성 입력): {audioData.Length} bytes");
                
                await ProcessStreamingRequestAsync(url, jsonData, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                DebugLog("음성 TTS 요청이 취소되었습니다.");
            }
            catch (Exception e)
            {
                DebugLog($"음성 TTS 처리 오류: {e.Message}");
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnError?.Invoke(e.Message));
            }
            finally
            {
                isProcessingRequest = false;
                UnityMainThreadDispatcher.Instance().Enqueue(() => OnRequestCompleted?.Invoke());
            }
        }
        
        /// <summary>
        /// 스트리밍 요청을 비동기로 처리합니다
        /// </summary>
        /// <param name="url">요청 URL</param>
        /// <param name="jsonData">요청 데이터</param>
        /// <param name="cancellationToken">취소 토큰</param>
        private async UniTask ProcessStreamingRequestAsync(string url, string jsonData, CancellationToken cancellationToken)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new AsyncStreamingDownloadHandler(this);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "text/event-stream");
                
                // 비동기 요청 시작
                var webRequestOperation = webRequest.SendWebRequest();
                
                // 요청 완료까지 대기하면서 취소 토큰 체크
                while (!webRequestOperation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        webRequest.Abort();
                        throw new OperationCanceledException();
                    }
                    
                    await UniTask.Delay(50, cancellationToken: cancellationToken); // 50ms 대기
                }
                
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    string errorMsg = $"요청 실패: {webRequest.error}";
                    DebugLog(errorMsg);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => OnError?.Invoke(errorMsg));
                    return;
                }
                
                DebugLog("스트리밍 완료");
            }
        }
        
        /// <summary>
        /// 백그라운드 태스크 처리를 위한 코루틴
        /// </summary>
        private IEnumerator ProcessBackgroundTasks()
        {
            while (true)
            {
                while (backgroundTasks.Count > 0)
                {
                    var task = backgroundTasks.Dequeue();
                    if (task.Status != UniTaskStatus.Succeeded)
                    {
                        // 태스크가 완료될 때까지 대기하지 않고 다음 프레임으로 넘어감
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        // 완료된 태스크 정리
                        if (task.Status == UniTaskStatus.Faulted)
                        {
                            try
                            {
                                task.GetAwaiter().GetResult();
                            }
                            catch (System.Exception ex)
                            {
                                DebugLog($"백그라운드 태스크 오류: {ex.Message}");
                            }
                        }
                    }
                }
                
                yield return new WaitForSeconds(0.1f); // 100ms마다 체크
            }
        }
        
        #endregion
        
        #region Stream Processing Methods
        
        /// <summary>
        /// 스트리밍 핸들러에서 호출되는 공개 메서드 (기존 호환성 유지)
        /// </summary>
        /// <param name="line">처리할 스트림 라인</param>
        public IEnumerator ProcessSingleStreamLine(string line)
        {
            yield return StartCoroutine(ProcessSingleStreamLineAsync(line));
        }
        
        /// <summary>
        /// 단일 스트림 라인을 비동기로 처리합니다
        /// </summary>
        /// <param name="line">처리할 스트림 라인</param>
        public IEnumerator ProcessSingleStreamLineAsync(string line)
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                yield break;
            
            // 예외 처리를 위한 변수들
            StreamingResponseData data = null;
            bool hasError = false;
            string errorMessage = "";
            
            // JSON 파싱 (try-catch에서 yield 사용 불가하므로 분리)
            try
            {
                // "data: " 제거
                string jsonData = line.Substring(6);
                
                // JSON 파싱
                data = JsonConvert.DeserializeObject<StreamingResponseData>(jsonData);
            }
            catch (Exception e)
            {
                hasError = true;
                errorMessage = $"스트림 처리 오류: {e.Message}";
            }
            
            // 오류가 발생했으면 처리하고 종료
            if (hasError)
            {
                DebugLog(errorMessage);
                OnError?.Invoke(errorMessage);
                yield break;
            }
            
            // 데이터가 null이면 종료
            if (data == null)
            {
                DebugLog("JSON 파싱 실패: null 데이터");
                yield break;
            }
            
            // 데이터 타입에 따른 처리 (try-catch 없이)
            yield return StartCoroutine(ProcessStreamingData(data));
        }
        
        /// <summary>
        /// 스트리밍 데이터를 처리합니다
        /// </summary>
        /// <param name="data">처리할 스트리밍 데이터</param>
        private IEnumerator ProcessStreamingData(StreamingResponseData data)
        {
            switch (data.type)
            {
                case "text":
                    OnTextGenerated?.Invoke(data.text);
                    DebugLog($"텍스트 생성: {data.text}");
                    break;
                    
                case "audio":
                    if (!string.IsNullOrEmpty(data.audio_data))
                    {
                        yield return StartCoroutine(ProcessAudioDataSequentiallyAsync(data));
                    }
                    break;
                    
                case "sentence_complete":
                    OnSentenceCompleted?.Invoke(data.sentence_id, data.text);
                    DebugLog($"문장 완료: {data.sentence_id} - {data.text}");
                    break;
                    
                case "complete":
                    OnAllCompleted?.Invoke(data.total_sentences);
                    DebugLog($"전체 완료: {data.total_sentences}개 문장");
                    break;
                    
                case "error":
                    OnError?.Invoke(data.error);
                    DebugLog($"오류 발생: {data.error}");
                    break;
            }
        }
        
        /// <summary>
        /// 오디오 데이터를 순차적으로 처리합니다
        /// </summary>
        /// <param name="data">스트리밍 응답 데이터</param>
        private IEnumerator ProcessAudioDataSequentiallyAsync(StreamingResponseData data)
        {
            // 백그라운드에서 WAV 데이터 파싱 (AudioClip 생성 제외)
            UniTask<AudioClipData> audioTask = UniTask.Run(() =>
            {
                try
                {
                    // Base64 디코딩
                    byte[] audioBytes = Convert.FromBase64String(data.audio_data);
                    
                    // WAV 데이터를 파싱 (AudioClip 생성은 메인 스레드에서)
                    return ParseWAVData(audioBytes, $"Sentence_{data.sentence_id}");
                }
                catch (Exception e)
                {
                    DebugLog($"오디오 처리 오류: {e.Message}");
                    return null;
                }
            });
            
            // 태스크 완료 대기 (비동기)
            while (audioTask.Status == UniTaskStatus.Pending)
            {
                yield return null;
            }
            
            AudioClipData clipData = audioTask.GetAwaiter().GetResult();
            
            if (clipData != null)
            {
                // 메인 스레드에서 AudioClip 생성
                AudioClip audioClip = CreateAudioClipFromData(clipData);
                
                if (audioClip != null)
                {
                    // 순차 재생을 위해 버퍼에 저장
                    audioBuffers[data.sentence_id] = audioClip;
                    DebugLog($"[문장 {data.sentence_id}] 오디오 버퍼에 저장됨");
                    
                    // 순차적으로 재생 가능한지 확인하고 재생
                    yield return StartCoroutine(TryPlayNextSequentialAudioAsync());
                }
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 다음 순차 오디오 재생을 비동기로 시도합니다
        /// </summary>
        private IEnumerator TryPlayNextSequentialAudioAsync()
        {
            while (audioBuffers.ContainsKey(nextSentenceToPlay) && !isPlayingSequentially)
            {
                isPlayingSequentially = true;
                
                AudioClip clipToPlay = audioBuffers[nextSentenceToPlay];
                audioBuffers.Remove(nextSentenceToPlay);
                
                // 사용 가능한 AudioSource 찾기
                AudioSource availableSource = GetAvailableAudioSource();
                
                if (availableSource != null)
                {
                    availableSource.clip = clipToPlay;
                    availableSource.Play();
                    
                    DebugLog($"[문장 {nextSentenceToPlay}] 순차 재생 시작");
                    OnSentenceCompleted?.Invoke(nextSentenceToPlay, "");
                    
                    // 재생 완료 대기를 비동기로 처리
                    yield return StartCoroutine(WaitForAudioCompletionAsync(availableSource));
                    
                    DebugLog($"[문장 {nextSentenceToPlay}] 재생 완료");
                    nextSentenceToPlay++;
                    
                    // 다음 문장이 이미 버퍼에 있는지 확인하고 연속 재생
                    if (audioBuffers.ContainsKey(nextSentenceToPlay))
                    {
                        DebugLog($"다음 문장 {nextSentenceToPlay}이(가) 준비되어 연속 재생 중...");
                        isPlayingSequentially = false; // 다음 루프를 위해 플래그 리셋
                        yield return null; // 한 프레임 대기
                        continue;
                    }
                }
                else
                {
                    DebugLog("사용 가능한 AudioSource가 없습니다.");
                    audioQueue.Enqueue(clipToPlay);
                    nextSentenceToPlay++;
                }
                
                isPlayingSequentially = false;
                yield return null;
            }
            
            // 현재 재생 가능한 문장이 없을 때 버퍼에 있는 문장들 로그
            if (audioBuffers.Count > 0)
            {
                string bufferedSentences = string.Join(", ", audioBuffers.Keys);
                DebugLog($"대기 중인 문장들: [{bufferedSentences}], 다음 재생할 문장: {nextSentenceToPlay}");
            }
        }
        
        /// <summary>
        /// 오디오 완료 대기를 비동기로 처리합니다
        /// </summary>
        /// <param name="source">대기할 AudioSource</param>
        private IEnumerator WaitForAudioCompletionAsync(AudioSource source)
        {
            while (source.isPlaying)
            {
                // 더 자주 yield하여 다른 작업이 수행될 수 있도록 함
                yield return null;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 파싱된 오디오 데이터를 담는 클래스
    /// AudioClip 생성을 메인 스레드에서 하기 위해 중간 데이터 저장용
    /// </summary>
    public class AudioClipData
    {
        /// <summary>AudioClip 이름</summary>
        public string clipName;
        
        /// <summary>오디오 샘플 데이터</summary>
        public float[] samples;
        
        /// <summary>샘플 수</summary>
        public int sampleCount;
        
        /// <summary>채널 수</summary>
        public int channels;
        
        /// <summary>샘플링 레이트</summary>
        public int sampleRate;
    }
} 