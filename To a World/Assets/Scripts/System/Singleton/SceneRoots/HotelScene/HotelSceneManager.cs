using ManagerScene;

public class HotelSceneManager : SceneSingleton<HotelSceneManager>
{
    private void Start()
    {
        HotelSceneServices.TimelineSequencer.PlayTimeline();
    }

    public void ToNextScene()
    {
        ManagerSceneServices.SceneController.SetCurrentScene(ESceneIndex.HotelScene);
        ManagerSceneServices.SceneController.LoadScene(ESceneIndex.CruiseScene);
    }
}
