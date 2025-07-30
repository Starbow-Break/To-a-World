using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (String.IsNullOrEmpty(ID))
        {
            ID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
