using System;
using TMPro;
using UnityEngine;

public class RecordMessageUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void Initialize()
    {
        InActive();
    }

    public void Active()
    {
        _text.enabled = true;
    }
    
    public void InActive()
    {
        _text.enabled = false;
    }
}
