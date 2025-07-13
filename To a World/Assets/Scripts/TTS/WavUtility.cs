using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// WAV 오디오 파일 처리 유틸리티
/// 
/// Unity AudioClip과 WAV 바이트 데이터 간의 변환을 담당합니다.
/// NPC TTS 시스템에서 음성 데이터를 주고받을 때 사용됩니다.
/// 
/// 주요 기능:
/// - AudioClip을 WAV 바이트 배열로 변환
/// - WAV 바이트 배열을 AudioClip으로 변환
/// - 16비트 PCM, 1-2채널 지원
/// - 안정적인 오디오 품질 보장
/// </summary>
public static class WavUtility
{
    private const int HEADER_SIZE = 44;

    /// <summary>
    /// AudioClip을 WAV 바이트 배열로 변환 (개선된 방식)
    /// </summary>
    /// <param name="audioClip">변환할 AudioClip</param>
    /// <returns>WAV 형식의 바이트 배열</returns>
    public static byte[] FromAudioClip(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogError("[WavUtility] AudioClip이 null입니다.");
            return null;
        }

        try
        {
            // 오디오 데이터 추출
            float[] samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            // WAV 헤더 생성
            int sampleRate = audioClip.frequency;
            int channels = audioClip.channels;
            int bitDepth = 16;

            Debug.Log($"[WavUtility] 오디오 변환 시작: {sampleRate}Hz, {channels}채널, {samples.Length}샘플");

            using (MemoryStream stream = new MemoryStream())
            {
                // WAV 헤더 작성
                WriteWAVHeader(stream, samples.Length, sampleRate, channels, bitDepth);

                // 오디오 데이터 작성 (16비트 PCM)
                foreach (float sample in samples)
                {
                    float clampedSample = Mathf.Clamp(sample, -1.0f, 1.0f);
                    short intSample = (short)(clampedSample * short.MaxValue);
                    byte[] bytes = BitConverter.GetBytes(intSample);
                    stream.Write(bytes, 0, 2);
                }

                byte[] result = stream.ToArray();
                Debug.Log($"[WavUtility] 오디오 변환 완료: {result.Length} bytes");
                return result;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[WavUtility] AudioClip to WAV 변환 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// WAV 바이트 배열을 AudioClip으로 변환
    /// </summary>
    /// <param name="wavData">WAV 형식의 바이트 배열</param>
    /// <param name="name">생성될 AudioClip의 이름</param>
    /// <returns>생성된 AudioClip</returns>
    public static AudioClip ToAudioClip(byte[] wavData, string name = "GeneratedAudio")
    {
        if (wavData == null || wavData.Length < HEADER_SIZE)
        {
            Debug.LogError("[WavUtility] 유효하지 않은 WAV 데이터입니다.");
            return null;
        }

        try
        {
            // WAV 헤더 파싱
            WavHeader header = ParseWavHeader(wavData);
            
            if (!ValidateWavHeader(header))
            {
                Debug.LogError("[WavUtility] 지원하지 않는 WAV 형식입니다.");
                return null;
            }

            // PCM 데이터 추출 (헤더 이후 부분)
            int dataLength = wavData.Length - HEADER_SIZE;
            byte[] pcmData = new byte[dataLength];
            Array.Copy(wavData, HEADER_SIZE, pcmData, 0, dataLength);

            // 16비트 PCM을 float 배열로 변환
            float[] samples = ConvertPCM16ToFloat(pcmData);

            // AudioClip 생성
            AudioClip audioClip = AudioClip.Create(name, samples.Length / header.channels, header.channels, header.sampleRate, false);
            audioClip.SetData(samples, 0);

            Debug.Log($"[WavUtility] AudioClip 생성 완료: {name} ({header.sampleRate}Hz, {header.channels}채널, {samples.Length}샘플)");
            
            return audioClip;
        }
        catch (Exception e)
        {
            Debug.LogError($"[WavUtility] AudioClip 변환 중 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// WAV 헤더를 작성합니다 (개선된 방식)
    /// </summary>
    /// <param name="stream">출력 스트림</param>
    /// <param name="sampleCount">샘플 수</param>
    /// <param name="sampleRate">샘플링 레이트</param>
    /// <param name="channels">채널 수</param>
    /// <param name="bitDepth">비트 깊이</param>
    private static void WriteWAVHeader(MemoryStream stream, int sampleCount, int sampleRate, int channels, int bitDepth)
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



    /// <summary>
    /// 16비트 PCM을 float 배열로 변환
    /// </summary>
    private static float[] ConvertPCM16ToFloat(byte[] pcmData)
    {
        int sampleCount = pcmData.Length / 2; // 16비트 = 2바이트
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // 리틀 엔디안에서 16비트 정수 읽기
            short pcmValue = (short)((pcmData[i * 2 + 1] << 8) | pcmData[i * 2]);
            
            // 16비트 정수를 float으로 변환 (-1.0 ~ 1.0)
            samples[i] = pcmValue / 32768.0f;
        }

        return samples;
    }

    /// <summary>
    /// WAV 헤더 파싱
    /// </summary>
    private static WavHeader ParseWavHeader(byte[] wavData)
    {
        var header = new WavHeader();

        using (var stream = new MemoryStream(wavData))
        using (var reader = new BinaryReader(stream))
        {
            // RIFF 헤더 확인
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF")
            {
                throw new Exception("[WavUtility] 유효하지 않은 RIFF 헤더입니다.");
            }

            reader.ReadInt32(); // 파일 크기 스킵
            
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE")
            {
                throw new Exception("[WavUtility] 유효하지 않은 WAVE 헤더입니다.");
            }

            // fmt 청크 찾기
            string fmt = new string(reader.ReadChars(4));
            if (fmt != "fmt ")
            {
                throw new Exception("[WavUtility] fmt 청크를 찾을 수 없습니다.");
            }

            int fmtChunkSize = reader.ReadInt32();
            header.audioFormat = reader.ReadInt16();
            header.channels = reader.ReadInt16();
            header.sampleRate = reader.ReadInt32();
            header.byteRate = reader.ReadInt32();
            header.blockAlign = reader.ReadInt16();
            header.bitsPerSample = reader.ReadInt16();

            // data 청크로 이동
            if (fmtChunkSize > 16)
            {
                reader.ReadBytes(fmtChunkSize - 16); // 추가 데이터 스킵
            }

            string data = new string(reader.ReadChars(4));
            if (data != "data")
            {
                throw new Exception("[WavUtility] data 청크를 찾을 수 없습니다.");
            }

            header.dataSize = reader.ReadInt32();
        }

        return header;
    }

    /// <summary>
    /// WAV 헤더 유효성 검사
    /// </summary>
    private static bool ValidateWavHeader(WavHeader header)
    {
        // PCM 형식만 지원
        if (header.audioFormat != 1)
        {
            Debug.LogError($"[WavUtility] 지원하지 않는 오디오 형식: {header.audioFormat} (PCM만 지원)");
            return false;
        }

        // 16비트만 지원
        if (header.bitsPerSample != 16)
        {
            Debug.LogError($"[WavUtility] 지원하지 않는 비트 깊이: {header.bitsPerSample} (16비트만 지원)");
            return false;
        }

        // 1-2 채널만 지원
        if (header.channels < 1 || header.channels > 2)
        {
            Debug.LogError($"[WavUtility] 지원하지 않는 채널 수: {header.channels} (1-2채널만 지원)");
            return false;
        }

        return true;
    }

    /// <summary>
    /// WAV 헤더 정보 구조체
    /// </summary>
    private struct WavHeader
    {
        public short audioFormat;
        public short channels;
        public int sampleRate;
        public int byteRate;
        public short blockAlign;
        public short bitsPerSample;
        public int dataSize;
    }
} 