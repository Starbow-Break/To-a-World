using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSequencer : MonoBehaviour
{
    [SerializeField] private PlayableDirector[] directors;
    [SerializeField] private float delay = 0.0f;
    
    private int _currentIndex = 0;
    
    public void PlayTimeline()
    {
        if (directors == null || directors.Length == 0)
        {
            Debug.LogWarning("PlayableDirector가 하나도 할당되지 않았습니다.");
            return;
        }

        PlayCurrent();
    }
    
    private void PlayCurrent()
    {
        var dir = directors[_currentIndex];
        dir.stopped += OnDirectorStopped;
        dir.Play();
    }
    
    private void OnDirectorStopped(PlayableDirector director)
    {
        director.stopped -= OnDirectorStopped;

        _currentIndex++;
        if (_currentIndex < directors.Length)
        {
            PlayCurrent();
        }
        else
        {
            Debug.Log("모든 타임라인 재생 완료");
        }
    }
}
