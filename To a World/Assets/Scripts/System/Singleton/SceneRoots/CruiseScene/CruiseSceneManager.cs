using UnityEngine;

public class CruiseSceneManager : SceneSingleton<CruiseSceneManager>
{
    private void Start()
    {
        CruiseSceneServices.TimelineSequencer.PlayTimeline();
    }
}
