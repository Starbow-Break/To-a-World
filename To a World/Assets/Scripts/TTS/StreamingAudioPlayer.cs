using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// 실시간 오디오 스트리밍 및 재생 관리 클래스
    /// 
    /// 주요 기능:
    /// - 실시간 오디오 청크 버퍼링
    /// - 순차적 오디오 재생
    /// - 오디오 큐 관리
    /// - 메모리 효율적인 오디오 처리
    /// 
    /// 사용 방법:
    /// ```csharp
    /// var player = GetComponent<StreamingAudioPlayer>();
    /// player.OnAudioChunkPlayed += OnChunkPlayed;
    /// player.AddAudioChunk(sentenceId, audioClip, npcText);
    /// ```
    /// </summary>
    public class StreamingAudioPlayer : MonoBehaviour
    {
        #region 설정 변수
        
        [Header("=== 오디오 재생 설정 ===")]
        [SerializeField] 
        [Tooltip("오디오 소스 참조")]
        private AudioSource audioSource;
        
        [SerializeField] 
        [Tooltip("최대 버퍼 크기")]
        private int maxBufferSize = 10;
        
        [SerializeField] 
        [Tooltip("오디오 청크 간 간격 (초)")]
        private float chunkInterval = 0.1f;
        
        [SerializeField] 
        [Tooltip("자동 메모리 정리 활성화")]
        private bool autoCleanup = true;
        
        #endregion
        
        #region 상태 변수
        
        /// <summary>현재 재생 중인지 여부</summary>
        public bool IsPlaying { get; private set; }
        
        /// <summary>순차 재생 중인지 여부</summary>
        public bool IsSequentialPlayback { get; private set; }
        
        /// <summary>다음에 재생할 문장 ID</summary>
        public int NextSentenceToPlay { get; private set; } = 1;
        
        /// <summary>버퍼된 오디오 청크 수</summary>
        public int BufferedChunkCount => audioBuffers.Count;
        
        /// <summary>현재 재생 중인 청크</summary>
        public AudioChunk? CurrentPlayingChunk { get; private set; }
        
        /// <summary>총 재생된 청크 수</summary>
        public int TotalPlayedChunks { get; private set; }
        
        #endregion
        
        #region 이벤트
        
        /// <summary>오디오 청크 재생 시작 이벤트</summary>
        public event Action<AudioChunk> OnAudioChunkStarted;
        
        /// <summary>오디오 청크 재생 완료 이벤트</summary>
        public event Action<AudioChunk> OnAudioChunkCompleted;
        
        /// <summary>모든 오디오 재생 완료 이벤트</summary>
        public event Action<int> OnAllAudioCompleted; // totalChunks
        
        /// <summary>재생 상태 변경 이벤트</summary>
        public event Action<bool> OnPlaybackStateChanged; // isPlaying
        
        /// <summary>오디오 오류 이벤트</summary>
        public event Action<string> OnAudioError;
        
        #endregion
        
        #region 내부 변수
        
        /// <summary>오디오 버퍼 (문장 ID별)</summary>
        private Dictionary<int, AudioChunk> audioBuffers = new Dictionary<int, AudioChunk>();
        
        /// <summary>순차 재생 코루틴</summary>
        private Coroutine sequentialPlaybackCoroutine;
        
        /// <summary>현재 재생 중인 오디오 클립</summary>
        private AudioClip currentlyPlayingClip;
        
        #endregion
        
        #region 오디오 청크 구조체
        
        /// <summary>
        /// 오디오 청크 정보 구조체
        /// </summary>
        [Serializable]
        public struct AudioChunk
        {
            public int sentenceId;
            public AudioClip audioClip;
            public string npcText;
            public float duration;
            public DateTime timestamp;
            
            public AudioChunk(int sentenceId, AudioClip audioClip, string npcText)
            {
                this.sentenceId = sentenceId;
                this.audioClip = audioClip;
                this.npcText = npcText;
                this.duration = audioClip != null ? audioClip.length : 0f;
                this.timestamp = DateTime.Now;
            }
        }
        
        #endregion
        
        #region Unity 라이프사이클
        
        /// <summary>
        /// Unity Start 메서드
        /// 오디오 소스를 초기화합니다
        /// </summary>
        void Start()
        {
            InitializeAudioSource();
        }
        
        /// <summary>
        /// Unity OnDestroy 메서드
        /// 리소스를 정리합니다
        /// </summary>
        void OnDestroy()
        {
            CleanupResources();
        }
        
        #endregion
        
        #region 오디오 소스 초기화
        
        /// <summary>
        /// 오디오 소스를 초기화합니다
        /// </summary>
        private void InitializeAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            // 오디오 소스 기본 설정
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            
            Debug.Log("[StreamingAudioPlayer] 오디오 소스 초기화 완료");
        }
        
        #endregion
        
        #region 오디오 청크 관리
        
        /// <summary>
        /// 오디오 청크를 버퍼에 추가합니다
        /// </summary>
        /// <param name="sentenceId">문장 ID</param>
        /// <param name="audioClip">오디오 클립</param>
        /// <param name="npcText">NPC 텍스트</param>
        public void AddAudioChunk(int sentenceId, AudioClip audioClip, string npcText)
        {
            if (audioClip == null)
            {
                Debug.LogError($"[StreamingAudioPlayer] 오디오 클립이 null입니다: 문장 {sentenceId}");
                return;
            }
            
            // 버퍼 크기 체크
            if (audioBuffers.Count >= maxBufferSize)
            {
                Debug.LogWarning($"[StreamingAudioPlayer] 버퍼가 가득참 (최대 {maxBufferSize}개)");
                
                if (autoCleanup)
                {
                    CleanupOldestChunk();
                }
                else
                {
                    return;
                }
            }
            
            // 오디오 청크 생성 및 추가
            var chunk = new AudioChunk(sentenceId, audioClip, npcText);
            audioBuffers[sentenceId] = chunk;
            
            Debug.Log($"[StreamingAudioPlayer] 오디오 청크 추가: 문장 {sentenceId}, 길이 {chunk.duration:F2}초, 현재 버퍼: [{string.Join(", ", audioBuffers.Keys)}]");
            
            // 순차 재생 시도 또는 재개
            TryStartOrResumeSequentialPlayback();
        }
        
        /// <summary>
        /// 가장 오래된 청크를 정리합니다
        /// </summary>
        private void CleanupOldestChunk()
        {
            DateTime oldestTime = DateTime.MaxValue;
            int oldestId = -1;
            
            foreach (var kvp in audioBuffers)
            {
                if (kvp.Value.timestamp < oldestTime)
                {
                    oldestTime = kvp.Value.timestamp;
                    oldestId = kvp.Key;
                }
            }
            
            if (oldestId != -1)
            {
                RemoveAudioChunk(oldestId);
                Debug.Log($"[StreamingAudioPlayer] 가장 오래된 청크 정리: 문장 {oldestId}");
            }
        }
        
        /// <summary>
        /// 특정 오디오 청크를 제거합니다
        /// </summary>
        /// <param name="sentenceId">문장 ID</param>
        public bool RemoveAudioChunk(int sentenceId)
        {
            if (audioBuffers.TryGetValue(sentenceId, out AudioChunk chunk))
            {
                // 오디오 클립 해제
                if (chunk.audioClip != null)
                {
                    DestroyImmediate(chunk.audioClip);
                }
                
                audioBuffers.Remove(sentenceId);
                
                Debug.Log($"[StreamingAudioPlayer] 오디오 청크 제거: 문장 {sentenceId}");
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region 순차 재생 제어
        
        /// <summary>
        /// 순차 재생을 시작하거나 재개합니다
        /// </summary>
        private void TryStartOrResumeSequentialPlayback()
        {
            // 다음 재생할 문장이 버퍼에 있는지 확인
            if (audioBuffers.ContainsKey(NextSentenceToPlay))
            {
                if (IsSequentialPlayback)
                {
                    // 이미 순차 재생 중이면 재개 신호만 로그
                    Debug.Log($"[StreamingAudioPlayer] 순차 재생 중 - 새 청크 도착으로 재개 가능: 문장 {NextSentenceToPlay}");
                }
                else
                {
                    // 순차 재생이 중지된 상태면 새로 시작
                    Debug.Log($"[StreamingAudioPlayer] 순차 재생 시작: 문장 {NextSentenceToPlay}, 버퍼: {audioBuffers.Count}개");
                    
                    if (sequentialPlaybackCoroutine != null)
                    {
                        StopCoroutine(sequentialPlaybackCoroutine);
                    }
                    
                    sequentialPlaybackCoroutine = StartCoroutine(SequentialPlaybackCoroutine());
                }
            }
            else
            {
                Debug.Log($"[StreamingAudioPlayer] 문장 {NextSentenceToPlay} 대기 중 - 현재 버퍼: [{string.Join(", ", audioBuffers.Keys)}]");
            }
        }
        
        /// <summary>
        /// 순차 재생을 시도합니다 (기존 호환성용)
        /// </summary>
        private void TryStartSequentialPlayback()
        {
            TryStartOrResumeSequentialPlayback();
        }
        
        /// <summary>
        /// 순차 재생 코루틴
        /// </summary>
        /// <returns>코루틴 IEnumerator</returns>
        private IEnumerator SequentialPlaybackCoroutine()
        {
            IsSequentialPlayback = true;
            OnPlaybackStateChanged?.Invoke(true);
            
            Debug.Log($"[StreamingAudioPlayer] 순차 재생 코루틴 시작 - 다음 문장: {NextSentenceToPlay}");
            
            // 연속적으로 재생 가능한 모든 청크들을 재생
            while (audioBuffers.ContainsKey(NextSentenceToPlay))
            {
                AudioChunk chunk = audioBuffers[NextSentenceToPlay];
                
                Debug.Log($"[StreamingAudioPlayer] 문장 {NextSentenceToPlay} 버퍼에서 가져옴, 길이: {chunk.duration:F2}초");
                
                // 버퍼에서 제거
                audioBuffers.Remove(NextSentenceToPlay);
                
                if (chunk.audioClip != null)
                {
                    Debug.Log($"[StreamingAudioPlayer] 재생 시작: 문장 {chunk.sentenceId}, 텍스트: '{chunk.npcText}', 길이: {chunk.duration:F2}초");
                    
                    // 현재 재생 중인 청크 설정
                    CurrentPlayingChunk = chunk;
                    currentlyPlayingClip = chunk.audioClip;
                    IsPlaying = true;
                    
                    // 재생 시작 이벤트
                    OnAudioChunkStarted?.Invoke(chunk);
                    
                    // 오디오 재생
                    audioSource.clip = chunk.audioClip;
                    audioSource.Play();
                    
                    Debug.Log($"[StreamingAudioPlayer] AudioSource.Play() 호출됨 - isPlaying: {audioSource.isPlaying}");
                    
                    // 재생 완료까지 대기
                    yield return new WaitForSeconds(chunk.duration);
                    
                    Debug.Log($"[StreamingAudioPlayer] 문장 {chunk.sentenceId} 재생 대기 완료");
                    
                    // 청크 간 간격
                    if (chunkInterval > 0)
                    {
                        yield return new WaitForSeconds(chunkInterval);
                    }
                    
                    // 재생 완료 이벤트
                    OnAudioChunkCompleted?.Invoke(chunk);
                    
                    Debug.Log($"[StreamingAudioPlayer] 재생 완료: 문장 {chunk.sentenceId}");
                    
                    // 오디오 클립 해제
                    if (currentlyPlayingClip != null)
                    {
                        DestroyImmediate(currentlyPlayingClip);
                        currentlyPlayingClip = null;
                    }
                    
                    // 총 재생 수 증가
                    TotalPlayedChunks++;
                    IsPlaying = false;
                }
                else
                {
                    Debug.LogWarning($"[StreamingAudioPlayer] 오디오 클립이 null입니다: 문장 {NextSentenceToPlay}");
                }
                
                NextSentenceToPlay++;
                Debug.Log($"[StreamingAudioPlayer] 다음 재생할 문장 ID 증가: {NextSentenceToPlay}");
                
                // 다음 문장이 이미 버퍼에 있는지 확인
                if (audioBuffers.ContainsKey(NextSentenceToPlay))
                {
                    Debug.Log($"[StreamingAudioPlayer] 다음 문장 {NextSentenceToPlay} 버퍼에 있음 - 연속 재생 계속");
                    yield return null; // 한 프레임 대기
                }
                else
                {
                    Debug.Log($"[StreamingAudioPlayer] 다음 문장 {NextSentenceToPlay} 버퍼에 없음 - 현재 버퍼: [{string.Join(", ", audioBuffers.Keys)}]");
                }
            }
            
            // 순차 재생 완료
            CurrentPlayingChunk = null;
            IsSequentialPlayback = false;
            IsPlaying = false;
            OnPlaybackStateChanged?.Invoke(false);
            
            Debug.Log($"[StreamingAudioPlayer] 순차 재생 코루틴 완료. 총 재생 청크: {TotalPlayedChunks}, 남은 버퍼: {audioBuffers.Count}개");
            
            sequentialPlaybackCoroutine = null;
            
            // 버퍼에 재생 가능한 청크가 있는지 확인하고 재시작
            if (audioBuffers.ContainsKey(NextSentenceToPlay))
            {
                Debug.Log($"[StreamingAudioPlayer] 추가 청크 발견 - 순차 재생 재시작: 문장 {NextSentenceToPlay}");
                yield return null; // 한 프레임 대기 후 재시작
                TryStartOrResumeSequentialPlayback();
            }
            else if (audioBuffers.Count == 0)
            {
                Debug.Log($"[StreamingAudioPlayer] 모든 오디오 재생 완료");
                OnAllAudioCompleted?.Invoke(TotalPlayedChunks);
            }
            else
            {
                Debug.Log($"[StreamingAudioPlayer] 버퍼에 남은 청크가 있지만 순서가 맞지 않음: 다음 예상={NextSentenceToPlay}, 버퍼=[{string.Join(", ", audioBuffers.Keys)}]");
            }
        }
        
        #endregion
        
        #region 재생 제어
        
        /// <summary>
        /// 모든 오디오 재생을 중지합니다
        /// </summary>
        public void StopAllAudio()
        {
            // 순차 재생 코루틴 중지
            if (sequentialPlaybackCoroutine != null)
            {
                StopCoroutine(sequentialPlaybackCoroutine);
                sequentialPlaybackCoroutine = null;
            }
            
            // 오디오 소스 중지
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
            
            // 현재 재생 중인 클립 해제
            if (currentlyPlayingClip != null)
            {
                DestroyImmediate(currentlyPlayingClip);
                currentlyPlayingClip = null;
            }
            
            // 상태 초기화
            CurrentPlayingChunk = null;
            IsSequentialPlayback = false;
            IsPlaying = false;
            
            // 🔧 핵심 수정: 다음 재생할 문장 ID 리셋
            NextSentenceToPlay = 1;
            Debug.Log($"[StreamingAudioPlayer] NextSentenceToPlay 리셋: {NextSentenceToPlay}");
            
            OnPlaybackStateChanged?.Invoke(false);
            
            Debug.Log("[StreamingAudioPlayer] 모든 오디오 재생 중지됨");
        }
        
        /// <summary>
        /// 현재 재생 중인 오디오를 일시정지합니다
        /// </summary>
        public void PauseAudio()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log("[StreamingAudioPlayer] 오디오 일시정지");
            }
        }
        
        /// <summary>
        /// 일시정지된 오디오를 재개합니다
        /// </summary>
        public void ResumeAudio()
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
                Debug.Log("[StreamingAudioPlayer] 오디오 재개");
            }
        }
        
        #endregion
        
        #region 시스템 초기화
        
        /// <summary>
        /// 오디오 시스템을 초기화합니다
        /// </summary>
        public void ResetAudioSystem()
        {
            // 모든 재생 중지
            StopAllAudio();
            
            // 버퍼 정리
            ClearAllBuffers();
            
            // 상태 초기화
            NextSentenceToPlay = 1;
            TotalPlayedChunks = 0;
            
            Debug.Log("[StreamingAudioPlayer] 오디오 시스템 초기화 완료");
        }
        
        /// <summary>
        /// 모든 버퍼를 정리합니다
        /// </summary>
        public void ClearAllBuffers()
        {
            // 버퍼에 있는 모든 오디오 클립 해제
            foreach (var kvp in audioBuffers)
            {
                if (kvp.Value.audioClip != null)
                {
                    DestroyImmediate(kvp.Value.audioClip);
                }
            }
            
            audioBuffers.Clear();
            
            Debug.Log("[StreamingAudioPlayer] 모든 버퍼 정리 완료");
        }
        
        #endregion
        
        #region 리소스 정리
        
        /// <summary>
        /// 모든 리소스를 정리합니다
        /// </summary>
        private void CleanupResources()
        {
            // 모든 재생 중지
            StopAllAudio();
            
            // 버퍼 정리
            ClearAllBuffers();
        }
        
        #endregion
        
        #region 공개 유틸리티 메서드
        
        /// <summary>
        /// 오디오 시스템 상태 정보를 가져옵니다
        /// </summary>
        /// <returns>상태 정보 문자열</returns>
        public string GetAudioSystemStatus()
        {
            if (IsSequentialPlayback)
            {
                var current = CurrentPlayingChunk;
                if (current.HasValue)
                {
                    return $"재생 중: 문장 {current.Value.sentenceId} ({BufferedChunkCount}개 버퍼됨)";
                }
                else
                {
                    return $"순차 재생 중 ({BufferedChunkCount}개 버퍼됨)";
                }
            }
            
            if (BufferedChunkCount > 0)
            {
                return $"대기 중: {BufferedChunkCount}개 청크 버퍼됨, 다음 재생: 문장 {NextSentenceToPlay}";
            }
            
            return "준비됨";
        }
        
        /// <summary>
        /// 버퍼된 청크 목록을 가져옵니다
        /// </summary>
        /// <returns>문장 ID 배열</returns>
        public int[] GetBufferedChunkIds()
        {
            var ids = new int[audioBuffers.Count];
            audioBuffers.Keys.CopyTo(ids, 0);
            Array.Sort(ids);
            return ids;
        }
        
        /// <summary>
        /// 특정 문장이 버퍼에 있는지 확인합니다
        /// </summary>
        /// <param name="sentenceId">문장 ID</param>
        /// <returns>버퍼에 있으면 true</returns>
        public bool HasBufferedChunk(int sentenceId)
        {
            return audioBuffers.ContainsKey(sentenceId);
        }
        
        /// <summary>
        /// 다음 재생할 문장 ID를 설정합니다
        /// </summary>
        /// <param name="sentenceId">문장 ID</param>
        public void SetNextSentenceToPlay(int sentenceId)
        {
            if (IsSequentialPlayback)
            {
                Debug.LogWarning("[StreamingAudioPlayer] 재생 중에는 다음 문장 ID를 변경할 수 없습니다.");
                return;
            }
            
            NextSentenceToPlay = sentenceId;
            Debug.Log($"[StreamingAudioPlayer] 다음 재생할 문장 ID 설정: {sentenceId}");
        }
        
        #endregion
    }
} 