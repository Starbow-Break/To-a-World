using System;
using UnityEngine;

public class CameraDetectable: MonoBehaviour
{
    [field: SerializeField] public string ID { get; private set; } = "";
    
    private Collider _collider;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            Debug.LogWarning($"Collider not found. Object {gameObject.name} cannot be detected.");
        }
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
        GameEventsManager.GetEvents<CameraEvents>().OnCameraShot -= OnCameraShot;
    }

    private void OnCameraShot(Camera camera)
    {
        if (CheckDetectCamera(camera))
        {
            Debug.Log($"찍혔다!! : {gameObject.name}");
            GameEventsManager.GetEvents<CameraEvents>().ShotInCamera(ID);
        }
    }
    
    private bool CheckDetectCamera(Camera camera)
    {
        var frustum = GeometryUtility.CalculateFrustumPlanes(camera);
        var bounds = _collider.bounds;
        return GeometryUtility.TestPlanesAABB(frustum, bounds);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (String.IsNullOrEmpty(ID))
        {
            ID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
