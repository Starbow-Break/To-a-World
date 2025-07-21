using UnityEngine;

public abstract class AInitializedSceneSingleton<T>: SceneSingleton<T>  where T : MonoBehaviour
{
    public new static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    Debug.LogWarning($"SceneSingleton<{typeof(T).Name}> 초기화 안됨");
                }
                else
                {
                    if (_instance is AInitializedSceneSingleton<T> initSingleton)
                    {
                        initSingleton.Initialize();
                    }
                }
            }
            
            return _instance;
        }
    }

    protected override void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            Initialize();
        }
        else
        {
            if (_instance != this)
            {
                Debug.LogWarning($"복사본 SceneSingleton {typeof(T).Name} 파괴됨.");
                Destroy(gameObject);
            }
        }
    }

    protected abstract void Initialize();
}
