using UnityEngine;

public interface IRegistry<K, V>
{
    public void Register(K id, V value);
    public void UnRegister(K id);
    public V Get(K key);
}