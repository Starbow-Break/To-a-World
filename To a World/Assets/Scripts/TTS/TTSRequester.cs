using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class TTSRequester: MonoBehaviour
{
    public event Action OnStartRecording;
    public event Action OnStopRecording;
    public event Action OnStartVoiceGeneration;
    
    private UnityRealtimeTTSClient ttsClient;
    
    [Header("설정")]
    [SerializeField] private string[] availableCharacters = {"friendly_assistant", "professional", "casual", "energetic"};
    [SerializeField] private string[] availableLanguages = {"en", "ko", "ja", "zh"};
    
    [Header("음성 녹음 설정")]
    [SerializeField] private int recordingSampleRate = 44100;
    [SerializeField] private int maxRecordingTime = 30; // 최대 녹음 시간 (초)
    
    private int totalSentences = 0;
    private int completedSentences = 0;
    private bool isGenerating = false;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private string microphoneDevice;
    
    private void OnEnable()
    {
        InitializeMicrophone();
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc += OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc += OnExitedNpc;
    }
    
    private void OnDisable()
    {
        // 녹음 중지
        if (isRecording)
        {
            Microphone.End(microphoneDevice);
        }
        
        GameEventsManager.GetEvents<NpcEvents>().OnEnteredNpc -= OnEnteredNpc;
        GameEventsManager.GetEvents<NpcEvents>().OnExitedNpc -= OnExitedNpc;
    }

    private void OnEnteredNpc(INpc npc)
    {
        var talkableNpc = npc as TalkableNpc;
        ttsClient = talkableNpc.Client;
    }
    
    private void OnExitedNpc(INpc npc)
    {
        ttsClient = null;
    }
    
    // 마이크 초기화
    private void InitializeMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
#if UNITY_EDITOR
            foreach (var device in Microphone.devices)
            {
                
                if (device.Contains("Oculus") || device.Contains("Quest"))
                {
                    microphoneDevice = device;
                }
            }
#else
            microphoneDevice = Microphone.devices[0];
#endif
            Debug.Log($"마이크 장치 초기화: {microphoneDevice}");
        }
        else
        {
            Debug.LogWarning("마이크 장치를 찾을 수 없습니다");
        }
    }
    
    public void TryStartRecording()
    {
        if (ttsClient == null || isRecording || isGenerating)
            return;
            
        StartRecording();
    }
    
    public void TryStopRecording()
    {
        if (ttsClient == null || !isRecording)
            return;
            
        StopRecording();
    }
    
    private void StartRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogWarning("마이크 장치가 없습니다");
            return;
        }
        
        isRecording = true;
        recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingTime, recordingSampleRate);
        OnStartRecording?.Invoke();
        
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
        OnStopRecording?.Invoke();
        
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
            Debug.LogWarning("녹음된 오디오가 없습니다");
            yield break;
        }
        
        // 녹음된 오디오를 WAV 바이트로 변환
        byte[] wavData = AudioClipToWAV(recordedClip);
        
        if (wavData != null && wavData.Length > 0)
        {
            // 선택된 캐릭터와 언어 가져오기
            string selectedCharacter = "friendly_assistant";
            string selectedLanguage = "en";
            
            // TTS 클라이언트로 음성 데이터 전송
            if (ttsClient != null)
            {
                StartVoiceGeneration(wavData, selectedCharacter, selectedLanguage);
            }
            else
            {
                Debug.LogError("TTS 클라이언트가 설정되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogError("오디오 변환 실패");
        }
        
        yield return null;
    }
    
    // 음성 생성 시작
    private void StartVoiceGeneration(byte[] audioData, string character, string language)
    {
        isGenerating = true;
        totalSentences = 0;
        completedSentences = 0;

        OnStartVoiceGeneration?.Invoke();
        
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
        catch (Exception e)
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
    
    
}
