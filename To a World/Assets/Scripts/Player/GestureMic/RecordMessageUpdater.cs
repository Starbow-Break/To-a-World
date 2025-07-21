using System;
using TMPro;
using UnityEngine;

public class RecordMessageUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void SetActive(bool value)
    {
        _text.enabled = value;
    }
}
