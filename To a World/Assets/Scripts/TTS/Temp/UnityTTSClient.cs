using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

[System.Serializable]
public class TTSRequest
{
    public string text;
    public string system_prompt;
    public string language = "en";
    public bool use_thinking = true;
    public string character_name;
    public string personality;
    public string speaking_style;
}

[System.Serializable]
public class TTSResponse
{
    public string generated_text;
    public string audio_url;
    public bool success;
    public string message;
}

[System.Serializable]
public class CharacterRequest
{
    public string character_name;
    public string personality;
    public string speaking_style;
    public string language_preference = "en";
}

[System.Serializable]
public class HealthResponse
{
    public string status;
    public bool tts_service_ready;
    public bool gpu_available;
    public float timestamp;
}

public class UnityTTSClient : MonoBehaviour
{
    [Header("Server Settings")]
    public string serverUrl = "http://localhost:8000";
    
    [Header("Character Settings")]
    public string characterName = "Friendly Assistant";
    public string personality = "warm and encouraging";
    public string speakingStyle = "casual and enthusiastic";
    public string language = "en";
    
    [Header("TTS Settings")]
    public bool useThinking = true;
    public string systemPrompt = "";
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    
    [Header("UI References")]
    public TMPro.TMP_InputField inputField;
    public TMPro.TMP_Text responseText;
    public UnityEngine.UI.Button sendButton;
    public UnityEngine.UI.Button healthCheckButton;
    public TMPro.TMP_Text statusText;
    
    [Header("Debug")]
    public bool debugMode = true;
    
    private void Start()
    {
        // AudioSource 자동 설정
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // UI 이벤트 설정
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendButtonClick);
        
        if (healthCheckButton != null)
            healthCheckButton.onClick.AddListener(OnHealthCheckButtonClick);
        
        // 서버 상태 확인
        StartCoroutine(CheckServerHealth());
    }
    
    public void OnSendButtonClick()
    {
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            SendTTSRequest(inputField.text);
            inputField.text = "";
        }
    }
    
    public void OnHealthCheckButtonClick()
    {
        StartCoroutine(CheckServerHealth());
    }
    
    public void SendTTSRequest(string text)
    {
        StartCoroutine(SendTTSRequestCoroutine(text));
    }
    
    private IEnumerator SendTTSRequestCoroutine(string text)
    {
        UpdateStatus("음성 생성 중...");
        
        // TTS 요청 데이터 구성
        TTSRequest request = new TTSRequest
        {
            text = text,
            system_prompt = string.IsNullOrEmpty(systemPrompt) ? null : systemPrompt,
            language = language,
            use_thinking = useThinking,
            character_name = characterName,
            personality = personality,
            speaking_style = speakingStyle
        };
        
        string jsonData = JsonConvert.SerializeObject(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(serverUrl + "/generate_speech_stream", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 생성된 텍스트 가져오기
                string generatedText = webRequest.GetResponseHeader("X-Generated-Text");
                if (!string.IsNullOrEmpty(generatedText))
                {
                    UpdateResponseText(generatedText);
                }
                
                // 오디오 데이터 처리
                byte[] audioData = webRequest.downloadHandler.data;
                yield return StartCoroutine(PlayAudioFromBytes(audioData));
                
                UpdateStatus("완료");
            }
            else
            {
                string error = $"Error: {webRequest.error}";
                if (debugMode)
                    Debug.LogError($"TTS 요청 실패: {error}");
                UpdateStatus(error);
            }
        }
    }
    
    private IEnumerator PlayAudioFromBytes(byte[] audioData)
    {
        // WAV 파일을 AudioClip으로 변환
        AudioClip audioClip = null;
        
        try
        {
            audioClip = WavUtility.ToAudioClip(audioData, "generated_speech");
        }
        catch (Exception e)
        {
            Debug.LogError($"오디오 변환 오류: {e.Message}");
            yield break;
        }
        
        if (audioClip != null)
        {
            try
            {
                audioSource.clip = audioClip;
                audioSource.Play();
                
                if (debugMode)
                    Debug.Log($"오디오 재생 시작: {audioClip.length}초");
            }
            catch (Exception e)
            {
                Debug.LogError($"오디오 재생 오류: {e.Message}");
                yield break;
            }
            
            // 오디오 재생 완료까지 대기 (try-catch 밖에서 처리)
            yield return new WaitForSeconds(audioClip.length);
        }
        else
        {
            Debug.LogError("AudioClip 생성 실패");
        }
    }
    
    private IEnumerator CheckServerHealth()
    {
        UpdateStatus("서버 상태 확인 중...");
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(serverUrl + "/health"))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    HealthResponse health = JsonConvert.DeserializeObject<HealthResponse>(webRequest.downloadHandler.text);
                    string status = $"서버: {health.status}, TTS: {(health.tts_service_ready ? "준비됨" : "준비 안됨")}, GPU: {(health.gpu_available ? "사용 가능" : "사용 불가")}";
                    UpdateStatus(status);
                }
                catch (Exception e)
                {
                    UpdateStatus($"상태 파싱 오류: {e.Message}");
                }
            }
            else
            {
                UpdateStatus($"서버 연결 실패: {webRequest.error}");
            }
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        
        if (debugMode)
            Debug.Log($"상태: {message}");
    }
    
    private void UpdateResponseText(string text)
    {
        if (responseText != null)
            responseText.text = text;
        
        if (debugMode)
            Debug.Log($"응답: {text}");
    }
    
    // 캐릭터 설정 업데이트
    public void UpdateCharacterSettings(string newCharacterName, string newPersonality, string newSpeakingStyle)
    {
        characterName = newCharacterName;
        personality = newPersonality;
        speakingStyle = newSpeakingStyle;
        
        if (debugMode)
            Debug.Log($"캐릭터 설정 변경: {characterName} - {personality} - {speakingStyle}");
    }
    
    // 언어 설정 변경
    public void SetLanguage(string newLanguage)
    {
        language = newLanguage;
        if (debugMode)
            Debug.Log($"언어 설정 변경: {language}");
    }
    
    // 시스템 프롬프트 설정
    public void SetSystemPrompt(string prompt)
    {
        systemPrompt = prompt;
        if (debugMode)
            Debug.Log($"시스템 프롬프트 설정: {systemPrompt}");
    }
}

