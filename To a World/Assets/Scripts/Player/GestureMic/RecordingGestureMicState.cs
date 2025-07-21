using UnityEngine.XR.Interaction.Toolkit;

public class RecordingGestureMicState: AGestureMicState
{
    public RecordingGestureMicState(GestureMic gestureMic) : base(gestureMic) {  }

    public override void UpdateButton()
    {
        RemoveAllRecordButtonListeners();
        
        _gestureMic.RecordButtonSetter.AddStopButtonListenerSelectEnter(SelectStopRecordButton);
        
        _gestureMic.RecordButtonSetter.SetStartButtonInteractable(false);
        _gestureMic.RecordButtonSetter.SetStopButtonInteractable(true);
        
        _gestureMic.RecordMessageSetter.ShowMessage();
    }

    private void SelectStopRecordButton(SelectEnterEventArgs arg)
    {
        if (_gestureMic.IsActive)
        {
            NPCChatSystem.NPCChatManager.TryStopRecording();
            _gestureMic.ChangeState(new ProcessingGestureMicState(_gestureMic));
        }
        else
        {
            NPCChatSystem.NPCChatManager.CancelRecording();
            _gestureMic.ChangeState(new ReadyGestureMicState(_gestureMic));
        }
    }
}
