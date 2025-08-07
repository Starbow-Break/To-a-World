using UnityEngine;
using UnityEngine.Playables;

namespace AirplaneScene
{
    public class AirplaneSceneServices : SceneSingleton<AirplaneSceneServices>
    {
        [SerializeField] private TimelineSequencer timelineSequencer;
        public static TimelineSequencer TimelineSequencer => Instance.timelineSequencer;
        
        [SerializeField] private LoadingScreen loadingScreen;
        public static LoadingScreen LoadingScreen => Instance.loadingScreen;
    }
}
