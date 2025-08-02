using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPCSystem
{
    /// <summary>
    /// NPC 성별 열거형
    /// </summary>
    public enum NPCGender
    {
        MALE,
        FEMALE,
        OTHER
    }

    /// <summary>
    /// 퀘스트 상태 열거형
    /// </summary>
    public enum QuestStatus
    {
        NOT_STARTED,
        ACTIVE,
        IN_PROGRESS,
        COMPLETED,
        FAILED,
        ABANDONED
    }

    /// <summary>
    /// 대화 발신자 타입 열거형
    /// </summary>
    public enum ConversationSenders
    {
        PLAYER,
        NPC,
        USER,
        ASSISTANT,
        SYSTEM
    }

    /// <summary>
    /// 응답 타입 상수들
    /// </summary>
    public static class ResponseTypes
    {
        public const string NPC_METADATA = "npc_metadata";
        public const string NPC_TEXT = "npc_text";
        public const string NPC_AUDIO = "npc_audio";
        public const string NPC_COMPLETE = "npc_complete";
        public const string NPC_ERROR = "npc_error";
    }

    /// <summary>
    /// NPC 정보 클래스
    /// </summary>
    [Serializable]
    public class NPCInfo
    {
        public string name;
        public NPCGender gender = NPCGender.FEMALE;
        public string personality = "친근하고 도움이 되는";
        public string background = "게임 세계의 NPC";
        public int? age = 25;
        public string voice_style = "자연스러운";
    }

    /// <summary>
    /// 퀘스트 정보 클래스
    /// </summary>
    [Serializable]
    public class QuestInfo
    {
        public string id;
        public string name;
        public string description;
        public string completion_condition;
        public string completion_dialogue;
        public string reward;
        public string difficulty = "normal";
        public QuestStatus status = QuestStatus.ACTIVE;
    }

    /// <summary>
    /// 대화 메시지 클래스
    /// </summary>
    [Serializable]
    public class ConversationMessage
    {
        public ConversationSenders sender;
        public string message;
        public double timestamp;
        public string quest_id;

        public ConversationMessage(ConversationSenders sender, string message, string questId = null)
        {
            this.sender = sender;
            this.message = message;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.quest_id = questId;
        }
    }

    /// <summary>
    /// NPC 음성 대화 요청 클래스
    /// </summary>
    [Serializable]
    public class NPCAudioRequest
    {
        public string audio_data;                        // Base64 인코딩된 WAV 오디오
        public NPCInfo npc;                             // NPC 정보
        public QuestInfo quest;                         // 퀘스트 정보 (선택사항)
        public List<ConversationMessage> conversation_history = new List<ConversationMessage>();
        public string memory_key;                       // 기기 고유 ID
        public string language = "en";                  // 언어 코드 (서버와 일치)
        public bool use_thinking = false;               // AI 사고 과정 사용 여부
        public string audio_format = "wav";             // 오디오 포맷
    }

    /// <summary>
    /// NPC 선제 대화 요청 클래스
    /// </summary>
    [Serializable]
    public class NPCInitiateRequest
    {
        public NPCInfo npc;                             // NPC 정보
        public QuestInfo quest;                         // 퀘스트 정보 (선택사항)
        public string initial_message;                  // 상황이나 트리거
        public List<ConversationMessage> conversation_history = new List<ConversationMessage>();
        public string memory_key;                       // 기기 고유 ID
        public string language = "en";                  // 언어 코드 (서버와 일치)
        public bool use_thinking = false;               // AI 사고 과정 사용 여부
    }

    // ===== 응답 데이터 모델들 =====

    /// <summary>
    /// NPC 메타데이터 응답
    /// </summary>
    [Serializable]
    public class NPCMetadataResponse
    {
        public string type;
        public double timestamp;
        public string npc_name;
        public string quest_id;
        public string memory_key;
        public string transcribed_text;
        public string initial_message;
        public string interaction_type;
        public int original_audio_length;
    }

    /// <summary>
    /// NPC 텍스트 응답
    /// </summary>
    [Serializable]
    public class NPCTextResponse
    {
        public string type;
        public int sentence_id;
        public string npc_text;
        public double timestamp;
        public string npc_name;
        public float estimated_duration;
        public float pause_after;
    }

    /// <summary>
    /// NPC 오디오 응답
    /// </summary>
    [Serializable]
    public class NPCAudioResponse
    {
        public string type;
        public int sentence_id;
        public string npc_text;
        public string audio_data;           // Base64 인코딩된 오디오
        public int audio_length;
        public double timestamp;
        public string npc_name;
        public NPCGender npc_gender;
    }

    /// <summary>
    /// NPC 완료 응답
    /// </summary>
    [Serializable]
    public class NPCCompleteResponse
    {
        public string type;
        public int total_sentences;
        public bool quest_complete;
        public string npc_name;
        public string quest_id;
        public bool memory_updated;
        public string transcribed_text;
        public string interaction_type;
        public string initial_message;
        public double timestamp;
    }

    /// <summary>
    /// NPC 오류 응답
    /// </summary>
    [Serializable]
    public class NPCErrorResponse
    {
        public string type;
        public int sentence_id;
        public string error;
        public string error_code;
        public double timestamp;
        public string npc_name;
        public string transcribed_text;
        public string initial_message;
    }
    
    /// <summary>
    /// 메모리 삭제 완료 응답
    /// </summary>
    [Serializable]
    public class MemoryClearedResponse
    {
        public string type;
        public string action;
        public string memory_key;
        public bool success;
        public string reason;
        public double timestamp;
    }
} 