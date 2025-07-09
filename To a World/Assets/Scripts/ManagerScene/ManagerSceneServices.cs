using Unity.VisualScripting;
using UnityEngine;

namespace ManagerScene
{
    public class ManagerSceneServices : SceneSingleton<ManagerSceneServices>
    {
        [SerializeField] SceneController _sceneController;
        public static SceneController SceneController => Instance._sceneController;
        
        [SerializeField] private DataManager _dataManager;
        public static DataManager DataManager => Instance._dataManager;
    }
}
