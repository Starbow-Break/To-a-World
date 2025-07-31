using UnityEngine.XR.Interaction.Toolkit;

public abstract class ATalkMicState: IState
{
    protected TalkMic _talkMic;
    protected GestureMicStateController _controller;

    public ATalkMicState(TalkMic talkMic, GestureMicStateController controller)
    {
        _talkMic = talkMic;
        _controller = controller;
    }
    
    public abstract void Enter();
    public abstract void Update();

    public virtual void Exit()
    {
        _talkMic.ButtonSetter.RemoveAllListenersSelectEnter();
    }

    protected abstract void SelectEnteredTalkMicButton(SelectEnterEventArgs arg);
}
