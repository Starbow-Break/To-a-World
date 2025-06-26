using Phone;
using UnityEngine;
using UnityEngine.UI;

namespace Phone
{
    public class PhotoGalleryUpdater : MonoBehaviour
    {
        [SerializeField] private RawImage photoPreview;
        public RawImage PhotoPreview => photoPreview;
        
        [SerializeField] private Transform contentTransform;
        public Transform ContentTransform => contentTransform;

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