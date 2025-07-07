using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TTSSystem;

/// <summary>
/// TTS 스트리밍 처리를 위한 DownloadHandler 클래스들
/// 
/// 역할:
/// - 서버로부터 실시간으로 전송되는 데이터를 처리
/// - 스트리밍 데이터를 파싱하여 완전한 JSON 라인으로 변환
/// - 비동기 처리를 통해 게임 성능에 미치는 영향을 최소화
/// 
/// 스트리밍 데이터 형식:
/// ```
/// data: {"type": "text", "sentence_id": 1, "text": "안녕하세요"}
/// data: {"type": "audio", "sentence_id": 1, "audio_data": "base64..."}
/// data: {"type": "complete", "total_sentences": 5}
/// ```
/// </summary>
namespace TTSSystem
{
    /// <summary>
    /// 실시간 스트리밍 처리를 위한 기본 DownloadHandler
    /// 
    /// 특징:
    /// - 코루틴을 사용한 동기적 처리
    /// - 안정적이지만 게임 성능에 영향을 줄 수 있음
    /// - 간단한 용도나 디버깅에 적합
    /// 
    /// 동작 과정:
    /// 1. 서버에서 데이터 수신
    /// 2. 버퍼에 데이터 누적
    /// 3. 완전한 라인이 만들어지면 처리
    /// 4. JSON 파싱 및 TTS 클라이언트로 전달
    /// </summary>
    public class StreamingDownloadHandler : DownloadHandlerScript
    {
        #region Private Fields
        
        /// <summary>
        /// TTS 클라이언트 참조
        /// 처리된 데이터를 전달하기 위해 사용
        /// </summary>
        private UnityRealtimeTTSClient ttsClient;
        
        /// <summary>
        /// 수신된 데이터를 임시 저장하는 버퍼
        /// 불완전한 데이터가 들어올 때 다음 데이터와 합쳐서 처리
        /// </summary>
        private StringBuilder buffer = new StringBuilder();
        
        /// <summary>
        /// 현재 처리 중인지 확인하는 플래그
        /// 중복 처리를 방지하기 위해 사용
        /// </summary>
        private bool isProcessing = false;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// StreamingDownloadHandler 생성자
        /// </summary>
        /// <param name="client">데이터를 전달할 TTS 클라이언트</param>
        public StreamingDownloadHandler(UnityRealtimeTTSClient client) : base()
        {
            ttsClient = client;
        }
        
        #endregion
        
        #region Data Reception
        
        /// <summary>
        /// 서버로부터 데이터를 수신할 때 호출되는 메서드
        /// Unity의 UnityWebRequest 시스템에서 자동으로 호출
        /// </summary>
        /// <param name="data">수신된 바이트 데이터</param>
        /// <param name="dataLength">데이터 길이</param>
        /// <returns>처리 성공 여부</returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            // 유효하지 않은 데이터 체크
            if (data == null || dataLength == 0)
                return false;
            
            // 바이트 데이터를 문자열로 변환
            string receivedText = Encoding.UTF8.GetString(data, 0, dataLength);
            
            // 버퍼에 추가
            buffer.Append(receivedText);
            
            // 아직 처리 중이 아니면 처리 시작
            if (!isProcessing)
            {
                ttsClient.StartCoroutine(ProcessBufferedData());
            }
            
            return true;
        }
        
        #endregion
        
        #region Data Processing
        
        /// <summary>
        /// 버퍼에 누적된 데이터를 처리하는 코루틴
        /// 
        /// 처리 과정:
        /// 1. 버퍼에서 완전한 라인 찾기
        /// 2. JSON 유효성 검사
        /// 3. 유효한 데이터만 TTS 클라이언트로 전달
        /// 4. 처리된 데이터는 버퍼에서 제거
        /// </summary>
        /// <returns>코루틴 열거자</returns>
        private IEnumerator ProcessBufferedData()
        {
            isProcessing = true;
            
            while (true)
            {
                string currentBuffer = buffer.ToString();
                
                // 완전한 라인 찾기 (data: 로 시작하고 \n으로 끝나는)
                int dataStart = currentBuffer.IndexOf("data: ");
                if (dataStart == -1)
                    break; // "data: "가 없으면 종료
                
                int lineEnd = currentBuffer.IndexOf('\n', dataStart);
                if (lineEnd == -1)
                    break; // 완전한 라인이 아직 없으면 종료
                
                // 완전한 라인 추출
                string completeLine = currentBuffer.Substring(dataStart, lineEnd - dataStart);
                
                // 버퍼에서 처리된 부분 제거
                buffer.Remove(0, lineEnd + 1);
                
                // JSON 유효성 검사 후 처리
                if (IsValidJsonLine(completeLine))
                {
                    // TTS 클라이언트에서 처리
                    ttsClient.StartCoroutine(ttsClient.ProcessSingleStreamLine(completeLine));
                }
                else
                {
                    // 유효하지 않은 데이터는 로그만 출력
                    ttsClient.DebugLog($"불완전한 JSON 라인 스킵: {completeLine.Substring(0, Math.Min(100, completeLine.Length))}...");
                }
                
                // 한 프레임 대기 (다른 작업들이 실행될 수 있도록)
                yield return null;
            }
            
            isProcessing = false;
        }
        
