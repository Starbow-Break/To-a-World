using System;
using NPCSystem;
using Phone;
using UnityEngine;

namespace Phone
{
    public class SpeechLoggerApp : AApp
    {
        [SerializeField] private SpeechLoggerWindow speechLoggerWindow;

        [SerializeField] private NPCConversationManager conversationManager;

        private string _currentLog = string.Empty;

        private void Awake()
        {
            conversationManager.OnMessageAdded += OnMessageAdded;
        }
        
        private void OnDestroy()
        {
            if (conversationManager != null)
            {
                conversationManager.OnMessageAdded -= OnMessageAdded;
            }
        }

        public override void Open()
        {
            speechLoggerWindow.Open();
        }

        public override void Close()
        {
            speechLoggerWindow.Close();
        }

        private void OnMessageAdded(ConversationMessage message)
        {
            _currentLog += $"{message.sender}: {message.message} \n";
            
            speechLoggerWindow.SetText(_currentLog);
        }
    }
}
