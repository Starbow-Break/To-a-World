using UnityEngine;
using UnityEngine.UI;

public class CountryButton : ARadioButton
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedOutline;

    public Sprite SelectedSprite => image.sprite;
    
    private void Awake()
    {
        selectedOutline.SetActive(false);
    }

    public override void SetSelected()
    {
        selectedOutline.SetActive(true);
    }

    public override void SetDeselected()
    {
        selectedOutline.SetActive(false);
    }
}
