using TMPro;
using UnityEngine;

public class QuestListItemUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text _questTitle;
    [SerializeField] private TMP_Text _questStepDescription;

    public void SetQuestTitle(string questTitle)
    {
        _questTitle.text = questTitle;
    }

    public void SetQuestDescription(string questStepDescription)
    {
        _questStepDescription.text = questStepDescription;
    }
}
