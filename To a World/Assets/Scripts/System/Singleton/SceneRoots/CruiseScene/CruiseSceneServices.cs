using UnityEngine;

public class CruiseSceneServices : SceneSingleton<CruiseSceneServices>
{
    [SerializeField] private TimelineSequencer timelineSequencer;
    public static TimelineSequencer TimelineSequencer => Instance.timelineSequencer;
}
