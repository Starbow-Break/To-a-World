using TMPro;
using UnityEngine;

namespace Phone
{
    public class SpeechLoggerWindow : MonoBehaviour, IAppWindow
    {
        [SerializeField] private TMP_Text textField;

        public void SetText(string context)
        {
            textField.text = context;
        }
        
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
