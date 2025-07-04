/*using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TTSUIController : MonoBehaviour
{
    [Header("UI 참조")]
    public TMP_InputField inputTextField;
    public Button generateButton;
    public Button stopButton;
    public Button recordStartButton;
    public Button recordStopButton;
    public TMP_Dropdown characterDropdown;
    public TMP_Dropdown languageDropdown;
    
    [Header("상태 표시")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI generatedText;
    public Slider progressSlider;
    public TextMeshProUGUI currentSentenceText;
    
    [Header("TTS 클라이언트")]
    public UnityRealtimeTTSClient ttsClient;
    
    [Header("설정")]
    public string[] availableCharacters = {"friendly_assistant", "professional", "casual", "energetic"};
    public string[] availableLanguages = {"en", "ko", "ja", "zh"};
    
    [Header("음성 녹음 설정")]
    public int recordingSampleRate = 44100;
    public int maxRecordingTime = 30; // 최대 녹음 시간 (초)
    
    private int totalSentences = 0;
    private int completedSentences = 0;
    private bool isGenerating = false;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private string microphoneDevice;
    
    void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }
    
    private void InitializeUI()
    {
        // 캐릭터 드롭다운 설정
        characterDropdown.ClearOptions();
        List<string> characterOptions = new List<string>(availableCharacters);
        characterDropdown.AddOptions(characterOptions);
        
        // 언어 드롭다운 설정
        languageDropdown.ClearOptions();
        List<string> languageOptions = new List<string>(availableLanguages);
        languageDropdown.AddOptions(languageOptions);
        
        // 초기 상태 설정
        statusText.text = "준비됨";
        generatedText.text = "";
        currentSentenceText.text = "";
        progressSlider.value = 0;
        stopButton.interactable = false;
        recordStopButton.interactable = false;
        
        // 마이크 장치 초기화
        InitializeMicrophone();
        
        // 기본값 설정
        if (inputTextField != null)
            inputTextField.text = "안녕하세요! 실시간 음성 합성 테스트입니다.";
    }
    
    private void SetupEventListeners()
    {
        // TTS 클라이언트 이벤트 구독
        if (ttsClient != null)
        {
            ttsClient.OnTextGenerated += OnTextGenerated;
            ttsClient.OnSentenceCompleted += OnSentenceCompleted;
            ttsClient.OnAllCompleted += OnAllCompleted;
            ttsClient.OnError += OnError;
        }
        
        // UI 버튼 이벤트
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
            
        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopButtonClicked);
            
        if (recordStartButton != null)
            recordStartButton.onClick.AddListener(OnRecordStartButtonClicked);
            
        if (recordStopButton != null)
            recordStopButton.onClick.AddListener(OnRecordStopButtonClicked);
    }
    
    public void OnGenerateButtonClicked()
    {
        if (isGenerating)
            return;
            
        string inputText = inputTextField.text.Trim();
        if (string.IsNullOrEmpty(inputText))
        {
            statusText.text = "텍스트를 입력해주세요.";
            statusText.color = Color.red;
            return;
        }
        
        // 선택된 캐릭터와 언어 가져오기
        string selectedCharacter = availableCharacters[characterDropdown.value];
        string selectedLanguage = availableLanguages[languageDropdown.value];
        
        StartGeneration(inputText, selectedCharacter, selectedLanguage);
    }
    
    public void OnStopButtonClicked()
    {
        if (ttsClient != null)
        {
            ttsClient.StopAllAudio();
        }
        
        ResetUIState();
        statusText.text = "중지됨";
        statusText.color = Color.yellow;
    }
    
    public void OnRecordStartButtonClicked()
    {
        if (isRecording || isGenerating)
            return;
            
        StartRecording();
    }
    
    public void OnRecordStopButtonClicked()
    {
        if (!isRecording)
            return;
            
        StopRecording();
    }
    
    private void StartGeneration(string text, string character, string language)
    {
        isGenerating = true;
        totalSentences = 0;
        completedSentences = 0;
        
        // UI 상태 업데이트
        generateButton.interactable = false;
        stopButton.interactable = true;
        statusText.text = "생성 중...";
        statusText.color = Color.blue;
        generatedText.text = "";
        currentSentenceText.text = "";
        progressSlider.value = 0;
        
        // TTS 시작
        if (ttsClient != null)
        {
            ttsClient.StartRealtimeTTS(text, character, language);
        }
        else
        {
            OnError("TTS 클라이언트가 설정되지 않았습니다.");
        }
    }
    
    private void ResetUIState()
    {
        isGenerating = false;
        generateButton.interactable = true;
        stopButton.interactable = false;
        recordStartButton.interactable = true;
        recordStopButton.interactable = false;
        progressSlider.value = 0;
    }
    
    // TTS 클라이언트 이벤트 핸들러들
    private void OnTextGenerated(string text)
    {
        if (generatedText != null)
        {
            generatedText.text += text + "\n";
        }
        
        currentSentenceText.text = $"현재: {text}";
    }
    
    private void OnSentenceCompleted(int sentenceId, string text)
    {
        completedSentences++;
        
        if (totalSentences > 0)
        {
            float progress = (float)completedSentences / totalSentences;
            progressSlider.value = progress;
        }
        
        statusText.text = $"진행 중... ({completedSentences}/{totalSentences})";
        statusText.color = Color.green;
        
        Debug.Log($"문장 {sentenceId} 완료: {text}");
    }
    
    private void OnAllCompleted(int totalSentenceCount)
    {
        totalSentences = totalSentenceCount;
        progressSlider.value = 1.0f;
        
        statusText.text = $"완료! (총 {totalSentenceCount}개 문장)";
        statusText.color = Color.green;
        currentSentenceText.text = "모든 문장 처리 완료";
        
        ResetUIState();
        
        Debug.Log($"모든 생성 완료: {totalSentenceCount}개 문장");
    }
    
    private void OnError(string errorMessage)
    {
        statusText.text = $"오류: {errorMessage}";
        statusText.color = Color.red;
        currentSentenceText.text = "";
        
        ResetUIState();
        
        Debug.LogError($"TTS 오류: {errorMessage}");
    }
    
    // 공개 메서드들 (외부에서 호출 가능)
    public void SetInputText(string text)
    {
        if (inputTextField != null)
            inputTextField.text = text;
    }
    
    public void SetCharacter(string characterName)
    {
        for (int i = 0; i < availableCharacters.Length; i++)
        {
            if (availableCharacters[i] == characterName)
            {
                characterDropdown.value = i;
                break;
            }
        }
    }
    
    public void SetLanguage(string languageCode)
    {
        for (int i = 0; i < availableLanguages.Length; i++)
        {
            if (availableLanguages[i] == languageCode)
            {
                languageDropdown.value = i;
                break;
            }
        }
    }
    
    public bool IsGenerating()
    {
        return isGenerating;
    }
    
    // 마이크 초기화
    private void InitializeMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            statusText.text = $"마이크 준비됨: {microphoneDevice}";
            Debug.Log($"마이크 장치 초기화: {microphoneDevice}");
        }
        else
        {
            statusText.text = "마이크 장치를 찾을 수 없습니다";
            statusText.color = Color.red;
            recordStartButton.interactable = false;
        }
    }
    
    // 녹음 시작
    private void StartRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            statusText.text = "마이크 장치가 없습니다";
            statusText.color = Color.red;
            return;
        }
        
        isRecording = true;
        recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingTime, recordingSampleRate);
        
        // UI 상태 업데이트
        recordStartButton.interactable = false;
        recordStopButton.interactable = true;
        generateButton.interactable = false;
        
        statusText.text = "녹음 중...";
        statusText.color = Color.red;
        currentSentenceText.text = "음성을 녹음하고 있습니다";
        
        // 최대 녹음 시간 후 자동 종료
        StartCoroutine(AutoStopRecording());
        
        Debug.Log("음성 녹음 시작");
    }
    
    // 녹음 중지
    private void StopRecording()
    {
        if (!isRecording)
            return;
            
        isRecording = false;
        Microphone.End(microphoneDevice);
        
        // UI 상태 업데이트
        recordStartButton.interactable = true;
        recordStopButton.interactable = false;
        
        statusText.text = "녹음 완료, 처리 중...";
        statusText.color = Color.blue;
        currentSentenceText.text = "녹음된 음성을 서버로 전송 중";
        
        Debug.Log("음성 녹음 완료");
        
        // 녹음된 오디오를 서버로 전송
        StartCoroutine(ProcessRecordedAudio());
    }
    
    // 자동 녹음 중지 코루틴
    private IEnumerator AutoStopRecording()
    {
        yield return new WaitForSeconds(maxRecordingTime);
        
        if (isRecording)
        {
            Debug.Log("최대 녹음 시간 도달, 자동 중지");
            StopRecording();
        }
    }
    
    // 녹음된 오디오 처리
    private IEnumerator ProcessRecordedAudio()
    {
        if (recordedClip == null)
        {
            statusText.text = "녹음된 오디오가 없습니다";
            statusText.color = Color.red;
            generateButton.interactable = true;
            yield break;
        }
        
        // 녹음된 오디오를 WAV 바이트로 변환
        byte[] wavData = AudioClipToWAV(recordedClip);
        
        if (wavData != null && wavData.Length > 0)
        {
            // 선택된 캐릭터와 언어 가져오기
            string selectedCharacter = availableCharacters[characterDropdown.value];
            string selectedLanguage = availableLanguages[languageDropdown.value];
            
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
            statusText.text = "오디오 변환 실패";
            statusText.color = Color.red;
            generateButton.interactable = true;
        }
        
        yield return null;
    }
    
    // 음성 생성 시작
    private void StartVoiceGeneration(byte[] audioData, string character, string language)
    {
        isGenerating = true;
        totalSentences = 0;
        completedSentences = 0;
        
        // UI 상태 업데이트
        generateButton.interactable = false;
        stopButton.interactable = true;
        statusText.text = "음성 생성 중...";
        statusText.color = Color.blue;
        generatedText.text = "";
        currentSentenceText.text = "";
        progressSlider.value = 0;
        
        // TTS 시작 (음성 데이터 전송)
        ttsClient.StartRealtimeTTSWithAudio(audioData, character, language);
    }
    
    // AudioClip을 WAV 바이트 배열로 변환
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
            Debug.LogError($"AudioClip to WAV 변환 오류: {e.Message}");
            return null;
        }
    }
    
    // WAV 헤더 작성
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
    
    void OnDestroy()
    {
        // 녹음 중지
        if (isRecording)
        {
            Microphone.End(microphoneDevice);
        }
        
        // 이벤트 구독 해제
        if (ttsClient != null)
        {
            ttsClient.OnTextGenerated -= OnTextGenerated;
            ttsClient.OnSentenceCompleted -= OnSentenceCompleted;
            ttsClient.OnAllCompleted -= OnAllCompleted;
            ttsClient.OnError -= OnError;
        }
    }
} */