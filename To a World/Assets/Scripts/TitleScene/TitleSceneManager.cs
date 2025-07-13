using ManagerScene;
using UnityEngine;

namespace TitleScene
{
    public class TitleSceneManager : SceneSingleton<TitleSceneManager>
    {
        [SerializeField] private AShowableUI[] _showableUIs;

        private int _currentIndex = 0;
    
        protected override void Awake()
        {
            base.Awake();
            _currentIndex = 0;
            foreach (var ui in _showableUIs)
            {
                ui.ActOnClose += ShowNextUI;
            }
        }
       
        private void OnEnable()
        {
            TitleSceneServices.LoadingScreen.ActOnFadeOutEnd += LoadNextScene;
        }

        private void OnDisable()
        {
            TitleSceneServices.LoadingScreen.ActOnFadeOutEnd -= LoadNextScene;
        }
    
        private void Start()
        {
            _showableUIs[_currentIndex].Show();
        }

        private void ShowNextUI()
        {
            _currentIndex++;

            if (_currentIndex >= _showableUIs.Length)
            {
                TitleSceneServices.LoadingScreen.FadeOut();
                return;
            }
        
            _showableUIs[_currentIndex].Show();
        }
        
        private void LoadNextScene()
        {
            ManagerSceneServices.SceneController.SetCurrentScene(ESceneIndex.TitleScene);
            ManagerSceneServices.SceneController.LoadScene(ESceneIndex.AirplaneScene);
        }
    }
}
