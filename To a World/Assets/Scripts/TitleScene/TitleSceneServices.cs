using UnityEngine;

namespace TitleScene
{
    public class TitleSceneServices : SceneSingleton<TitleSceneServices>
    {
        [SerializeField] private LoadingScreen loadingScreen;
        public LoadingScreen LoadingScreen => loadingScreen;
    }
}
