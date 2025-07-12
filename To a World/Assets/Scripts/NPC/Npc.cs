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
                Debug.LogWarning("No Collider attached to Npc. Npc can't interact player.");
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

    public void SpeakText(string text)
    {
        //  TODO : 인자로 받은 문자열 그대로 말해주는 로직 구성
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
