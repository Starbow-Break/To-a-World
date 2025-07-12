using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GestureMic : MonoBehaviour
{
    public bool IsActive { get; private set; }
    
    [SerializeField] private StaticHandGesture _handGesture;
    [SerializeField] private TTSUIController _ttsSender;

    [SerializeField] private List<GameObject> _objects;
    
    private bool _wakeUpFlag = true;
    private bool _gestureFlag = false;
    private bool _recordingFlag = false;
    
    private bool _nextAcviveValue => _wakeUpFlag && (_gestureFlag || _recordingFlag);

    private void Start()
    {
        SetActiveMic(false);
    }

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
        if (IsActive != _nextAcviveValue)
        {
            SetActiveMic(_nextAcviveValue);
        }
    }

    public void Sleep() => _wakeUpFlag = false;
    public void WakeUp() => _wakeUpFlag = true;

    private void SetActiveMic(bool active)
    {
        IsActive = active;
        foreach (GameObject obj in _objects)
        { 
            obj.SetActive(IsActive);
        }
    }
    
    private void SetTrueGestureFlag() => _gestureFlag = true;
    private void SetFalseGestureFlag() => _gestureFlag = false;
    
    private void SetTrueRecordingFlag() => _recordingFlag = true;
    private void SetFalseRecordingFlag() => _recordingFlag = false;
}
