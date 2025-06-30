using System;
using UnityEngine;
using UnityEngine.UI;

namespace Phone
{
    public class PhotoButton : MonoBehaviour, IScrollingSelectable
    {
        [SerializeField] private RawImage photo;
        [SerializeField] private Button button;
        [SerializeField] private float sizeDelta = 1.3f;
        
        private Texture2D _texture = null;
        private float _originalWidth = 0f;
        private float _originalHeight = 0f;
        
        public Texture2D Texture => _texture;
        public event Action<PhotoButton> OnButtonClick;

        public void SetImage(Texture2D texture)
        {
            photo.texture = texture;
            _texture = texture;
        }

        public void SetSelected()
        {
            photo.rectTransform.sizeDelta = new Vector2(_originalWidth * sizeDelta, _originalHeight * sizeDelta);
        }
        
        public void SetUnselected()
        {
            photo.rectTransform.sizeDelta = new Vector2(_originalWidth, _originalHeight);
        }

        private void Awake()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            _originalWidth = rectTransform.rect.width;
            _originalHeight = rectTransform.rect.height;
        }
        private void OnEnable()
        {
            button.onClick.AddListener(() => OnButtonClick?.Invoke(this));
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(() => OnButtonClick?.Invoke(this));
        }

        public void Select()
        {
            OnButtonClick?.Invoke(this);
        }
    }
}
