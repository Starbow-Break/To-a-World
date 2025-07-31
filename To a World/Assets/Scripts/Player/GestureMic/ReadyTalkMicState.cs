using UnityEngine.XR.Interaction.Toolkit;

public class ReadyTalkMicState: ATalkMicState
{
    public ReadyTalkMicState(
        TalkMic talkMic, 
        GestureMicStateController controller) : base(talkMic, controller) {  }

    public override void Enter()
    {
        _talkMic.SetActive(true);
        
        _talkMic.ButtonSetter.AddListenerSelectEnter(SelectEnteredTalkMicButton);
        _talkMic.ButtonSetter.SetButtonInteractable(true);
        
        _talkMic.MessageSetter.HideMessage();
    }
    
    public override void Update() {  }

    protected override void SelectEnteredTalkMicButton(SelectEnterEventArgs arg)
    {
        var npcChatManager = NPCChatSystem.NPCChatManager;

        if (npcChatManager != null)
        {
            npcChatManager.TryStartRecording();
            _controller.ChangeState<RecordingTalkMicState>();
        }
    }
}
