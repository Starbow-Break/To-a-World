using ManagerScene;
using UnityEngine;

namespace AirplaneScene
{
    public class AirplaneSceneManager : MonoBehaviour
    {
        private void Start()
        {
            AirplaneSceneServices.TimelineSequencer.PlayTimeline();
        }
    }
}
