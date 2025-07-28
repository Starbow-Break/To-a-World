using System;
using UnityEngine;

public class GestureMicStateController: StateController
{
    [SerializeField] private GestureMic _gestureMic;

    private void Start()
    {
        AddState(new ReadyGestureMicState(_gestureMic, this));
        AddState(new RecordingGestureMicState(_gestureMic, this));
        AddState(new ProcessingGestureMicState(_gestureMic, this));
        
        ChangeState<ReadyGestureMicState>();
    }
}
