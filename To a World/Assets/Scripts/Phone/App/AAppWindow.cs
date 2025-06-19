using UnityEngine;

namespace Phone
{
    public abstract class AAppWindow
    {
        [SerializeField] protected EAppType _appType = EAppType.None;
        public EAppType GetAppType() => _appType;
    
        public abstract void Open();
        public abstract void Close();
    }
}