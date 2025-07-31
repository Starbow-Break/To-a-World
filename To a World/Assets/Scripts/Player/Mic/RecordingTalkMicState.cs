using UnityEngine.XR.Interaction.Toolkit;

public class RecordingTalkMicState: ATalkMicState
{
    public RecordingTalkMicState(
        TalkMic talkMic, 
        TalkMicStateController controller,
        TalkMicStateParams stateParams) : base(talkMic, controller, stateParams) {  }

    public override void Enter()
    {
        base.Enter();
        _talkMic.ButtonSetter.AddListenerSelectEnter(SelectEnteredTalkMicButton);
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
