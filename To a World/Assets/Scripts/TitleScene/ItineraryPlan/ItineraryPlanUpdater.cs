using UnityEngine;
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

        [SerializeField] private GameObject placeList;
        public GameObject PlaceList => placeList;
        
        [SerializeField] private Transform itineraryPlanContent;
        public Transform ItineraryPlanContent => itineraryPlanContent;
    }
}
