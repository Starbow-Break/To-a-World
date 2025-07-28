public class ProcessingGestureMicState: AGestureMicState
{
    public ProcessingGestureMicState(
        GestureMic gestureMic, 
        GestureMicStateController controller) : base(gestureMic, controller) {  }
    
    public override void Enter()
    {
        _gestureMic.RecordButtonSetter.SetStartButtonInteractable(false);
        _gestureMic.RecordButtonSetter.SetStopButtonInteractable(false);
        
        _gestureMic.RecordMessageSetter.HideMessage();

        NPCChatSystem.NPCChatManager.OnProcessingStateChanged += ProcessingStateChanged;
    }

    public override void Update() {  }

    public override void Exit()
    {
        base.Exit();
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged -= ProcessingStateChanged;
    }

    private void ProcessingStateChanged(bool state)
    {
        if (!state)
        {
            _controller.ChangeState<ReadyGestureMicState>();
        }
    }
}
