using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BeltBuckle : MonoBehaviour
{
    [SerializeField] private Transform _tabJointPoint;
    
    private Rigidbody _rb;
    private XRGrabInteractable _grabInteractable;
    private FixedJoint _joint;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _joint = GetComponent<FixedJoint>();
    }

    public void ConnectTab(BeltTab tab)
    {
        var tabRigidBody = tab.GetComponent<Rigidbody>();
        var tabGrabInteractable = tab.GetComponent<XRGrabInteractable>();

        _rb.isKinematic = false;
        tabRigidBody.isKinematic = false;
        
        tab.transform.position = _tabJointPoint.position;
        tab.transform.rotation = _tabJointPoint.rotation;
        
        _joint.connectedBody = tabRigidBody;

        _grabInteractable.enabled = false;
        tabGrabInteractable.enabled = false;
    }
}
