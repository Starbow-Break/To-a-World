using UnityEngine;

public class ISingleton<T> where T: MonoBehaviour
{
    public static T Instance { get; }
}
