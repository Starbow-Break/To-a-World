using UnityEngine;

public class QuestIcon : MonoBehaviour
{
    [Header("Icons")] 
    [SerializeField] private GameObject _requirementsNotMetToStartIcon;
    [SerializeField] private GameObject _canStartIcon;
    [SerializeField] private GameObject _requirementsNotMetToFinishIcon;
    [SerializeField] private GameObject _canFinishIcon;

    public void SetState(QuestState newState, bool startPoint, bool finishPoint)
    {
        _requirementsNotMetToStartIcon.SetActive(false);
        _canStartIcon.SetActive(false); 
        _requirementsNotMetToFinishIcon.SetActive(false);
        _canFinishIcon.SetActive(false);

        switch (newState)
        {
            case QuestState.REQUIREMENTS_NOT_MET:
                if (startPoint)
                {
                    _requirementsNotMetToStartIcon.SetActive(true);
                }
                break;
            case QuestState.CAN_START:
                if (startPoint)
                {
                    _canStartIcon.SetActive(true);
                }
                break;
            case QuestState.IN_PROGRESS:
                if (finishPoint)
                {
                    _requirementsNotMetToFinishIcon.SetActive(true);
                }
                break;
            case QuestState.CAN_FINISH:
                if (finishPoint)
                {
                    _canFinishIcon.SetActive(true);
                }
                break;
            case QuestState.FINISHED:
                break;
            default:
                Debug.LogWarning($"Invalid quest state: {newState}");
                break;
        }
    }
}
