using NPCSystem;
using UnityEngine;

public class TalkMicMessageSetter : MonoBehaviour
{
    [SerializeField] private MicMessageUpdater _updater;
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        HideMessage();
    }
    
    public void ShowMessage() => _updater.SetActive(true);
    public void HideMessage() => _updater.SetActive(false);
}