// WAV 파일을 AudioClip으로 변환하는 유틸리티 클래스
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] fileBytes, string name = "wav")
    {
        try
        {
            // WAV 헤더 파싱
            int headerSize = 44;
            if (fileBytes.Length < headerSize)
            {
                Debug.LogError("WAV 파일이 너무 작습니다.");
                return null;
            }
            
            // WAV 파일 형식 확인
            string riff = Encoding.ASCII.GetString(fileBytes, 0, 4);
            string wave = Encoding.ASCII.GetString(fileBytes, 8, 4);
            
            if (riff != "RIFF" || wave != "WAVE")
            {
                Debug.LogError("올바른 WAV 파일이 아닙니다.");
                return null;
            }
            
            // 오디오 형식 정보 추출
            int channels = BitConverter.ToInt16(fileBytes, 22);
            int sampleRate = BitConverter.ToInt32(fileBytes, 24);
            int bitsPerSample = BitConverter.ToInt16(fileBytes, 34);
            
            // 오디오 데이터 추출
            int dataSize = BitConverter.ToInt32(fileBytes, 40);
            float[] audioData = new float[dataSize / (bitsPerSample / 8)];
            
            int audioStartIndex = headerSize;
            
            if (bitsPerSample == 16)
            {
                for (int i = 0; i < audioData.Length; i++)
                {
                    int byteIndex = audioStartIndex + i * 2;
                    if (byteIndex + 1 < fileBytes.Length)
                    {
                        short sample = BitConverter.ToInt16(fileBytes, byteIndex);
                        audioData[i] = sample / 32768f; // 16비트를 -1~1로 정규화
                    }
                }
            }
            else if (bitsPerSample == 32)
            {
                for (int i = 0; i < audioData.Length; i++)
                {
                    int byteIndex = audioStartIndex + i * 4;
                    if (byteIndex + 3 < fileBytes.Length)
                    {
                        audioData[i] = BitConverter.ToSingle(fileBytes, byteIndex);
                    }
                }
            }
            else
            {
                Debug.LogError($"지원하지 않는 비트 깊이: {bitsPerSample}");
                return null;
            }
            
            // AudioClip 생성
            AudioClip audioClip = AudioClip.Create(name, audioData.Length / channels, channels, sampleRate, false);
            audioClip.SetData(audioData, 0);
            
            return audioClip;
        }
        catch (Exception e)
        {
            Debug.LogError($"WAV to AudioClip 변환 오류: {e.Message}");
            return null;
        }
    }
} 