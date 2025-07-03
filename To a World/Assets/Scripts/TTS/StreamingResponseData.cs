using System;

[Serializable]
public class StreamingResponseData
{
    public string type;
    public int sentence_id;
    public string text;
    public string audio_data;
    public int audio_length;
    public double timestamp;
    public string language;
    public string character;
    public int total_sentences;
    public string error;
}
