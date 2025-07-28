using UnityEngine;

public class CameraQuest : AQuest
{
    private GameObject _target;
    
    public override void Initialize(AQuestParams questParams)
    {
        var param = questParams as CameraQuestParams;
        if (param != null)
        {
            
        }
    }

    private void OnEnable()
    {
        GameEventsManager.GetEvents<CameraEvents>().OnShotInCamera += OnShotInCamera;
    }

    private void OnDisable()
    {
        GameEventsManager.GetEvents<CameraEvents>().OnShotInCamera -= OnShotInCamera;
    }

    private void OnShotInCamera(Camera camera, CameraDetectable detectable)
    {
        
        if (detectable != null)
        {
            CompleteQuest();    
        }
    }
}
