using Unity.VisualScripting;
using UnityEngine;

namespace ManagerScene
{
    public class ManagerSceneServices : SceneSingleton<ManagerSceneServices>
    {
        [SerializeField] SceneController sceneController;
        public static SceneController SceneController => Instance.sceneController;
        
        [SerializeField] private DataManager dataManager;
        public static DataManager DataManager => Instance.dataManager;
    }
}
