using System;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    private IState _currentState;
    private Dictionary<Type, IState> _states = new();

    protected virtual void Update()
    {
        _currentState?.Update();
    }

    public void ChangeState<T>() where T : IState
    {
        if (_states.TryGetValue(typeof(T), out IState state))
        {
            _currentState?.Exit();
            _currentState = state;
            _currentState?.Enter();
            
            Debug.Log("Change State " + typeof(T).Name);
        }
        else
        {
            Debug.LogWarning($"State {typeof(T)} does not exist.");
        }
    }

    protected virtual void AddState(IState state)
    {
        if (!_states.TryAdd(state.GetType(), state))
        {
            Debug.LogWarning($"Duplicate State Type : {state.GetType()}");
        }
    }
    
    protected virtual void RemoveState(IState state)
    {
        if (_states.Remove(state.GetType()))
        {
            if (state.GetType() == _currentState.GetType())
            {
                _currentState = null;
            }
        }
        else
        {
            Debug.LogWarning($"State {state.GetType()} does not exist.");
        }
    }
}