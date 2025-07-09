using TTSSystem;
using UnityEngine;

public class TalkableNpc : MonoBehaviour, INpc
{
    [field: SerializeField] 
    public UnityRealtimeTTSClient TTSResultPlayer { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<NpcEvents>().EnteredNpc(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.PlayerTag))
        {
            GameEventsManager.GetEvents<NpcEvents>().ExitedNpc(this);
        }
    }
}