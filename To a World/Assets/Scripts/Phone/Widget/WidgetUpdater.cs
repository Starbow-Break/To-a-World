using TMPro;
using UnityEngine;

namespace Phone.Widget
{
    public class WidgetUpdater : MonoBehaviour
    {
        [SerializeField] private TMP_Text questTitle;
        public TMP_Text QuestTitle => questTitle;
        
        [SerializeField] private TMP_Text questDescription;
        public TMP_Text QuestDescription => questDescription;
    }
}
