using UnityEngine;

public class CameraQuest : AQuest
{
    private string _targetId;
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as CameraQuestParams;
        if (param != null)
        {
            _targetId = param.CameraDetectable.ID;
        }
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<CameraEvents>().OnShotInCamera += OnShotInCamera;
    }

    private void OnDisable()
    {
        if (GameEventsManager.TryGetEvents<CameraEvents>(out var cameraEvents))
        {
            cameraEvents.OnShotInCamera -= OnShotInCamera;
        }
    }

    private void OnShotInCamera(string id)
    {
        if (id == _targetId)
        {
            CompleteQuest();
        }
    }
}
