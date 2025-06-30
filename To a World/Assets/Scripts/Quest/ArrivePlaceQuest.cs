using System;
using UnityEngine;

public class ArrivePlaceQuest : AQuest
{
    [SerializeField] private PlaceTargetPointRegistry placeTargetPointRegistry;
    [SerializeField] private string _targetPointId;
    
    private PlaceTargetPoint _targetPoint;
    private bool _arrive = false;

    private void Awake()
    {
        _targetPoint = placeTargetPointRegistry.Get(_targetPointId);
    }
    
    private void OnEnable()
    {
        GameEventsManager.GetEvents<PlaceEvents>().OnArrive += OnArrive;
        _targetPoint.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<PlaceEvents>().OnArrive -= OnArrive;
        _targetPoint.gameObject.SetActive(false);
    }
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as ArrivePlaceQuestParams;
        if (param != null)
        {
            _targetPointId = param.TargetPlaceID;
        }
    }

    private void OnArrive(string placeId)
    {
        if (placeId == _targetPointId)
        {
            _arrive = true;
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
