using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TitleScene.ItineraryPlan
{
    public class ItineraryButton : MonoBehaviour
    {
        [SerializeField] private Image placeImage;
        [SerializeField] private Button button;
        
        public bool IsInitialized { get; private set;}
        public Button Button => button;
        public void SetButton(Sprite sprite)
        {
            placeImage.gameObject.SetActive(true);
            placeImage.sprite = sprite;
            IsInitialized = true;
        }
        
    }
}
