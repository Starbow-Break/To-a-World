using UnityEngine.XR.Interaction.Toolkit;

public class ProcessingTalkMicState: ATalkMicState
{
    public ProcessingTalkMicState(
        TalkMic talkMic, 
        GestureMicStateController controller) : base(talkMic, controller) {  }
    
    public override void Enter()
    {
        _talkMic.ButtonSetter.SetButtonInteractable(false);
        
        _talkMic.MessageSetter.HideMessage();

        NPCChatSystem.NPCChatManager.OnProcessingStateChanged += ProcessingStateChanged;
    }

    public override void Update() {  }

    public override void Exit()
    {
        base.Exit();
        NPCChatSystem.NPCChatManager.OnProcessingStateChanged -= ProcessingStateChanged;
    }

    protected override void SelectEnteredTalkMicButton(SelectEnterEventArgs arg) {  }

    private void ProcessingStateChanged(bool state)
    {
        if (!state)
        {
            _controller.ChangeState<ReadyTalkMicState>();
        }
    }
}
