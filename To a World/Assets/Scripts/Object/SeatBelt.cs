using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SeatBelt : MonoBehaviour
{
    [field: SerializeField] public BeltBuckle Buckle { get; private set; }
    [field: SerializeField] public BeltTab Tab { get; private set; }
    
    private XRGrabInteractable _buckleGrab;
    private XRGrabInteractable _tabGrab;
    
    private Collider _buckleCollider;
    private Collider _tabCollider;

    private void Awake()
    {
        _buckleGrab = Buckle.GetComponent<XRGrabInteractable>();
        _tabGrab = Tab.GetComponent<XRGrabInteractable>();
        
        _buckleCollider = Buckle.GetComponentInChildren<Collider>();
        _tabCollider = Tab.GetComponentInChildren<Collider>();
    }

    private void Start()
    {
        // 테스트용
        GameEventsManager.GetEvents<QuestEvents>().StartQuest("SeatBeltQuest");
    }

    private void Update()
    {
        if (_buckleGrab.isActiveAndEnabled && _buckleGrab.isSelected 
            && _tabGrab.isActiveAndEnabled && _tabGrab.isSelected)
        {
            if (_buckleCollider.bounds.Intersects(_tabCollider.bounds))
            {
                Buckle.ConnectTab(Tab);
                GameEventsManager.GetEvents<SeatBeltEvents>().ConnectedBelt();
            }
        }
    }
}
