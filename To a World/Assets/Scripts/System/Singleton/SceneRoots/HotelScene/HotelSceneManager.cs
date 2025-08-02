using UnityEngine;

public class HotelSceneManager : SceneSingleton<HotelSceneManager>
{
    private void Start()
    {
        HotelSceneServices.TimelineSequencer.PlayTimeline();
    }
}
