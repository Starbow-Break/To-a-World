using UnityEngine;
using UnityEngine.UI;

namespace Phone
{
    public class PhotoCameraWindow : MonoBehaviour, IAppWindow
    {
        [SerializeField] private RawImage screen;
        public RawImage Screen => screen;
        
        [SerializeField] private RawImage photoPreview;
        public RawImage PhotoPreview => photoPreview;
        
        [SerializeField] private Button takePhotoButton;
        public Button TakePhotoButton => takePhotoButton;
        
        [SerializeField] private Button galleryButton;
        public Button GalleryButton => galleryButton;
        
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
