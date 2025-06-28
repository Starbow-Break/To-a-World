using UnityEngine;

public interface IRegistry<K, V>
{
    public void Register(K key, V value);
    public void UnRegister(K key);
    public V Get(K key);
}