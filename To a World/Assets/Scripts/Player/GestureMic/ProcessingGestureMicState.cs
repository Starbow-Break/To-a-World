public class ProcessingGestureMicState: AGestureMicState
{
    public ProcessingGestureMicState(GestureMic gestureMic) : base(gestureMic) {  }
    
    public override void UpdateButton()
    {
        _gestureMic.RecordButtonSetter.SetStartButtonInteractable(false);
        _gestureMic.RecordButtonSetter.SetStopButtonInteractable(false);
        
        _gestureMic.RecordMessageSetter.HideMessage();

        NPCChatSystem.NPCChatManager.OnProcessingStateChanged += ProcessingStateChanged;
    }

    private void ProcessingStateChanged(bool state)
    {
        if (!state)
        {
            _gestureMic.ChangeState(new ReadyGestureMicState(_gestureMic));
        }
    }
}
