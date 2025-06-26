using UnityEngine;

namespace Phone
{
    public abstract class AApp : MonoBehaviour
    {
        [SerializeField] private EAppType appType = EAppType.None;
        [SerializeField] private AppManager appManager;
    
        public EAppType AppType => appType;
    
        public abstract void Open();
        public abstract void Close();

        public virtual void Back()
        {
            appManager.ClosePhoneApp();
        }
    }
}
