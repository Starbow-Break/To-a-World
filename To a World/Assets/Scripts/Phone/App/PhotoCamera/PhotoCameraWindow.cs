using UnityEngine;

namespace Phone
{
    public class PhotoCameraWindow : MonoBehaviour, IAppWindow
    {
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
