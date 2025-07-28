using TMPro;
using UnityEngine;

public class TestFix : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;

    public void Update()
    {
        transform.position = _target.position + _target.rotation * _offset;
        transform.rotation = _target.rotation;
    }
}
