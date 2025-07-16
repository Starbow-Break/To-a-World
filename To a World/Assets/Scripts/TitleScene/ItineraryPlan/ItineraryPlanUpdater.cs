using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TitleScene
{
    public class ItineraryPlanUpdater : MonoBehaviour
    {
        [SerializeField] private Button submitButton;
        public Button SubmitButton => submitButton;

        [SerializeField] private Button addButton;
        public Button AddButton => addButton;
        
        [SerializeField] private Button removeButton;
        public Button RemoveButton => removeButton;

        [SerializeField] private GameObject[] itinerarySelectWindows;
        public GameObject[] ItinerarySelectWindows => itinerarySelectWindows;
        
        [SerializeField] private Button nextItineraryButton;
        public Button NextItineraryButton => nextItineraryButton;
        
        [SerializeField] private Button previousItineraryButton;
        public Button PreviousItineraryButton => previousItineraryButton;
        
        [SerializeField] private Transform itineraryPlanContent;
        public Transform ItineraryPlanContent => itineraryPlanContent;

        [SerializeField] private GameObject itinerarySelectWindowBackground;
        public GameObject ItinerarySelectWindowBackground => itinerarySelectWindowBackground;
        
        [SerializeField] private TMP_Text itinerarySelectWindowTitle;
        public TMP_Text ItinerarySelectWindowTitle => itinerarySelectWindowTitle;
        
        public void OpenItinerarySelectWindow(int index)
        {
            itinerarySelectWindowBackground.SetActive(true);
            itinerarySelectWindows[index].SetActive(true);
        }

        public void CloseItinerarySelectWindow()
        {
            itinerarySelectWindowBackground.SetActive(false);
            foreach (var itinerarySelectWindow in itinerarySelectWindows)
            {
                itinerarySelectWindow.SetActive(false);
            }
        }
    }
}
