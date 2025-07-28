using System;
using UnityEngine;

public class CameraEvents: IEvents
{
    public event Action<Camera> OnCameraShot;
    public void CameraShot(Camera camera) => OnCameraShot?.Invoke(camera);

    public event Action<string> OnShotInCamera;
    public void ShotInCamera(string objectId) => OnShotInCamera?.Invoke(objectId);
}
