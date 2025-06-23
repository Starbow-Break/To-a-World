using UnityEngine;
using UnityEngine.UI;

namespace Phone
{
    public class PhotoCameraWindow : MonoBehaviour, IAppWindow
    {
        [SerializeField] private RawImage screen;
        public RawImage Screen => screen;
        
        [SerializeField] private Button takePhotoButton;
        public Button TakePhotoButton => takePhotoButton;
        
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
