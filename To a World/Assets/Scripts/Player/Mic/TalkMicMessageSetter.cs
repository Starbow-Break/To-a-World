using System;
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
        UpdateMessage(String.Empty);
    }
    
    public void UpdateMessage(string message) => _updater.SetText(message);
}
