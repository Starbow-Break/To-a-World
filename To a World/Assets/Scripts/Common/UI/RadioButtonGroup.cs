using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Unity.Android.Gradle.Manifest.Action;

public class RadioButtonGroup : MonoBehaviour
{
    public List<ARadioButton> RadioButtons = new List<ARadioButton>();

    private ARadioButton SelectedButton { get; set; } = null;
    private int SelectedIndex { get; set; } = -1;  //-1 선택안됨
    public event Action<ARadioButton> OnValueChanged; // 선택된 값이 바뀔때 발생하는 이벤트
    
    public void RegisterButton(ARadioButton aRadioButton)
    {
        RadioButtons.Add(aRadioButton);
        aRadioButton.ActOnSelect += () => SetSelectedButton(RadioButtons.IndexOf(aRadioButton));
    }
    
    private void Awake()
    {
        for (int i = 0; i < RadioButtons.Count; i++)
        {
            int index = i;
            RadioButtons[i].ActOnSelect += () =>
            {
                SetSelectedButton(index);
            };
        }    
    }

    private void SetSelectedButton(int selectedIndex)
    {
        // 눌렀던 거 다시 누를 때
        if (SelectedButton == RadioButtons[selectedIndex])
        {
            SelectedButton.SetDeselected();
            SelectedButton = null;
            SelectedIndex = -1;
        }
        else
        {
            // 다른 거 누를 때
            if(SelectedButton != null)
                SelectedButton.SetDeselected();
        
            SelectedButton = RadioButtons[selectedIndex];
            SelectedIndex = selectedIndex;
            SelectedButton.SetSelected();
        }

        OnValueChanged?.Invoke(RadioButtons[selectedIndex]);
    }

}
