using UnityEngine;

public class BaseHandXRInput<T> : ABaseCustomXRInput<T> where T : struct
{
    public override bool IsWork { get => !Sleeping && _gesturePerform; }

    [Header("Hand Gesture")]
    [SerializeField] private StaticHandGesture _handGesture;
    
    protected bool _gesturePerform;
    
    protected virtual void OnEnable()
    {
        _handGesture.GesturePerformed.AddListener(GesturePerformed);
        _handGesture.GestureEnded.AddListener(GestureEnded);
    }

    protected virtual void OnDisable()
    {
        _handGesture.GesturePerformed.RemoveListener(GesturePerformed);
        _handGesture.GestureEnded.RemoveListener(GestureEnded);
    }
    
    protected virtual void GesturePerformed() => _gesturePerform = true;
    protected virtual void GestureEnded() => _gesturePerform = false;
}
