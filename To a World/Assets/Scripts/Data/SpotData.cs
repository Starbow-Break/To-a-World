using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SpotData", menuName = "ScriptableObjects/SpotData")]
public class SpotData : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    
    private void OnValidate()
    {
#if UNITY_EDITOR       
        if (String.IsNullOrEmpty(ID))
        {
            ID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
