using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Phone
{
    public class QuestSkipperWindow : MonoBehaviour, IAppWindow
    {
        [SerializeField] private Button skipButton;
        [SerializeField] private TMP_Text questName;

        public Button SkipButton => skipButton;

        public void SetQuestName(string name)
        {
            questName.text = name;
        }
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}