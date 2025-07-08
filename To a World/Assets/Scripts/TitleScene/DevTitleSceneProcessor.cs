using UnityEngine;
using UnityEngine.Serialization;

namespace TitleScene
{
    public class DevTitleSceneProcessor : MonoBehaviour
    {
        [SerializeField] private AShowableUI[] showableUis;

        private int _currentIndex = 0;
    
        protected void Awake()
        {
            _currentIndex = 0;
            foreach (var ui in showableUis)
            {
                ui.ActOnClose += ShowNextUI;
            }
        }
    
        private void Start()
        {
            showableUis[_currentIndex].Show();
        }

        private void ShowNextUI()
        {
            _currentIndex++;

            if (_currentIndex >= showableUis.Length)
            {
                //끝
                return;
            }
        
            showableUis[_currentIndex].Show();
        }
    }
}
