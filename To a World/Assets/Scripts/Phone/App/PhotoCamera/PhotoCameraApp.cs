using UnityEngine;
using UnityEngine.Serialization;

namespace Phone
{
    public class PhotoCameraApp : AApp
    {
        [SerializeField] private PhotoCameraWindow window;
        public override void Open()
        {
            window.Open();
        }

        public override void Close()
        {
            window.Close();
        }
    }
}
