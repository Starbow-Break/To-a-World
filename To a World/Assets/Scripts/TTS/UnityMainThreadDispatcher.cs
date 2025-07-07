using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity 메인 스레드에서 작업을 실행하기 위한 디스패처 클래스
/// 
/// 문제 상황:
/// - Unity는 메인 스레드에서만 Unity API를 사용할 수 있습니다
/// - 백그라운드 스레드에서 GameObject 조작, UI 업데이트 등은 불가능합니다
/// - 비동기 작업 후 결과를 Unity 객체에 반영할 때 필요합니다
/// 
/// 해결책:
/// - 백그라운드 스레드에서 작업을 큐에 저장
/// - 메인 스레드(Update)에서 큐를 확인하여 작업 실행
/// - 스레드 안전성을 위한 lock 사용
/// 
/// 사용 예시:
/// ```csharp
/// // 백그라운드 스레드에서
/// UnityMainThreadDispatcher.Instance().Enqueue(() => {
///     // 메인 스레드에서 실행될 코드
///     someGameObject.SetActive(true);
///     someText.text = "처리 완료";
/// });
/// ```
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    #region Singleton Pattern
    
    /// <summary>
    /// 싱글톤 인스턴스
    /// 전체 애플리케이션에서 하나의 인스턴스만 존재합니다
    /// </summary>
    private static UnityMainThreadDispatcher _instance;
    
    /// <summary>
    /// 싱글톤 인스턴스에 접근하는 메서드
    /// 인스턴스가 없으면 자동으로 생성합니다
    /// </summary>
    /// <returns>UnityMainThreadDispatcher 인스턴스</returns>
    public static UnityMainThreadDispatcher Instance()
    {
        // 인스턴스가 없으면 생성
        if (_instance == null)
        {
            // 새로운 GameObject 생성
            var go = new GameObject("UnityMainThreadDispatcher");
            
            // 컴포넌트 추가
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            
            // 씬이 바뀌어도 객체가 파괴되지 않도록 설정
            DontDestroyOnLoad(go);
        }
        return _instance;
    }
    
    #endregion
    
    #region Thread-Safe Queue Management
    
    /// <summary>
    /// 메인 스레드에서 실행할 작업들을 저장하는 큐
    /// 스레드 안전성을 위해 lock과 함께 사용됩니다
    /// </summary>
    private readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();
    
    /// <summary>
    /// 메인 스레드에서 실행할 작업을 큐에 추가합니다
    /// 백그라운드 스레드에서 호출해도 안전합니다
    /// </summary>
    /// <param name="action">실행할 작업 (Action 델리게이트)</param>
    /// <example>
    /// ```csharp
    /// // 예시 1: 간단한 작업
    /// dispatcher.Enqueue(() => Debug.Log("메인 스레드에서 실행"));
    /// 
    /// // 예시 2: GameObject 조작
    /// dispatcher.Enqueue(() => {
    ///     player.transform.position = newPosition;
    ///     healthBar.fillAmount = currentHealth / maxHealth;
    /// });
    /// 
    /// // 예시 3: UI 업데이트
    /// dispatcher.Enqueue(() => {
    ///     statusText.text = "작업 완료";
    ///     loadingPanel.SetActive(false);
    /// });
    /// ```
    /// </example>
    public void Enqueue(System.Action action)
    {
        // 스레드 안전성을 위한 lock
        // 여러 스레드가 동시에 큐에 접근하는 것을 방지
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
    
    /// <summary>
    /// 현재 큐에 대기 중인 작업 수를 반환합니다
    /// 디버깅이나 성능 모니터링에 사용할 수 있습니다
    /// </summary>
    /// <returns>대기 중인 작업 수</returns>
    public int GetQueueCount()
    {
        lock (_executionQueue)
        {
            return _executionQueue.Count;
        }
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Unity Update 메서드
    /// 매 프레임마다 큐에 있는 모든 작업을 실행합니다
    /// 
    /// 주의사항:
    /// - 큐에 너무 많은 작업이 쌓이면 프레임 드롭이 발생할 수 있습니다
    /// - 무거운 작업은 여러 프레임에 걸쳐 분산해서 처리하는 것이 좋습니다
    /// </summary>
    void Update()
    {
        // 스레드 안전성을 위한 lock
        lock (_executionQueue)
        {
            // 큐에 있는 모든 작업을 실행
            while (_executionQueue.Count > 0)
            {
                try
                {
                    // 큐에서 작업을 꺼내서 실행
                    System.Action action = _executionQueue.Dequeue();
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    // 작업 실행 중 오류가 발생해도 다른 작업들이 계속 실행되도록 처리
                    Debug.LogError($"UnityMainThreadDispatcher 작업 실행 중 오류 발생: {ex.Message}");
                    Debug.LogException(ex);
                }
            }
        }
    }
    
    /// <summary>
    /// Unity Awake 메서드
    /// 인스턴스가 여러 개 생성되는 것을 방지합니다
    /// </summary>
    void Awake()
    {
        // 이미 인스턴스가 존재하는 경우 새로 생성된 객체는 파괴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 첫 번째 인스턴스인 경우 설정
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Unity OnDestroy 메서드
    /// 객체가 파괴될 때 인스턴스 참조를 정리합니다
    /// </summary>
    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// 큐에 있는 모든 작업을 제거합니다
    /// 긴급 상황이나 씬 전환 시 사용할 수 있습니다
    /// </summary>
    public void ClearQueue()
    {
        lock (_executionQueue)
        {
            _executionQueue.Clear();
        }
    }
    
    /// <summary>
    /// 현재 메인 스레드에서 실행 중인지 확인합니다
    /// Unity 2020.1 이상에서 사용 가능합니다
    /// </summary>
    /// <returns>메인 스레드에서 실행 중이면 true, 아니면 false</returns>
    public bool IsMainThread()
    {
        #if UNITY_2020_1_OR_NEWER
        return System.Threading.Thread.CurrentThread.ManagedThreadId == 1;
        #else
        // 구버전 Unity에서는 정확한 판단이 어려우므로 항상 false 반환
        return false;
        #endif
    }
    
    #endregion
} 