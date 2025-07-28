using System;
using UnityEngine;

public class CameraEvents: IEvents
{
    public event Action<Camera> OnCameraShot;
    public void CameraShot(Camera camera) => OnCameraShot?.Invoke(camera);

    public event Action<Camera, CameraDetectable> OnShotInCamera;
    public void ShotInCamera(Camera camera, CameraDetectable detectable) => OnShotInCamera?.Invoke(camera, detectable);
}
