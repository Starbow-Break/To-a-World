using System.Collections.Generic;
using UnityEngine;

public class PlaceTargetPointRegistry : MonoBehaviour, IRegistry<string, PlaceTargetPoint>
{
    [SerializeField] private List<PlaceTargetPoint> _initTargetPoints = new();
    
    private Dictionary<string, PlaceTargetPoint> _container;

    private void Awake()
    {
        _container = new();
        
        foreach (var targetPoint in _initTargetPoints)
        {
            Register(targetPoint.ID, targetPoint);
        }
    }

    public void Register(string placeId, PlaceTargetPoint placeTargetPoint)
    {
        if (!_container.TryAdd(placeId, placeTargetPoint))
        {
            Debug.LogWarning($"Duplicate ID : {placeId}");
        }
    }
    
    public void UnRegister(string placeId)
    {
        if (!_container.Remove(placeId))
        {
            Debug.LogWarning($"Object With ID {placeId} does not exist");
        }
    }

    public PlaceTargetPoint Get(string placeId)
    {
        return _container[placeId];
    }
}
