using UnityEngine;

public class SpeechBubbleUpdater: MonoBehaviour
{
    public void Initialize()
    {
        InActive();
    }

    public void Active()
    {
        gameObject.SetActive(true);
    }
    
    public void InActive()
    {
        gameObject.SetActive(false);
    }
}
