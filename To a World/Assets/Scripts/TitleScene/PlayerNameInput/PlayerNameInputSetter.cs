using System;
using UnityEngine;

namespace TitleScene
{
    public class PlayerNameInputSetter : AShowableUI
    {
        [SerializeField] private PlayerNameInputUpdater updater;
        
        private void Awake()
        {
            updater.SubmitButton.onClick.AddListener(Submit);
        }

        private void Submit()
        {
            string inputName = updater.PlayerName;
            //저장
            Close();
        }
    
        public override void Show()
        {
            updater.gameObject.SetActive(true);
        }

        protected override void Close()
        {
            base.Close();
            updater.gameObject.SetActive(false);
        }
    }
}
