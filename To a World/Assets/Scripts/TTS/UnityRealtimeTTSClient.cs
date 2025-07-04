using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class UnityRealtimeTTSClient : MonoBehaviour
{
    [Header("서버 설정")]
    [SerializeField] private string serverUrl = "http://localhost:8000";
    
    [Header("TTS 설정")]
    [SerializeField] private string defaultLanguage = "en";
    [SerializeField] private string defaultCharacter = "friendly_assistant";
    
    [Header("오디오 설정")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int maxConcurrentAudio = 3; // 동시 재생 가능한 오디오 수
    
    [Header("디버그")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 내부 변수들
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    private List<AudioSource> audioSources = new List<AudioSource>();
    private int currentAudioIndex = 0;
    
    // 순차 재생을 위한 추가 변수들
    private Dictionary<int, AudioClip> audioBuffers = new Dictionary<int, AudioClip>();
    private int nextSentenceToPlay = 1; // 다음에 재생할 문장 ID
    private bool isPlayingSequentially = false;
    
    // 이벤트
    public event Action<string> OnStart;
    public event Action<string> OnTextGenerated;
    public event Action<int, string> OnSentenceCompleted;
    public event Action<int> OnAllCompleted;
    public event Action<string> OnError;
    
    void Start()
    {
        InitializeAudioSources();
    }
    
    private void InitializeAudioSources()
    {
        // 여러 AudioSource 준비 (동시 재생용)
        for (int i = 0; i < maxConcurrentAudio; i++)
        {
            GameObject audioObj = new GameObject($"RealtimeAudio_{i}");
            audioObj.transform.SetParent(transform);
            
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            
            audioSources.Add(source);
        }
        
        DebugLog($"{maxConcurrentAudio}개의 AudioSource 초기화 완료");
    }
    
    public void StartRealtimeTTSWithAudio(byte[] audioData, string characterName = null, string language = null)
    {
        // 이전 상태 초기화
        audioBuffers.Clear();
        nextSentenceToPlay = 1;
        isPlayingSequentially = false;
        StopAllAudio();
        
        StartCoroutine(RealtimeTTSWithAudioCoroutine(audioData, characterName, language));
    }
    
    private IEnumerator RealtimeTTSWithAudioCoroutine(byte[] audioData, string characterName, string language)
    {
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
        
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new StreamingDownloadHandler(this);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "text/event-stream");
            
            // 스트리밍 시작 - 실시간으로 데이터가 들어올 때마다 처리됨
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"요청 실패: {webRequest.error}";
                DebugLog(errorMsg);
                OnError?.Invoke(errorMsg);
                yield break;
            }
            
            DebugLog("음성 스트리밍 완료");
        }
    }
    
    // 스트리밍 핸들러에서 호출되는 공개 메서드
    public IEnumerator ProcessSingleStreamLine(string line)
    {
        if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
            yield break;
        
        StreamingResponseData data = null;
        bool parseSuccess = false;
        
        try
        {
            string jsonPart = line.Substring(6).Trim(); // "data: " 제거 및 공백 제거
            
            // 빈 JSON 체크
            if (string.IsNullOrEmpty(jsonPart) || jsonPart == "{}" || jsonPart == "null")
            {
                yield break;
            }
            
            data = JsonConvert.DeserializeObject<StreamingResponseData>(jsonPart);
            parseSuccess = data != null;
            
            if (parseSuccess)
            {
                DebugLog($"JSON 파싱 성공: type={data.type}, sentence_id={data.sentence_id}");
            }
        }
        catch (JsonException e)
        {
            DebugLog($"JSON 파싱 오류: {e.Message} | 라인 길이: {line.Length}");
            // JSON 파싱 실패 시 완전한 라인이 아닐 가능성이 높으므로 스킵
            yield break;
        }
        catch (Exception e)
        {
            DebugLog($"일반 파싱 오류: {e.Message}");
            yield break;
        }
        
        if (parseSuccess && data != null)
        {
            switch (data.type)
            {
                case "metadata":
                    DebugLog($"메타데이터 수신: 언어={data.language}, 캐릭터={data.character}");
                    break;
                    
                case "text":
                    DebugLog($"[문장 {data.sentence_id}] 텍스트: {data.text}");
                    OnTextGenerated?.Invoke(data.text);
                    break;
                    
                case "audio":
                    DebugLog($"[문장 {data.sentence_id}] 오디오 수신 ({data.audio_length} bytes)");
                    yield return StartCoroutine(ProcessAudioDataSequentially(data));
                    break;
                    
                case "complete":
                    DebugLog($"완료! 총 {data.total_sentences}개 문장 처리됨");
                    OnAllCompleted?.Invoke(data.total_sentences);
                    break;
                    
                case "error":
                    DebugLog($"서버 오류: {data.error}");
                    OnError?.Invoke(data.error);
                    break;
                    
                default:
                    DebugLog($"알 수 없는 데이터 타입: {data.type}");
                    break;
            }
        }
        
        yield return null;
    }
    
    private IEnumerator ProcessAudioDataSequentially(StreamingResponseData data)
    {
        AudioClip audioClip = null;
        bool success = false;
        
        try
        {
            // Base64 디코딩
            byte[] audioBytes = Convert.FromBase64String(data.audio_data);
            
            // WAV 바이트를 AudioClip으로 변환
            audioClip = WAVToAudioClip(audioBytes, $"Sentence_{data.sentence_id}");
            success = true;
        }
        catch (Exception e)
        {
            DebugLog($"오디오 처리 오류: {e.Message}");
        }
        
        if (success && audioClip != null)
        {
            // 순차 재생을 위해 버퍼에 저장
            audioBuffers[data.sentence_id] = audioClip;
            DebugLog($"[문장 {data.sentence_id}] 오디오 버퍼에 저장됨");
            
            // 순차적으로 재생 가능한지 확인하고 재생
            yield return StartCoroutine(TryPlayNextSequentialAudio());
        }
        
        yield return null;
    }
    
    private IEnumerator TryPlayNextSequentialAudio()
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
                
                // 재생이 완료될 때까지 대기
                while (availableSource.isPlaying)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                DebugLog($"[문장 {nextSentenceToPlay}] 재생 완료");
                nextSentenceToPlay++;
                
                // 다음 문장이 이미 버퍼에 있는지 확인하고 연속 재생
                if (audioBuffers.ContainsKey(nextSentenceToPlay))
                {
                    DebugLog($"다음 문장 {nextSentenceToPlay}이(가) 준비되어 연속 재생 중...");
                    isPlayingSequentially = false; // 다음 루프를 위해 플래그 리셋
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
    
    private AudioClip WAVToAudioClip(byte[] wavData, string clipName)
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
            
            // AudioClip 생성
            AudioClip audioClip = AudioClip.Create(clipName, sampleCount, channels, sampleRate, false);
            audioClip.SetData(samples, 0);
            
            return audioClip;
        }
        catch (Exception e)
        {
            DebugLog($"WAV 변환 오류: {e.Message}");
            return null;
        }
    }
    
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
    
    public void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[RealtimeTTS] {message}");
        }
    }
    
    // 외부에서 호출할 수 있는 공개 메서드들
    public void StopAllAudio()
    {
        foreach (AudioSource source in audioSources)
        {
            source.Stop();
            source.clip = null;
        }
        audioQueue.Clear();
        DebugLog("모든 오디오 중지됨");
    }
    
    public bool IsPlaying()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
                return true;
        }
        return audioQueue.Count > 0;
    }
    
    public void SetServerUrl(string newUrl)
    {
        serverUrl = newUrl;
        DebugLog($"서버 URL 변경: {newUrl}");
    }
    
    public void SetDefaultCharacter(string characterName)
    {
        defaultCharacter = characterName;
        DebugLog($"기본 캐릭터 변경: {characterName}");
    }
}