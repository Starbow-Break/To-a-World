using System;
using UnityEngine;

public class CameraDetectable: MonoBehaviour
{
    [SerializeField] private CameraDetectableData _data;
    
    public string ID { get; private set; }
    private Collider _collider;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            Debug.LogWarning($"Collider not found. Object {gameObject.name} cannot be detected.");
        }

        ID = _data.ID;
    }

    private void Start()
    {
        CameraDetectableRegistry.Instance.Register(ID, this);
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<CameraEvents>().OnCameraShot += OnCameraShot;
    }

    private void OnDisable()
    {
        if (GameEventsManager.TryGetEvents<CameraEvents>(out var cameraEvents))
        {
            cameraEvents.OnCameraShot -= OnCameraShot;
        }
    }

    private void OnCameraShot(Camera camera)
    {
        if (CheckDetectCamera(camera))
        {
            GameEventsManager.GetEvents<CameraEvents>().ShotInCamera(ID);
        }
    }
    
    private bool CheckDetectCamera(Camera camera)
    {
        var frustum = GeometryUtility.CalculateFrustumPlanes(camera);
        var bounds = _collider.bounds;
        return GeometryUtility.TestPlanesAABB(frustum, bounds);
    }
}
