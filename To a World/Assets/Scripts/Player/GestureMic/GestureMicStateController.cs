using System;
using UnityEngine;

public class GestureMicStateController: StateController
{
    [SerializeField] private TalkMic talkMic;

    private void Start()
    {
        AddState(new ReadyTalkMicState(talkMic, this));
        AddState(new RecordingTalkMicState(talkMic, this));
        AddState(new ProcessingTalkMicState(talkMic, this));
        
        ChangeState<ReadyTalkMicState>();
    }
}
