using UnityEngine.XR.Interaction.Toolkit;

public class ReadyGestureMicState: AGestureMicState
{
    public ReadyGestureMicState(GestureMic gestureMic) : base(gestureMic) {  }

    public override void UpdateButton()
    {
        RemoveAllRecordButtonListeners();
        
        _gestureMic.SetActive(true);
        _gestureMic.RecordButtonSetter.AddStartButtonListenerSelectEnter(SelectStartRecordButton);
        
        _gestureMic.RecordButtonSetter.SetStartButtonInteractable(true);
        _gestureMic.RecordButtonSetter.SetStopButtonInteractable(false);
        
        _gestureMic.RecordMessageSetter.HideMessage();
    }

    private void SelectStartRecordButton(SelectEnterEventArgs arg)
    {
        var npcChatManager = NPCChatSystem.NPCChatManager;

        if (npcChatManager != null)
        {
            npcChatManager.TryStartRecording();
            _gestureMic.ChangeState(new RecordingGestureMicState(_gestureMic));
        }
    }
}
