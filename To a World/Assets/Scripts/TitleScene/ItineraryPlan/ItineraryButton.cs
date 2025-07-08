using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TitleScene.ItineraryPlan
{
    public class ItineraryButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text placeNameText;
        [SerializeField] private Image placeImage;
        [SerializeField] private Button button;
        
        public PlaceDesc? PlaceDescOrNull { get; private set; } = null;
        public Button Button => button;
        public void SetButton(PlaceDesc placeDesc)
        {
            PlaceDescOrNull = placeDesc;
            placeNameText.text = placeDesc.placeName;
            placeImage.sprite = placeDesc.sprite;
        }
        
    }
}
