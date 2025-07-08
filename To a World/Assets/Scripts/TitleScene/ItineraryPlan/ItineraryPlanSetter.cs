using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace TitleScene.ItineraryPlan
{
    public class ItineraryPlanSetter : AShowableUI
    {
        [SerializeField] private ItineraryPlanUpdater updater;
        [SerializeField] private PlaceTable table;
        [SerializeField] private PlaceButton placeButtonPrefab;
        [SerializeField] private ItineraryButton itineraryButtonPrefab;

        [CanBeNull] private ItineraryButton _selectedItineraryButtonOrNull = null;
        
        private List<ItineraryButton> _itineraryButtons = new List<ItineraryButton>();
        
        private void Awake()
        {
            updater.SubmitButton.onClick.AddListener(Submit);
            updater.AddButton.onClick.AddListener(AddNewPlace);
            updater.RemoveButton.onClick.AddListener(RemovePlace);
            
            foreach (var place in table.places)
            {
                PlaceButton newButton = Instantiate(placeButtonPrefab, updater.PlaceList.transform);
                newButton.SetButton(place, SetSelectedItinerary);
            }
        }

        private void SetSelectedItinerary(PlaceDesc obj)
        {
            if (_selectedItineraryButtonOrNull == null)
            {
                Debug.LogError("선택된 버튼없음");
                return;
            }
            _selectedItineraryButtonOrNull.SetButton(obj);
            _selectedItineraryButtonOrNull = null;
            updater.PlaceList.gameObject.SetActive(false);
            
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
            updater.PlaceList.gameObject.SetActive(false);
            updater.SubmitButton.gameObject.SetActive(false);
        }
        
        private void Submit()
        {
            //저장
            Close();
        }

        private void SelectPlace(ItineraryButton obj)
        {
            updater.PlaceList.gameObject.SetActive(true);
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
                if(button.PlaceDescOrNull != null)
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
    }
}
