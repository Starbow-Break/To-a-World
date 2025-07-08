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
    
        public void SetButton(PlaceDesc placeDesc, Action<PlaceDesc> onClick)
        {
            button.image.sprite = placeDesc.sprite;
            nameText.text = placeDesc.placeName;
            button.onClick.AddListener(() => onClick.Invoke(placeDesc));
        }
    }
}
