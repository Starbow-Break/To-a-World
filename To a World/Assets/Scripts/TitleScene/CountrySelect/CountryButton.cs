using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CountryButton : ARadioButton
{
    [SerializeField] private Image image;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;

    private void Awake()
    {
        image.color = _deselectedColor;
    }

    public override void SetSelected()
    {
        image.color = _selectedColor;
    }

    public override void SetDeselected()
    {
        image.color = _deselectedColor;
    }
}