        /// <summary>
        /// JSON 라인의 유효성을 검사하는 메서드
        /// 
        /// 검사 항목:
        /// 1. "data: "로 시작하는지 확인
        /// 2. 중괄호 개수가 맞는지 확인
        /// 3. 기본적인 JSON 구조 검사
        /// </summary>
        /// <param name="line">검사할 라인</param>
        /// <returns>유효한 JSON이면 true, 아니면 false</returns>
        private bool IsValidJsonLine(string line)
        {
            if (!line.StartsWith("data: "))
                return false;
            
            try
            {
                // "data: " 부분 제거
                string jsonPart = line.Substring(6);
                
                // 기본적인 JSON 구조 검사
                if (jsonPart.Trim().StartsWith("{") && jsonPart.Trim().EndsWith("}"))
                {
                    // 중괄호 개수 확인
                    int openBraces = 0;
                    int closeBraces = 0;
                    
                    foreach (char c in jsonPart)
                    {
                        if (c == '{') openBraces++;
                        if (c == '}') closeBraces++;
                    }
                    
                    // 중괄호 개수가 맞고 최소 1개 이상이면 유효
                    return openBraces == closeBraces && openBraces > 0;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Completion Handling
        
        /// <summary>
        /// 모든 데이터 수신이 완료되었을 때 호출되는 메서드
        /// 남은 버퍼 데이터를 처리합니다
        /// </summary>
        protected override void CompleteContent()
        {
            // 남은 버퍼가 있고 처리 중이 아니면 마지막 처리 실행
            if (buffer.Length > 0 && !isProcessing)
            {
                ttsClient.StartCoroutine(ProcessBufferedData());
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 비동기 스트리밍 처리를 위한 개선된 DownloadHandler
    /// 
    /// 특징:
    /// - Task 기반 비동기 처리
    /// - 게임 성능에 미치는 영향 최소화
    /// - 백그라운드에서 데이터 처리
    /// - 메인 스레드 디스패처를 통한 Unity API 호출
    /// 
    /// 장점:
    /// - 게임 프레임 드롭 방지
    /// - 더 나은 반응성
    /// - 확장성 있는 구조
    /// 
    /// 사용 시나리오:
    /// - 대용량 데이터 처리
    /// - 실시간 성능이 중요한 경우
    /// - 복잡한 비동기 로직이 필요한 경우
    /// </summary>
    public class AsyncStreamingDownloadHandler : DownloadHandlerScript
    {
        #region Private Fields
        
        /// <summary>
        /// TTS 클라이언트 참조
        /// 처리된 데이터를 전달하기 위해 사용
        /// </summary>
        private UnityRealtimeTTSClient ttsClient;
        
        /// <summary>
        /// 수신된 데이터를 임시 저장하는 버퍼
        /// 스레드 안전성을 고려하여 사용
        /// </summary>
        private StringBuilder buffer = new StringBuilder();
        
        /// <summary>
        /// 현재 처리 중인지 확인하는 플래그
        /// 여러 비동기 작업의 중복 실행을 방지
        /// </summary>
        private bool isProcessing = false;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// AsyncStreamingDownloadHandler 생성자
        /// </summary>
        /// <param name="client">데이터를 전달할 TTS 클라이언트</param>
        public AsyncStreamingDownloadHandler(UnityRealtimeTTSClient client) : base()
        {
            ttsClient = client;
        }
        
        #endregion
        
        #region Data Reception
        
        /// <summary>
        /// 서버로부터 데이터를 수신할 때 호출되는 메서드
        /// 비동기 처리를 통해 빠른 응답성을 제공
        /// </summary>
        /// <param name="data">수신된 바이트 데이터</param>
        /// <param name="dataLength">데이터 길이</param>
        /// <returns>처리 성공 여부</returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            // 유효하지 않은 데이터 체크
            if (data == null || dataLength == 0)
                return false;
            
            // 바이트 데이터를 문자열로 변환
            string receivedText = Encoding.UTF8.GetString(data, 0, dataLength);
            
            // 버퍼에 추가
            buffer.Append(receivedText);
            
            // 아직 처리 중이 아니면 비동기 처리 시작
            if (!isProcessing)
            {
                // Task.Run을 사용해 백그라운드에서 처리
                // await를 사용하지 않고 Fire-and-Forget 방식으로 실행
                _ = Task.Run(async () => await ProcessBufferedDataAsync());
            }
            
            return true;
        }
        
        #endregion
        
        #region Async Data Processing
        
        /// <summary>
        /// 버퍼에 누적된 데이터를 비동기로 처리하는 메서드
        /// 
        /// 비동기 처리 장점:
        /// - 메인 스레드 블로킹 방지
        /// - 더 나은 성능과 반응성
        /// - 복잡한 처리 로직도 원활하게 실행
        /// 
        /// 처리 과정:
        /// 1. 백그라운드에서 데이터 파싱
        /// 2. JSON 유효성 검사
        /// 3. 메인 스레드 디스패처를 통해 Unity API 호출
        /// 4. 최소한의 지연으로 CPU 사용량 최적화
        /// </summary>
        /// <returns>비동기 작업</returns>
        private async Task ProcessBufferedDataAsync()
        {
            isProcessing = true;
            
            while (true)
            {
                string currentBuffer = buffer.ToString();
                
                // 완전한 라인 찾기 (data: 로 시작하고 \n으로 끝나는)
                int dataStart = currentBuffer.IndexOf("data: ");
                if (dataStart == -1)
                    break; // "data: "가 없으면 종료
                
                int lineEnd = currentBuffer.IndexOf('\n', dataStart);
                if (lineEnd == -1)
                    break; // 완전한 라인이 아직 없으면 종료
                
                // 완전한 라인 추출
                string completeLine = currentBuffer.Substring(dataStart, lineEnd - dataStart);
                
                // 버퍼에서 처리된 부분 제거
                buffer.Remove(0, lineEnd + 1);
                
                // JSON 유효성 검사
                if (IsValidJsonLine(completeLine))
                {
                    // 메인 스레드에서 처리되도록 큐에 추가
                    UnityMainThreadDispatcher.Instance().Enqueue(() => 
                    {
                        ttsClient.StartCoroutine(ttsClient.ProcessSingleStreamLineAsync(completeLine));
                    });
                }
                else
                {
                    // 유효하지 않은 데이터는 로그만 출력 (메인 스레드에서)
                    UnityMainThreadDispatcher.Instance().Enqueue(() => 
                    {
                        ttsClient.DebugLog($"불완전한 JSON 라인 스킵: {completeLine.Substring(0, Math.Min(100, completeLine.Length))}...");
                    });
                }
                
                // 1ms 대기로 CPU 사용량 최소화
                // 너무 빠른 처리로 인한 CPU 과부하 방지
                await Task.Delay(1);
            }
            
            isProcessing = false;
        }
        
        /// <summary>
        /// JSON 라인의 유효성을 검사하는 메서드
        /// 기본 StreamingDownloadHandler와 동일한 로직
        /// </summary>
        /// <param name="line">검사할 라인</param>
        /// <returns>유효한 JSON이면 true, 아니면 false</returns>
        private bool IsValidJsonLine(string line)
        {
            if (!line.StartsWith("data: "))
                return false;
            
            try
            {
                // "data: " 부분 제거
                string jsonPart = line.Substring(6);
                
                // 기본적인 JSON 구조 검사
                if (jsonPart.Trim().StartsWith("{") && jsonPart.Trim().EndsWith("}"))
                {
                    // 중괄호 개수 확인
                    int openBraces = 0;
                    int closeBraces = 0;
                    
                    foreach (char c in jsonPart)
                    {
                        if (c == '{') openBraces++;
                        if (c == '}') closeBraces++;
                    }
                    
                    // 중괄호 개수가 맞고 최소 1개 이상이면 유효
                    return openBraces == closeBraces && openBraces > 0;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Completion Handling
        
        /// <summary>
        /// 모든 데이터 수신이 완료되었을 때 호출되는 메서드
        /// 남은 버퍼 데이터를 비동기로 처리합니다
        /// </summary>
        protected override void CompleteContent()
        {
            // 남은 버퍼가 있고 처리 중이 아니면 마지막 처리 실행
            if (buffer.Length > 0 && !isProcessing)
            {
                // 비동기로 마지막 처리 실행
                _ = Task.Run(async () => await ProcessBufferedDataAsync());
            }
        }
        
        #endregion
    }
} 