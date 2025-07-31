using System;
using System.Collections.Generic;
using UnityEngine;

public class TalkMicStateController: StateController
{
    [SerializeField] private TalkMic talkMic;
    
    [Header("StateParams")]
    [SerializeField] private TalkMicStateParams _readyStateParams;
    [SerializeField] private TalkMicStateParams _recordingStateParams;
    [SerializeField] private TalkMicStateParams _processingStateParams;

    private void Start()
    {
        AddState(new ReadyTalkMicState(talkMic, this, _readyStateParams));
        AddState(new RecordingTalkMicState(talkMic, this, _recordingStateParams));
        AddState(new ProcessingTalkMicState(talkMic, this, _processingStateParams));
        
        ChangeState<ReadyTalkMicState>();
    }
}
