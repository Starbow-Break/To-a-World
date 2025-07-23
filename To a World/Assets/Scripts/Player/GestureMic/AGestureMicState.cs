public abstract class AGestureMicState: IState
{
    protected GestureMic _gestureMic;
    protected GestureMicStateController _controller;

    public AGestureMicState(GestureMic gestureMic, GestureMicStateController controller)
    {
        _gestureMic = gestureMic;
        _controller = controller;
    }
    
    public abstract void Enter();
    public abstract void Update();
    public virtual void Exit()
    {
        RemoveAllRecordButtonListeners();
    }

    protected void RemoveAllRecordButtonListeners()
    {
        _gestureMic.RecordButtonSetter.RemoveStartButtonAllListenersSelectEnter();
        _gestureMic.RecordButtonSetter.RemoveStartButtonAllListenersSelectExit();
        _gestureMic.RecordButtonSetter.RemoveStopButtonAllListenersSelectEnter();
        _gestureMic.RecordButtonSetter.RemoveStopButtonAllListenersSelectExit();
    }
}
