using ManagerScene;
using UnityEngine;

namespace AirplaneScene
{
    public class AirplaneSceneManager : SceneSingleton<AirplaneSceneManager>
    {
        private void Start()
        {
            AirplaneSceneServices.TimelineSequencer.PlayTimeline();
        }

        public void ToNextScene()
        {
            ManagerSceneServices.SceneController.SetCurrentScene(ESceneIndex.AirplaneScene);
            ManagerSceneServices.SceneController.LoadScene(ESceneIndex.HotelScene);
        }
    }
}
