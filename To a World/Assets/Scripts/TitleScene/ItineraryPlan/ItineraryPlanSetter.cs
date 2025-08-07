using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace TitleScene.ItineraryPlan
{
    public class ItineraryPlanSetter : AShowableUI
    {
        [SerializeField] private ItineraryPlanUpdater updater;
        [SerializeField] private ItineraryButton itineraryButtonPrefab;

        [CanBeNull] private ItineraryButton _selectedItineraryButtonOrNull = null;
        
        private readonly List<ItineraryButton> _itineraryButtons = new List<ItineraryButton>();
        private int _currentItinerarySelectIndex = 0;
        private void Awake()
        {
            updater.SubmitButton.onClick.AddListener(Submit);
            updater.AddButton.onClick.AddListener(AddNewPlace);
            updater.RemoveButton.onClick.AddListener(RemovePlace);
            
            updater.NextItineraryButton.onClick.AddListener(ShowNextItinerarySelectWindow);
            updater.PreviousItineraryButton.onClick.AddListener(ShowPrevItinerarySelectWindow);

            foreach (var window in updater.ItinerarySelectWindows)
            {
                var buttons = window.GetComponentsInChildren<PlaceButton>();
                foreach (var placeButton in buttons)
                {
                    placeButton.SetButton(SetSelectedItinerary);
                }
            }
        }

        private void SetSelectedItinerary(Sprite obj)
        {
            if (_selectedItineraryButtonOrNull == null)
            {
                Debug.LogError("선택된 버튼없음");
                return;
            }
            _selectedItineraryButtonOrNull.SetButton(obj);
            _selectedItineraryButtonOrNull = null;
            updater.CloseItinerarySelectWindow();
            
            SetSubmitButtonActivity();
        }

        private void RemovePlace()
        {
            var itineraryButton = _itineraryButtons[^1];
            _itineraryButtons.Remove(itineraryButton);
            Destroy(itineraryButton.gameObject);
            
            SetSubmitButtonActivity();
        }

        private void AddNewPlace()
        {
            ItineraryButton newButton = Instantiate(itineraryButtonPrefab, updater.ItineraryPlanContent);
            newButton.Button.onClick.AddListener(() => SelectPlace(newButton));
            _itineraryButtons.Add(newButton);
            SetSubmitButtonActivity();
        }
        
        public override void Show()
        {
            updater.gameObject.SetActive(true);
            updater.CloseItinerarySelectWindow();
            updater.SubmitButton.gameObject.SetActive(false);
        }
        
        private void Submit()
        {
            //저장
            Close();
        }

        private void SelectPlace(ItineraryButton obj)
        {
            _currentItinerarySelectIndex = 0;
            updater.OpenItinerarySelectWindow(_currentItinerarySelectIndex);
            _selectedItineraryButtonOrNull = obj;
            SetSubmitButtonActivity();
        }

        private void SetSubmitButtonActivity()
        {
            if (_itineraryButtons.Count == 0)
            {
                updater.SubmitButton.gameObject.SetActive(false);
                return;
            }

            foreach (var button in _itineraryButtons)
            {
                if(button.IsInitialized)
                    continue;
                
                updater.SubmitButton.gameObject.SetActive(false);
                return;
            }
            
            updater.SubmitButton.gameObject.SetActive(true);
        }
        
        protected override void Close()
        {
            //끝
            updater.gameObject.SetActive(false);
            base.Close();
        }

        private void ShowNextItinerarySelectWindow()
        {
            updater.ItinerarySelectWindows[_currentItinerarySelectIndex].gameObject.SetActive(false);
            _currentItinerarySelectIndex++;

            if (_currentItinerarySelectIndex >= updater.ItinerarySelectWindows.Length)
            {
                _currentItinerarySelectIndex = 0;
            }
            
            updater.ItinerarySelectWindows[_currentItinerarySelectIndex].gameObject.SetActive(true);
            
            SetTitle();
        }

        private void ShowPrevItinerarySelectWindow()
        {
            updater.ItinerarySelectWindows[_currentItinerarySelectIndex].gameObject.SetActive(false);
            _currentItinerarySelectIndex--;

            if (_currentItinerarySelectIndex < 0)
            {
                _currentItinerarySelectIndex = updater.ItinerarySelectWindows.Length - 1;
            }
            
            updater.ItinerarySelectWindows[_currentItinerarySelectIndex].gameObject.SetActive(true);

            SetTitle();
        }
        
        //임시 하드코딩
        private void SetTitle()
        {
            string title = "";
            switch (_currentItinerarySelectIndex)
            {
                case 0:
                    title = "Hotel";
                    break;
                case 1:
                    title = "Transportation";
                    break;
                case 2:
                    title = "Sight";
                    break;
            }
            
            updater.ItinerarySelectWindowTitle.text = title;
        }
    }
}
