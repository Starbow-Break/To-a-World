using System;
using System.Collections.Generic;

/// <summary>
/// TTS(Text-to-Speech) 시스템에서 사용하는 데이터 모델들을 정의합니다.
/// 서버와의 통신에 사용되는 요청/응답 데이터 구조를 포함합니다.
/// </summary>
namespace TTSSystem
{
    /// <summary>
    /// 실시간 TTS 요청을 위한 데이터 모델
    /// 서버에 음성 합성을 요청할 때 사용되는 파라미터들을 정의합니다.
    /// </summary>
    [Serializable]
    public class TTSRealtimeRequest
    {
        /// <summary>
        /// 음성으로 변환할 텍스트
        /// 예: "안녕하세요, 반가워요!"
        /// </summary>
        public string text;

        /// <summary>
        /// 시스템 프롬프트 (선택적)
        /// AI가 응답을 생성할 때 참고할 지침이나 컨텍스트
        /// 예: "친근하고 도움이 되는 톤으로 응답해주세요"
        /// </summary>
        public string system_prompt;

        /// <summary>
        /// 음성 합성에 사용할 언어
        /// 기본값: "en" (영어)
        /// 지원 언어: "ko" (한국어), "en" (영어), "ja" (일본어) 등
        /// </summary>
        public string language = "en";

        /// <summary>
        /// AI 사고 과정 사용 여부
        /// true: AI가 더 깊이 생각해서 응답 생성
        /// false: 빠른 응답 생성
        /// 기본값: false
        /// </summary>
        public bool use_thinking = false;

        /// <summary>
        /// 캐릭터 이름
        /// 특정 캐릭터의 음성 스타일을 사용할 때 지정
        /// 예: "friendly_assistant", "professional_guide", "casual_friend"
        /// </summary>
        public string character_name;

        /// <summary>
        /// 캐릭터 성격 (선택적)
        /// 음성의 성격적 특성을 지정
        /// 예: "밝고 긍정적인", "차분하고 전문적인", "유머러스하고 친근한"
        /// </summary>
        public string personality;

        /// <summary>
        /// 말하기 스타일 (선택적)
        /// 음성의 말하기 방식을 지정
        /// 예: "천천히 명확하게", "빠르고 활기차게", "부드럽고 따뜻하게"
        /// </summary>
        public string speaking_style;
    }

    /// <summary>
    /// 스트리밍 응답 데이터 모델
    /// 서버에서 실시간으로 전송되는 응답 데이터의 구조를 정의합니다.
    /// 서버는 여러 개의 이런 데이터를 순차적으로 전송합니다.
    /// </summary>
    [Serializable]
    public class StreamingResponseData
    {
        /// <summary>
        /// 응답 데이터의 타입
        /// 가능한 값:
        /// - "metadata": 메타데이터 (언어, 캐릭터 정보 등)
        /// - "text": 생성된 텍스트
        /// - "audio": 오디오 데이터
        /// - "complete": 전체 처리 완료
        /// - "error": 오류 발생
        /// </summary>
        public string type;

        /// <summary>
        /// 문장 ID
        /// 각 문장에 대한 고유 식별자
        /// 순차적 재생을 위해 사용됩니다 (1, 2, 3, ...)
        /// </summary>
        public int sentence_id;

        /// <summary>
        /// 생성된 텍스트 내용
        /// type이 "text"일 때 사용됩니다
        /// </summary>
        public string text;

        /// <summary>
        /// Base64로 인코딩된 오디오 데이터
        /// type이 "audio"일 때 사용됩니다
        /// WAV 형식의 오디오 데이터가 Base64로 인코딩되어 전송됩니다
        /// </summary>
        public string audio_data;

        /// <summary>
        /// 오디오 데이터의 바이트 길이
        /// 디코딩 후 예상되는 오디오 데이터 크기입니다
        /// </summary>
        public int audio_length;

        /// <summary>
        /// 타임스탬프
        /// 데이터가 생성된 시간 (Unix 타임스탬프)
        /// </summary>
        public double timestamp;

        /// <summary>
        /// 사용된 언어
        /// 실제로 음성 합성에 사용된 언어 코드
        /// </summary>
        public string language;

        /// <summary>
        /// 사용된 캐릭터
        /// 실제로 음성 합성에 사용된 캐릭터 이름
        /// </summary>
        public string character;

        /// <summary>
        /// 전체 문장 개수
        /// type이 "complete"일 때 사용됩니다
        /// 총 몇 개의 문장이 처리되었는지 나타냅니다
        /// </summary>
        public int total_sentences;

        /// <summary>
        /// 오류 메시지
        /// type이 "error"일 때 사용됩니다
        /// 발생한 오류에 대한 상세 정보를 포함합니다
        /// </summary>
        public string error;
    }

    /// <summary>
    /// TTS 시스템에서 사용하는 상수들을 정의합니다
    /// </summary>
    public static class TTSConstants
    {
        /// <summary>
        /// 지원하는 언어 코드들
        /// </summary>
        public static class Languages
        {
            public const string English = "en";
            public const string Korean = "ko";
            public const string Japanese = "ja";
            public const string Chinese = "zh";
            public const string Spanish = "es";
            public const string French = "fr";
            public const string German = "de";
            
            public static List<string> GetValues() => new List<string>
            {
                English,
                Korean,
                Japanese,
                Chinese,
                Spanish,
                French,
                German
            };
        }

        /// <summary>
        /// 기본 캐릭터 이름들
        /// </summary>
        public static class Characters
        {
            public const string FriendlyAssistant = "friendly_assistant";
            public const string ProfessionalGuide = "professional_guide";
            public const string CasualFriend = "casual_friend";
            public const string WiseTeacher = "wise_teacher";
            public const string EnergeticHost = "energetic_host";

            public static List<string> GetValues() => new List<string>
            {
                FriendlyAssistant,
                ProfessionalGuide,
                CasualFriend,
                WiseTeacher,
                EnergeticHost
            };
        }

        /// <summary>
        /// 스트리밍 응답 타입들
        /// </summary>
        public static class ResponseTypes
        {
            public const string Metadata = "metadata";
            public const string Text = "text";
            public const string Audio = "audio";
            public const string Complete = "complete";
            public const string Error = "error";
        }

        /// <summary>
        /// 기본 설정 값들
        /// </summary>
        public static class Defaults
        {
            public const string Language = Languages.English;
            public const string Character = Characters.FriendlyAssistant;
            public const int MaxConcurrentAudio = 3;
            public const int AsyncProcessingInterval = 1;
        }
    }
} 