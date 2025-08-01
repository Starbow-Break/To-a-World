using UnityEngine.XR.Interaction.Toolkit;

public class ReadyTalkMicState: ATalkMicState
{
    public ReadyTalkMicState(
        TalkMic talkMic, 
        TalkMicStateController controller,
        TalkMicStateParams stateParams) : base(talkMic, controller, stateParams) {  }

    public override void Enter()
    {
        base.Enter();
        _talkMic.ButtonSetter.AddOnClickListener(OnClickTalkMicButton);
        _talkMic.ButtonSetter.SetButtonInteractable(true);
    }
    
    public override void Update() {  }

    protected override void OnClickTalkMicButton()
    {
        var npcChatManager = NPCChatSystem.NPCChatManager;

        if (npcChatManager != null)
        {
            npcChatManager.TryStartRecording();
            _controller.ChangeState<RecordingTalkMicState>();
        }
    }
}
