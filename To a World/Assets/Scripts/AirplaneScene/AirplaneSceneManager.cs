using ManagerScene;
using UnityEngine;

namespace AirplaneScene
{
    public class AirplaneSceneManager : MonoBehaviour
    {
        private void Start()
        {
            if(ManagerSceneServices.Instance != null)
                ManagerSceneServices.LoadingScreen.gameObject.SetActive(false);
            AirplaneSceneServices.TimelineSequencer.PlayTimeline();
        }
    }
}
