using UnityEngine;

public class SnapAttractor: AAttractor
{
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] protected bool _syncRotation = false;
    
    protected override void MoveObjectPerFrame(GameObject obj)
    {
        obj.transform.position = transform.position + transform.rotation * _offset;

        if (_syncRotation)
        {
            obj.transform.rotation = transform.rotation;
        }
    }
}
