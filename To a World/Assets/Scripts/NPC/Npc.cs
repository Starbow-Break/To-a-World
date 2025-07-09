using UnityEngine;

public class Npc : MonoBehaviour
{
    public NpcInfo Info { get; private set; }

    [SerializeField] private string _language;
    [SerializeField] private string _characters;
    
    protected void Awake()
    {
        Info = new NpcInfo(_language, _characters);
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
