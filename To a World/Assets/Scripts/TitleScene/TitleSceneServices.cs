using UnityEngine;

namespace TitleScene
{
    public class TitleSceneServices : SceneSingleton<TitleSceneServices>
    {
        [SerializeField] private LoadingScreen loadingScreen;
        public static LoadingScreen LoadingScreen => Instance.loadingScreen;
    }
}
