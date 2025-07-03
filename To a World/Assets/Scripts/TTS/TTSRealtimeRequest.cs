using System;

[Serializable]
public class TTSRealtimeRequest
{
    public string text;
    public string system_prompt;
    public string language = "en";
    public bool use_thinking = false;
    public string character_name;
    public string personality;
    public string speaking_style;
}