using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ARadioButton : MonoBehaviour, IPointerClickHandler
{
    public event Action ActOnSelect;
    public abstract void SetSelected(); 
    public abstract void SetDeselected();

    public void OnPointerClick(PointerEventData eventData)
    {
        ActOnSelect?.Invoke();
    }
}
