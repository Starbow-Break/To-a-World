using UnityEngine.XR.Interaction.Toolkit;

public class RecordingGestureMicState: AGestureMicState
{
    public RecordingGestureMicState(
        GestureMic gestureMic, 
        GestureMicStateController controller) : base(gestureMic, controller) {  }

    public override void Enter()
    {
        _gestureMic.RecordButtonSetter.AddStopButtonListenerSelectEnter(SelectStopRecordButton);
        
        _gestureMic.RecordButtonSetter.SetStartButtonInteractable(false);
        _gestureMic.RecordButtonSetter.SetStopButtonInteractable(true);
        
        _gestureMic.RecordMessageSetter.ShowMessage();
    }

    public override void Update()
    {
        if (!_gestureMic.IsActive)
        {
            NPCChatSystem.NPCChatManager.CancelRecording();
            _controller.ChangeState<ReadyGestureMicState>();
        }
    }

    private void SelectStopRecordButton(SelectEnterEventArgs arg)
    {
        NPCChatSystem.NPCChatManager.TryStopRecording(); 
        _controller.ChangeState<ProcessingGestureMicState>();
    }
}
