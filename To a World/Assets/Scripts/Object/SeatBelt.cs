using MagicaCloth2;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SeatBelt : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor _beltSocket;
    [SerializeField] private XRGrabbableSnapAttractor _buckleAttractor;
    [SerializeField] private XRGrabbableSnapAttractor _tabAttractor;
    [SerializeField] private MagicaCloth _cloth;

    private Renderer _buckleAttractorModelRenderer;
    private Renderer _tabAttractorModelRenderer;
    
    private void Awake()
    {
        _buckleAttractorModelRenderer = _buckleAttractor.GetComponentInChildren<Renderer>();
        _tabAttractorModelRenderer = _tabAttractor.GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        _buckleAttractor.AddListenerOnSelectEntered(arg => _buckleAttractorModelRenderer.enabled = false);
        _buckleAttractor.AddListenerOnSelectExited(arg => _buckleAttractorModelRenderer.enabled = true);
        
        _tabAttractor.AddListenerOnSelectEntered(arg => _tabAttractorModelRenderer.enabled = false);
        _tabAttractor.AddListenerOnSelectExited(arg => _tabAttractorModelRenderer.enabled = true);
        
        _beltSocket.selectEntered.AddListener(OnConnect);
    }

    private void OnDisable()
    {
        _buckleAttractor.RemoveAllListenersOnSelectEntered();
        _buckleAttractor.RemoveAllListenersOnSelectExited();
        
        _tabAttractor.RemoveAllListenersOnSelectEntered();
        _tabAttractor.RemoveAllListenersOnSelectExited();
        
        _beltSocket.selectEntered.RemoveAllListeners();
    }

    private void OnConnect(SelectEnterEventArgs args)
    {
        _cloth.enabled = false;
        
        Destroy(_buckleAttractor.gameObject);
        Destroy(_tabAttractor.gameObject);
        
        GameEventsManager.GetEvents<SeatBeltEvents>().ConnectedBelt();
    }
}
