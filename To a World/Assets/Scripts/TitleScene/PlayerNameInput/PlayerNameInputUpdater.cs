using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TitleScene
{
    public class PlayerNameInputUpdater : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        public string PlayerName => _inputField.text;
    
        [SerializeField] private Button _button;
        public Button SubmitButton => _button;
    }
}
