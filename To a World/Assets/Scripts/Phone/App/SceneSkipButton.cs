using UnityEngine;
using UnityEngine.Events;

namespace Phone
{
    public class SceneSkipButton : AppButton
    {
        public UnityAction OnClick;

        private void Start()
        {
            if(_button != null)
                _button.onClick.AddListener(OnClick);
        }
    }
}

