using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManagerScene
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private GameObject _fadeCanvas;
        
        private ESceneIndex _currentScene = ESceneIndex.None;
        private readonly List<AsyncOperation> _loadingOperations = new List<AsyncOperation>();

        private const float FadeDuration = 0.3f;
        
        public void LoadScene(ESceneIndex scene)
        {
            if(_currentScene != ESceneIndex.None)
                _loadingOperations.Add(SceneManager.UnloadSceneAsync((int)_currentScene));
        
            _loadingOperations.Add(SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive));
            StartCoroutine(GetSceneLoading(scene));
            //_fadeCanvas.SetActive(true);
        }

        public void SetCurrentScene(ESceneIndex scene)
        {
            _currentScene = scene;
        }
        
        private IEnumerator GetSceneLoading(ESceneIndex scene)
        {
            foreach (var t in _loadingOperations)
            {
                while (!t.isDone)
                {
                    yield return null;
                }
            }
        
            //_fadeCanvas.SetActive(false);
            yield return new WaitForSeconds(FadeDuration);
            _currentScene = scene;
        }
    }
}
