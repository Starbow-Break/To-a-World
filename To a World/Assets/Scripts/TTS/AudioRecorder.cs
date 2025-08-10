using System;
using System.Collections;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// 마이크 녹음 관리 클래스
    /// 
    /// 주요 기능:
    /// - 마이크 장치 초기화 및 감지
    /// - 음성 녹음 시작/중지
    /// - 자동 녹음 중지 (최대 녹음 시간 도달 시)
    /// - 녹음 상태 추적 및 이벤트 알림
    /// 
    /// 사용 방법:
    /// ```csharp
    /// var recorder = GetComponent<AudioRecorder>();
    /// recorder.OnRecordingCompleted += OnAudioRecorded;
    /// recorder.StartRecording();
    /// ```
    /// </summary>
    public class AudioRecorder : MonoBehaviour
    {
        #region 설정 변수
        
        [Header("=== 녹음 설정 ===")]
        [SerializeField] 
        [Tooltip("녹음 샘플링 레이트")]
        private int sampleRate = 44100;
        
        [SerializeField] 
        [Tooltip("최대 녹음 시간 (초)")]
        private int maxRecordingTime = 30;
        
        [SerializeField] 
        [Tooltip("녹음 시작 시 자동으로 기존 녹음 정리")]
        private bool autoCleanupOnStart = true;
        
        #endregion
        
        #region 상태 변수
        
        /// <summary>현재 녹음 중인지 여부</summary>
        public bool IsRecording { get; private set; }
        
        /// <summary>마이크 장치 이름</summary>
        public string MicrophoneDevice { get; private set; }
        
        /// <summary>마이크 장치가 사용 가능한지 여부</summary>
        public bool IsMicrophoneAvailable { get; private set; }
        
        /// <summary>현재 녹음된 클립</summary>
        public AudioClip CurrentRecordedClip { get; private set; }
        
        /// <summary>녹음 시작 시간</summary>
        public float RecordingStartTime { get; private set; }
        
        /// <summary>현재 녹음 시간</summary>
        public float CurrentRecordingTime => IsRecording ? Time.time - RecordingStartTime : 0f;
        
        #endregion
        
        #region 이벤트
        
        /// <summary>녹음 시작 이벤트</summary>
        public event Action OnRecordingStarted;
        
        /// <summary>녹음 완료 이벤트 (AudioClip 포함)</summary>
        public event Action<AudioClip> OnRecordingCompleted;
        
        /// <summary>녹음 취소 이벤트</summary>
        public event Action OnRecordingCancelled;
        
        /// <summary>녹음 오류 이벤트</summary>
        public event Action<string> OnRecordingError;
        
        /// <summary>마이크 상태 변경 이벤트</summary>
        public event Action<bool> OnMicrophoneStatusChanged;
        
        #endregion
        
        #region 내부 변수
        
        /// <summary>자동 중지 코루틴 참조</summary>
        private Coroutine autoStopCoroutine;
        
        #endregion
        
        #region Unity 라이프사이클
        
        /// <summary>
        /// Unity Start 메서드
        /// 마이크 초기화 수행
        /// </summary>
        void Start()
        {
            InitializeMicrophone();
        }
        
        /// <summary>
        /// Unity OnDestroy 메서드
        /// 리소스 정리 수행
        /// </summary>
        void OnDestroy()
        {
            CleanupResources();
        }
        
        #endregion
        
        #region 마이크 초기화
        
        /// <summary>
        /// 마이크 장치를 초기화합니다
        /// 사용 가능한 마이크 장치를 감지하고 설정합니다
        /// </summary>
        public void InitializeMicrophone()
        {
            try
            {
                // 마이크 장치 감지
                if (Microphone.devices.Length > 0)
                {
                    #if UNITY_EDITOR
                    foreach (var device in Microphone.devices)
                    {
                        if (device.Contains("Oculus") || device.Contains("Quest"))
                        {
                            MicrophoneDevice = device;
                        }
                    }
                    #else
                    MicrophoneDevice = Microphone.devices[0];
                    #endif
                    
                    IsMicrophoneAvailable = true;
                    Debug.Log($"[AudioRecorder] 마이크 장치 초기화 완료: {MicrophoneDevice}");
                }
                else
                {
                    MicrophoneDevice = null;
                    IsMicrophoneAvailable = false;
                    
                    Debug.LogWarning("[AudioRecorder] 마이크 장치를 찾을 수 없습니다.");
                }
                
                // 마이크 상태 변경 이벤트 알림
                OnMicrophoneStatusChanged?.Invoke(IsMicrophoneAvailable);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioRecorder] 마이크 초기화 중 오류: {e.Message}");
                IsMicrophoneAvailable = false;
                OnMicrophoneStatusChanged?.Invoke(false);
            }
        }
        
        #endregion
        
        #region 녹음 제어
        
        /// <summary>
        /// 녹음을 시작합니다
        /// </summary>
        /// <returns>녹음 시작 성공 여부</returns>
        public bool StartRecording()
        {
            // 이미 녹음 중인 경우
            if (IsRecording)
            {
                Debug.LogWarning("[AudioRecorder] 이미 녹음 중입니다.");
                return false;
            }
            
            // 마이크 장치 사용 불가능한 경우
            if (!IsMicrophoneAvailable)
            {
                string error = "마이크 장치가 사용 불가능합니다.";
                Debug.LogError($"[AudioRecorder] {error}");
                OnRecordingError?.Invoke(error);
                return false;
            }
            
            try
            {
                // 기존 녹음 정리 (옵션)
                if (autoCleanupOnStart)
                {
                    CleanupCurrentRecording();
                }
                
                // 녹음 시작
                CurrentRecordedClip = Microphone.Start(MicrophoneDevice, false, maxRecordingTime, sampleRate);
                
                if (CurrentRecordedClip == null)
                {
                    string error = "녹음 시작 실패: AudioClip 생성 오류";
                    Debug.LogError($"[AudioRecorder] {error}");
                    OnRecordingError?.Invoke(error);
                    return false;
                }
                
                // 녹음 상태 업데이트
                IsRecording = true;
                RecordingStartTime = Time.time;
                
                // 자동 중지 코루틴 시작
                autoStopCoroutine = StartCoroutine(AutoStopRecording());
                
                Debug.Log($"[AudioRecorder] 녹음 시작 - 최대 시간: {maxRecordingTime}초");
                OnRecordingStarted?.Invoke();
                
                return true;
            }
            catch (Exception e)
            {
                string error = $"녹음 시작 중 오류: {e.Message}";
                Debug.LogError($"[AudioRecorder] {error}");
                OnRecordingError?.Invoke(error);
                return false;
            }
        }
        
        /// <summary>
        /// 녹음을 중지합니다
        /// </summary>
        /// <returns>녹음 중지 성공 여부</returns>
        public bool StopRecording()
        {
            if (!IsRecording)
            {
                Debug.LogWarning("[AudioRecorder] 현재 녹음 중이 아닙니다.");
                return false;
            }
            
            try
            {
                // 마이크 중지
                Microphone.End(MicrophoneDevice);
                
                // 자동 중지 코루틴 정리
                if (autoStopCoroutine != null)
                {
                    StopCoroutine(autoStopCoroutine);
                    autoStopCoroutine = null;
                }
                
                // 녹음 상태 업데이트
                IsRecording = false;
                
                Debug.Log($"[AudioRecorder] 녹음 완료 - 녹음 시간: {CurrentRecordingTime:F2}초");
                
                // 녹음 완료 이벤트 알림
                OnRecordingCompleted?.Invoke(CurrentRecordedClip);
                
                return true;
            }
            catch (Exception e)
            {
                string error = $"녹음 중지 중 오류: {e.Message}";
                Debug.LogError($"[AudioRecorder] {error}");
                OnRecordingError?.Invoke(error);
                return false;
            }
        }
        
        /// <summary>
        /// 녹음을 취소합니다
        /// 녹음된 데이터를 폐기합니다
        /// </summary>
        public void CancelRecording()
        {
            if (IsRecording)
            {
                // 마이크 중지
                Microphone.End(MicrophoneDevice);
                
                // 자동 중지 코루틴 정리
                if (autoStopCoroutine != null)
                {
                    StopCoroutine(autoStopCoroutine);
                    autoStopCoroutine = null;
                }
                
                // 녹음 상태 업데이트
                IsRecording = false;
                
                Debug.Log("[AudioRecorder] 녹음 취소됨");
            }
            
            // 녹음 데이터 정리
            CleanupCurrentRecording();
            
            // 취소 이벤트 알림
            OnRecordingCancelled?.Invoke();
        }
        
        #endregion
        
        #region 자동 중지 기능
        
        /// <summary>
        /// 자동 녹음 중지 코루틴
        /// 최대 녹음 시간에 도달하면 자동으로 녹음을 중지합니다
        /// </summary>
        /// <returns>코루틴 IEnumerator</returns>
        private IEnumerator AutoStopRecording()
        {
            yield return new WaitForSeconds(maxRecordingTime);
            
            if (IsRecording)
            {
                Debug.Log("[AudioRecorder] 최대 녹음 시간 도달, 자동 중지");
                StopRecording();
            }
        }
        
        #endregion
        
        #region 리소스 관리
        
        /// <summary>
        /// 현재 녹음 클립을 정리합니다
        /// </summary>
        private void CleanupCurrentRecording()
        {
            if (CurrentRecordedClip != null)
            {
                DestroyImmediate(CurrentRecordedClip);
                CurrentRecordedClip = null;
            }
        }
        
        /// <summary>
        /// 모든 리소스를 정리합니다
        /// </summary>
        private void CleanupResources()
        {
            // 녹음 중지
            if (IsRecording)
            {
                Microphone.End(MicrophoneDevice);
                IsRecording = false;
            }
            
            // 코루틴 정리
            if (autoStopCoroutine != null)
            {
                StopCoroutine(autoStopCoroutine);
                autoStopCoroutine = null;
            }
            
            // 녹음 클립 정리
            CleanupCurrentRecording();
        }
        
        #endregion
        
        #region 공개 유틸리티 메서드
        
        /// <summary>
        /// 마이크 장치 목록을 가져옵니다
        /// </summary>
        /// <returns>마이크 장치 이름 배열</returns>
        public string[] GetAvailableMicrophoneDevices()
        {
            return Microphone.devices;
        }
        
        /// <summary>
        /// 특정 마이크 장치를 설정합니다
        /// </summary>
        /// <param name="deviceName">설정할 마이크 장치 이름</param>
        /// <returns>설정 성공 여부</returns>
        public bool SetMicrophoneDevice(string deviceName)
        {
            if (IsRecording)
            {
                Debug.LogWarning("[AudioRecorder] 녹음 중에는 마이크 장치를 변경할 수 없습니다.");
                return false;
            }
            
            var devices = GetAvailableMicrophoneDevices();
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i] == deviceName)
                {
                    MicrophoneDevice = deviceName;
                    IsMicrophoneAvailable = true;
                    
                    Debug.Log($"[AudioRecorder] 마이크 장치 변경: {deviceName}");
                    OnMicrophoneStatusChanged?.Invoke(true);
                    return true;
                }
            }
            
            Debug.LogWarning($"[AudioRecorder] 마이크 장치를 찾을 수 없습니다: {deviceName}");
            return false;
        }
        
        /// <summary>
        /// 현재 녹음 상태 정보를 가져옵니다
        /// </summary>
        /// <returns>녹음 상태 정보 문자열</returns>
        public string GetRecordingStatusInfo()
        {
            if (!IsMicrophoneAvailable)
            {
                return "마이크 사용 불가능";
            }
            
            if (IsRecording)
            {
                return $"녹음 중... ({CurrentRecordingTime:F1}초 / {maxRecordingTime}초)";
            }
            
            return "녹음 준비됨";
        }
        
        #endregion
    }
} 