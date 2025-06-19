using System.Collections.Generic;
using Phone;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    [SerializeField] private List<AAppWindow> _phoneApps;
    
    private EAppType _currentAppType = EAppType.None;

    public void OpenPhoneApp(EAppType appType)
    {
        if (_currentAppType == appType)
            return;

        if (_currentAppType != EAppType.None)
        {
            ClosePhoneApp();
        }
        
        _currentAppType = appType;

        foreach (var app in _phoneApps)
        {
            if (app.GetAppType() == appType)
            {
                app.Open();
                return;
            }
        }
    }

    public void ClosePhoneApp()
    {
        foreach (var app in _phoneApps)
        {
            if (app.GetAppType() == _currentAppType)
            {
                app.Close();
                _currentAppType = EAppType.None;
                return;
            }
        }
    }
}
