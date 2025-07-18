using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class PlaceTargetPointRegistry : MonoBehaviour, IRegistry<string, PlaceTargetPoint>
{
    [SerializeField] private List<PlaceTargetPoint> _initTargetPoints = new();
    
    private Dictionary<string, PlaceTargetPoint> _container = new();

    private void Awake()
    {
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
        if (_container.TryGetValue(placeId, out var placeTargetPoint))
        {
            return placeTargetPoint;
        }
        return null;
    }
}
