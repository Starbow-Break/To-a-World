using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Phone
{
    public class PhotoCameraApp : AApp
    {
        [SerializeField] private PhotoCameraWindow window;
        [SerializeField] private Camera photoCamera;
        
        [SerializeField] private RenderTexture photoRenderTexture;
        
        [SerializeField] private PhotoGalleryViewer galleryViewer;
        
        public override void Open()
        {
            window.Open();
            photoCamera.gameObject.SetActive(true);
        }

        public override void Close()
        {
            window.Close();
            photoCamera.gameObject.SetActive(false);
        }

        public override void Back()
        {
            if (galleryViewer.IsOpened)
            {
                galleryViewer.CloseGallery();
                return;
            }
            base.Back();
        }

        private void Start()
        {
            window.gameObject.SetActive(false);
            photoCamera.gameObject.SetActive(false);
            
            // 카메라에 렌더텍스처 연결
            photoCamera.targetTexture = photoRenderTexture;

            // 핸드폰 UI에 해당 텍스처 연결
            window.Screen.texture = photoRenderTexture;
        }

        private void OnEnable()
        {
            window.TakePhotoButton.onClick.AddListener(TakePhoto);
            window.GalleryButton.onClick.AddListener(()=> galleryViewer.OpenGallery());
        }

        private void OnDisable()
        {
            window.TakePhotoButton.onClick.RemoveListener(TakePhoto);
            window.GalleryButton.onClick.RemoveListener(()=> galleryViewer.OpenGallery());
        }
        
        private void TakePhoto()
        {
            StartCoroutine(TakePhotoCoroutine());
        }

        private IEnumerator TakePhotoCoroutine()
        {
            yield return new WaitForEndOfFrame();
            
            RenderTexture.active = photoRenderTexture;
            Texture2D photoTexture = new Texture2D(photoRenderTexture.width, photoRenderTexture.height);
            photoTexture.ReadPixels(new Rect(0, 0, photoRenderTexture.width, photoRenderTexture.height), 0, 0);
            photoTexture.Apply();
            RenderTexture.active = null;
            
            window.PhotoPreview.texture = photoTexture;
            galleryViewer.AddPhoto(photoTexture);
            
            GameEventsManager.GetEvents<CameraEvents>().CameraShot(photoCamera);
        }
    }
}
