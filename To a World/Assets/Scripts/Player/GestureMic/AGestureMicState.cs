public abstract class AGestureMicState
{
    protected GestureMic _gestureMic;

    public AGestureMicState(GestureMic gestureMic)
    {
        _gestureMic = gestureMic;
    }
    
    public abstract void UpdateButton();

    protected void RemoveAllRecordButtonListeners()
    {
        _gestureMic.RecordButtonSetter.RemoveStartButtonAllListenersSelectEnter();
        _gestureMic.RecordButtonSetter.RemoveStartButtonAllListenersSelectExit();
        _gestureMic.RecordButtonSetter.RemoveStopButtonAllListenersSelectEnter();
        _gestureMic.RecordButtonSetter.RemoveStopButtonAllListenersSelectExit();
    }
}
