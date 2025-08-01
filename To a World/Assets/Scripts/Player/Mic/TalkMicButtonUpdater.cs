using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TalkMicButtonUpdater : MonoBehaviour
{
    [SerializeField] private Image Background;
    [SerializeField] private Image Icon;
    
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    
    public void SetInteractable(bool interactable)
    {
        _button.enabled = interactable;
    }

    public void SetBackgroundColor(Color color)
    {
        Background.color = color;
    }
    
    public void SetIcon(Sprite sprite)
    {
        Icon.sprite = sprite;
    }
    
    public void AddOnClickListener(UnityAction onClickAction)
    {
        _button.onClick.AddListener(onClickAction);
    }
    
    
    public void RemoveAllOnClickListeners()
    {
        _button.onClick.RemoveAllListeners();
    }
}
