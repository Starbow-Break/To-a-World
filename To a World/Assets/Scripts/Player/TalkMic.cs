using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Gestures;

public class GestureMic : MonoBehaviour
{
    [SerializeField] private StaticHandGesture _handGesture;
    [SerializeField] private TTSUIController _ttsSender;

    [SerializeField] private List<GameObject> _objects;
    
    private bool _gestureFlag = false;
    private bool _recordingFlag = false;
    private bool _latestValue = false;

    private void OnEnable()
    {
        _ttsSender.OnStartRecording += SetTrueGestureFlag;
        _ttsSender.OnStopRecording += SetFalseGestureFlag;

        _handGesture.GesturePerformed.AddListener(SetTrueRecordingFlag);
        _handGesture.GestureEnded.AddListener(SetFalseRecordingFlag);
    }

    private void OnDisable()
    {
        _ttsSender.OnStartRecording -= SetTrueGestureFlag;
        _ttsSender.OnStopRecording -= SetFalseGestureFlag;
        
        _handGesture.GesturePerformed.RemoveAllListeners();
        _handGesture.GestureEnded.RemoveAllListeners();
    }

    private void Update()
    {
        bool value = _gestureFlag || _recordingFlag;
        if (_latestValue != value)
        {
            Debug.Log($"{_gestureFlag} {_recordingFlag}");
            
            _latestValue = value;
            foreach (GameObject obj in _objects)
            {
                obj.SetActive(value);
            }
        }
    }
    
    private void SetTrueGestureFlag() => _gestureFlag = true;
    private void SetFalseGestureFlag() => _gestureFlag = false;
    
    private void SetTrueRecordingFlag() => _recordingFlag = true;
    private void SetFalseRecordingFlag() => _recordingFlag = false;
}
