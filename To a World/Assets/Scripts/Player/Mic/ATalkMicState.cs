using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class ATalkMicState: IState
{
    protected TalkMic _talkMic;
    protected TalkMicStateController _controller;

    protected Color _buttonColor;
    protected string _message;

    public ATalkMicState(TalkMic talkMic, TalkMicStateController controller, TalkMicStateParams stateParams)
    {
        _talkMic = talkMic;
        _controller = controller;

        _buttonColor = stateParams.ButtonColor;
        _message = stateParams.Message;
    }

    public virtual void Enter()
    {
        _talkMic.ButtonSetter.SetColor(_buttonColor);
        _talkMic.MessageSetter.UpdateMessage(_message);
    }
    
    public abstract void Update();

    public virtual void Exit()
    {
        _talkMic.ButtonSetter.RemoveAllListenersSelectEnter();
    }

    protected abstract void SelectEnteredTalkMicButton(SelectEnterEventArgs arg);
}
