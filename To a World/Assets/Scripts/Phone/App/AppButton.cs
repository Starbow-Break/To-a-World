using UnityEngine;
using UnityEngine.UI;

public class AppButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private EAppType _appType;
    
    public EAppType GetAppType() => _appType;
}
