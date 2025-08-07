using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManagerScene
{
    public class SceneController : MonoBehaviour
    {
        private ESceneIndex _currentScene = ESceneIndex.None;
        private readonly List<AsyncOperation> _loadingOperations = new List<AsyncOperation>();
        public ESceneIndex CurrentScene
        {
            get => _currentScene;
            set => _currentScene = value;
        }

        public void LoadScene(ESceneIndex scene)
        {
            if(_currentScene != ESceneIndex.None)
                _loadingOperations.Add(SceneManager.UnloadSceneAsync((int)_currentScene));
        
            _loadingOperations.Add(SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive));
            StartCoroutine(GetSceneLoading(scene));
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
        
            _currentScene = scene;
        }
    }
}
