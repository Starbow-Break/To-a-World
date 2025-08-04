using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Item: MonoBehaviour
{
    [SerializeField] protected ItemData _itemData;

    public string ID { get; private set; }

    protected virtual void Awake()
    {
        if (_itemData != null)
        {
            ID = _itemData != null ? _itemData.ID : this.name;
        }
    }
}
