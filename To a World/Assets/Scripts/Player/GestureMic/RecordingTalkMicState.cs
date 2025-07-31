using UnityEngine.XR.Interaction.Toolkit;

public class RecordingTalkMicState: ATalkMicState
{
    public RecordingTalkMicState(
        TalkMic talkMic, 
        GestureMicStateController controller) : base(talkMic, controller) {  }

    public override void Enter()
    {
        _talkMic.ButtonSetter.AddListenerSelectEnter(SelectEnteredTalkMicButton);
        _talkMic.MessageSetter.ShowMessage();
    }

    public override void Update()
    {
        if (!_talkMic.IsActive)
        {
            NPCChatSystem.NPCChatManager.CancelRecording();
            _controller.ChangeState<ReadyTalkMicState>();
        }
    }

    protected override void SelectEnteredTalkMicButton(SelectEnterEventArgs arg)
    {
        NPCChatSystem.NPCChatManager.TryStopRecording(); 
        _controller.ChangeState<ProcessingTalkMicState>();
    }
}
