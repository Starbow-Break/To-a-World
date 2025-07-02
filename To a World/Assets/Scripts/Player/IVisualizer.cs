using UnityEngine;

public interface IVisualizer<T> where T : MonoBehaviour
{
    public void UpdateVisual();
}