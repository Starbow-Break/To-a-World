using System.Collections;
using ManagerScene;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class ManagerSceneLoader : MonoBehaviour
{
    [SerializeField] private ESceneIndex currentScene;
    private const ESceneIndex ManagerSceneIndex = ESceneIndex.ManagerScene;

    private void Awake()
    {
        if (!IsSceneLoaded(ManagerSceneIndex))
        {
            SceneManager.LoadScene((int)ManagerSceneIndex, LoadSceneMode.Additive);
        }

        StartCoroutine(WaitForManagerSceneLoad());
        Destroy(gameObject);
    }

    private IEnumerator WaitForManagerSceneLoad()
    {
        yield return new WaitUntil(()=> IsSceneLoaded(ManagerSceneIndex) == true);
        
        ManagerSceneServices.SceneController.SetCurrentScene(currentScene);
    }
    
    private bool IsSceneLoaded(ESceneIndex sceneIndex)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == (int)sceneIndex)
            {
                return true;
            }
        }
        return false;
    }
}
