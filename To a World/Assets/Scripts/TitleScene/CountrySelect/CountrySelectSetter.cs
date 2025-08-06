using System;
using UnityEngine;

namespace TitleScene
{
    public class CountrySelectSetter : AShowableUI
    {
        [SerializeField] private CountrySelectUpdater updater;
        private void Awake()
        {
            updater.SubmitButton.onClick.AddListener(Submit);

            updater.RadioButtonGroup.OnValueChanged += SetSelectedImage;
        }
        
        private void Submit()
        {
            //저장
            Close();
        }
        
        public override void Show()
        {
            updater.gameObject.SetActive(true);
            updater.SubmitButton.gameObject.SetActive(true);
        }

        private void SetSelectedImage(ARadioButton args)
        {
            if (args.TryGetComponent(out CountryButton countryButton) == false)
            {
                Debug.LogError($"CountryButton {args.name} is not a CountryButton");
            }
            updater.SelectedImage.sprite = countryButton.SelectedSprite;
        }
        
        private void ToggleSubmitButton(int obj)
        {
            if (obj == -1)
            {
                updater.SubmitButton.gameObject.SetActive(false);
                return;
            } 
            updater.SubmitButton.gameObject.SetActive(true);
        }

        protected override void Close()
        {
            updater.gameObject.SetActive(false);
            base.Close();
        }
    }
}
