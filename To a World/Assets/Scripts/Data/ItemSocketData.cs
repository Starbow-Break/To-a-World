using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSocketData", menuName = "ScriptableObjects/ItemSocketData")]
public class ItemSocketData : ScriptableObject
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
