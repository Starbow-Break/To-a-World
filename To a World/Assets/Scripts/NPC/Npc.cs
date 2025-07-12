using UnityEngine;

public class Npc : MonoBehaviour
{
    [field: SerializeField] public NpcInfo Info { get; private set; }
    [SerializeField] private Collider _interactionCollider;

    private void Awake()
    {
        if (_interactionCollider == null)
        {
            _interactionCollider = GetComponentInChildren<Collider>();

            if (_interactionCollider == null)
            {
                Debug.LogError("No Collider attached to Npc. Npc can't interact player.");
            }
        }
    }

    public void Sleep()
    {
        _interactionCollider.enabled = false;
    }

    public void WakeUp()
    {
        _interactionCollider.enabled = true;
    }
    
    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<NpcEvents>().EnteredNpc(this);
        }
    }
    
    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<NpcEvents>().ExitedNpc(this);
        }
    }
}
