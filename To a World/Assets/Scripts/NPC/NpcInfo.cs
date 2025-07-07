using TTSSystem;

public class NpcInfo
{
    public string Language { get; private set; }
    public string Character { get; private set; }

    public NpcInfo(string language, string characters)
    {
        Language = TTSConstants.Defaults.Language;
        Character = TTSConstants.Defaults.Character;
        
        foreach (var lan in TTSConstants.Languages.GetValues())
        {
            if (lan == language)
            {
                Language = lan;
                break;
            }
        }
        
        foreach (var chara in TTSConstants.Characters.GetValues())
        {
            if (chara == characters)
            {
                Character = chara;
                break;
            }
        }
    }
}
