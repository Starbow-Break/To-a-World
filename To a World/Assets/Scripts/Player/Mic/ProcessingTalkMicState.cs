using UnityEngine.XR.Interaction.Toolkit;

public class ProcessingTalkMicState: ATalkMicState
{
    public ProcessingTalkMicState(
        TalkMic talkMic, 
        TalkMicStateController controller,
        TalkMicStateParams stateParams) : base(talkMic, controller, stateParams) {  }
    
    public override void Enter()
    {
        base.Enter();
        _talkMic.ButtonSetter.SetButtonInteractable(false);
        
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged += ProcessingStateChanged;
    }

    public override void Update() {  }

    public override void Exit()
    {
        base.Exit();
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged -= ProcessingStateChanged;
    }

    protected override void OnClickTalkMicButton() {  }

    private void ProcessingStateChanged(bool state)
    {
        if (!state)
        {
            _controller.ChangeState<ReadyTalkMicState>();
        }
    }
}
