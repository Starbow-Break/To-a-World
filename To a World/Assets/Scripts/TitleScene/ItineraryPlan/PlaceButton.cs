using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TitleScene.ItineraryPlan
{
    public class PlaceButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text nameText;
    
        public void SetButton(Action<Sprite> onClick)
        {
            button.onClick.AddListener(() => onClick.Invoke(image.sprite));
        }
    }
}
