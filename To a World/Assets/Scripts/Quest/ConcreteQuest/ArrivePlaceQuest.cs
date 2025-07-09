using System;
using UnityEngine;

public class ArrivePlaceQuest : AQuest
{
    [SerializeField] private PlaceTargetPointRegistry placeTargetPointRegistry;
    
    private string _targetPointId;
    private PlaceTargetPoint _targetPoint;
    
    private void OnEnable()
    {
        GameEventsManager.GetEvents<PlaceEvents>().OnArrive += OnArrive;

        if (_targetPoint != null)
        {
            _targetPoint.gameObject.SetActive(true); 
        }
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<PlaceEvents>().OnArrive -= OnArrive;
        
        if (_targetPoint != null)
        {
            _targetPoint.gameObject.SetActive(false); 
        }
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as ArrivePlaceQuestParams;
        if (param != null)
        {
            _targetPointId = param.TargetPlaceID;
            _targetPoint = placeTargetPointRegistry.Get(_targetPointId);
        }
    }

    private void OnArrive(string placeId)
    {
        if (placeId == _targetPointId)
        {
            CompleteQuest();
        }
    }
    
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (String.IsNullOrEmpty(_targetPointId))
        {
            _targetPointId = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
