using UnityEngine;
using UnityEngine.UI;

namespace TitleScene
{
    public class CountrySelectUpdater : MonoBehaviour
    {
        [SerializeField] private RadioButtonGroup radioButtonGroup;
        public RadioButtonGroup RadioButtonGroup => radioButtonGroup;
        
        [SerializeField] private Button submitButton;
        public Button SubmitButton => submitButton;

        [SerializeField] private Image selectedImage;
        public Image SelectedImage => selectedImage;
    } 
}
