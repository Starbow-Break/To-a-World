using MagicaCloth2;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SeatBelt : MonoBehaviour
{
    [field: SerializeField] public BeltBuckle Buckle { get; private set; }
    [SerializeField] private MagicaCloth _cloth;
    
    private void Start()
    {
        // 테스트용
        GameEventsManager.GetEvents<QuestEvents>().StartQuest("SeatBeltQuest");
    }

    private void OnEnable()
    {
        Buckle.AddListenerOnSelectEntered(OnConnect);
    }

    private void OnDisable()
    {
        Buckle.RemoveAllListenersOnSelectEntered();
    }

    private void OnConnect(SelectEnterEventArgs args)
    {
        _cloth.enabled = false;
        GameEventsManager.GetEvents<SeatBeltEvents>().ConnectedBelt();
    }
}
