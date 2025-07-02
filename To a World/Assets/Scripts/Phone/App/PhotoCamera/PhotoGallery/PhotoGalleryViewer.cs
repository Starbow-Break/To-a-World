using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Phone
{
    public class PhotoGalleryViewer : MonoBehaviour
    {
        [SerializeField] private PhotoGalleryUpdater updater;
        [SerializeField] private PhotoButton photoButtonPrefab;
        [SerializeField] private SnapScroller snapScroller;
        
        private readonly List<PhotoButton> _photoButtons = new List<PhotoButton>();
        private PhotoButton _currentPhotoButton = null;
        
        public bool IsOpened => updater.gameObject.activeSelf;

        public void OpenGallery()
        {
            updater.Open();

            if (_photoButtons.Count == 0)
                return;

            for (int i = 0; i < _photoButtons.Count; i++)
            {
                if (i == _photoButtons.Count - 1)
                    _photoButtons[i].SetSelected();
                else
                    _photoButtons[i].SetUnselected();
            }

            SelectPhoto(_photoButtons[_photoButtons.Count - 1]);
        }

        public void CloseGallery()
        {
            updater.Close();
            _currentPhotoButton = null;
        }
        
        public void AddPhoto(Texture2D photo)
        {
            PhotoButton newPhotoButton = Instantiate(photoButtonPrefab, updater.ContentTransform);

            // float newX = 0f;
            // if (_photoButtons.Count > 0)
            // {
            //     RectTransform rt = _photoButtons[^1].RectTransform;
            //     newX = rt.anchoredPosition.x + rt.sizeDelta.x + 10f;
            // }
            // newPhotoButton.RectTransform.anchoredPosition = new Vector2(newX, 0f);
            newPhotoButton.SetImage(photo);
            newPhotoButton.gameObject.transform.SetAsFirstSibling();
            newPhotoButton.OnButtonClick += SelectPhoto;
            //newPhotoButton.OnButtonClick += MovePhoto;
            
            _photoButtons.Add(newPhotoButton);
            snapScroller.AddElement(newPhotoButton.gameObject.GetComponent<RectTransform>());
        }

        private void SelectPhoto(PhotoButton photoButton)
        {
            ShowPhoto(photoButton.Texture);

            if (_currentPhotoButton != null)
            {
                _currentPhotoButton.SetUnselected();
            }
            
            _currentPhotoButton = photoButton;
            photoButton.SetSelected();
        }
        private void ShowPhoto(Texture2D photo)
        {
            updater.PhotoPreview.texture = photo;
        }

        private void MovePhoto(PhotoButton photoButton)
        {
            if (photoButton.TryGetComponent<RectTransform>(out var rectTransform))
            {
                snapScroller.SetToSnapPoint(rectTransform);
            }
        }
    }
}
