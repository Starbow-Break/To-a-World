using UnityEngine;

public interface IRegistry<K, V>
{
    public V Get(K key);
}