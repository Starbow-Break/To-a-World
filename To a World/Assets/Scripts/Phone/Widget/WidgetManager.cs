using TMPro;
using UnityEngine;

namespace Phone.Widget
{
    public class WidgetManager : MonoBehaviour
    {
        [SerializeField] private WidgetUpdater updater;
        [SerializeField] private Color finishedTextColor;
        private Color _originalTextColor;
        
        private void Awake()
        {
            _originalTextColor = updater.QuestTitle.color;
            GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange += OnQuestStateChange;
        }
        
        private void OnDestroy()
        {
            GameEventsManager.GetEvents<QuestEvents>().OnQuestStateChange -= OnQuestStateChange;
        }

        private void OnQuestStateChange(AQuest quest)
        {
            if (quest.State == EQuestState.IN_PROGRESS
                || quest.State == EQuestState.CAN_FINISH)
            {
                updater.QuestTitle.text = quest.Info.Name;
                updater.QuestDescription.text = quest.Info.Description;
                
                updater.QuestTitle.color = _originalTextColor;
                updater.QuestDescription.color = _originalTextColor;

                updater.QuestTitle.fontStyle |= FontStyles.Normal;
                updater.QuestDescription.fontStyle |= FontStyles.Normal;
                return;
            }
            
            updater.QuestTitle.color = finishedTextColor;
            updater.QuestDescription.color = finishedTextColor;

            updater.QuestTitle.fontStyle |= FontStyles.Strikethrough;
            updater.QuestDescription.fontStyle |= FontStyles.Strikethrough;
        }
        
        private void Start()
        {
        }
        

    }
}
