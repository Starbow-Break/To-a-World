using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class StreamingDownloadHandler : DownloadHandlerScript
{
    private UnityRealtimeTTSClient ttsClient;
    private StringBuilder buffer = new StringBuilder();
    private bool isProcessing = false;
    
    public StreamingDownloadHandler(UnityRealtimeTTSClient client)
    {
        ttsClient = client;
    }
    
    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0)
            return false;
            
        string receivedText = Encoding.UTF8.GetString(data, 0, dataLength);
        buffer.Append(receivedText);
        
        // 실시간으로 처리
        if (!isProcessing)
        {
            ttsClient.StartCoroutine(ProcessBufferedData());
        }
        
        return true;
    }
    
    private IEnumerator ProcessBufferedData()
    {
        isProcessing = true;
        
        while (true)
        {
            string currentBuffer = buffer.ToString();
            
            // 완전한 라인 찾기 (data: 로 시작하고 \n으로 끝나는)
            int dataStart = currentBuffer.IndexOf("data: ");
            if (dataStart == -1)
                break;
                
            int lineEnd = currentBuffer.IndexOf('\n', dataStart);
            if (lineEnd == -1)
                break; // 완전한 라인이 아직 없음
            
            // 완전한 라인 추출
            string completeLine = currentBuffer.Substring(dataStart, lineEnd - dataStart);
            
            // 버퍼에서 처리된 부분 제거
            buffer.Remove(0, lineEnd + 1);
            
            // JSON 유효성 검사
            if (IsValidJsonLine(completeLine))
            {
                yield return ttsClient.StartCoroutine(ttsClient.ProcessSingleStreamLine(completeLine));
            }
            else
            {
                ttsClient.DebugLog($"불완전한 JSON 라인 스킵: {completeLine.Substring(0, Math.Min(100, completeLine.Length))}...");
            }
            
            yield return null;
        }
        
        isProcessing = false;
    }
    
    private bool IsValidJsonLine(string line)
    {
        if (!line.StartsWith("data: "))
            return false;
            
        try
        {
            string jsonPart = line.Substring(6);
            // 간단한 JSON 유효성 검사
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
                return openBraces == closeBraces && openBraces > 0;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    protected override void CompleteContent()
    {
        // 남은 버퍼 처리
        if (buffer.Length > 0 && !isProcessing)
        {
            ttsClient.StartCoroutine(ProcessBufferedData());
        }
    }
}