using UnityEngine;

public class HotelSceneServices : SceneSingleton<HotelSceneServices>
{
    [SerializeField] private TimelineSequencer timelineSequencer;
    public static TimelineSequencer TimelineSequencer => Instance.timelineSequencer;
}
