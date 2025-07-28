using UnityEngine.XR.Interaction.Toolkit;

public class ReadyGestureMicState: AGestureMicState
{
    public ReadyGestureMicState(
        GestureMic gestureMic, 
        GestureMicStateController controller) : base(gestureMic, controller) {  }

    public override void Enter()
    {
        _gestureMic.SetActive(true);
        _gestureMic.RecordButtonSetter.AddStartButtonListenerSelectEnter(SelectStartRecordButton);
        
        _gestureMic.RecordButtonSetter.SetStartButtonInteractable(true);
        _gestureMic.RecordButtonSetter.SetStopButtonInteractable(false);
        
        _gestureMic.RecordMessageSetter.HideMessage();
    }
    
    public override void Update() {  }

    private void SelectStartRecordButton(SelectEnterEventArgs arg)
    {
        var npcChatManager = NPCChatSystem.NPCChatManager;

        if (npcChatManager != null)
        {
            npcChatManager.TryStartRecording();
            _controller.ChangeState<RecordingGestureMicState>();
        }
    }
}
