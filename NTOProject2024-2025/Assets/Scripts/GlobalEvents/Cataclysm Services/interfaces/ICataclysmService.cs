using UnityEngine.Events;

namespace GlobalEvents.Cataclysm_Services.interfaces
{
    public interface ICataclysmService
    {
        public GlobalEvents.EventType NeededEventType { get; }
    
        public UnityEvent ActivateVisual { get; } 
        public UnityEvent DeactivateVisual { get; }

        public void LaunchGlobalEvent(GlobalEvent globalEvent);
        public void RevertGlobalEvent(GlobalEvent globalEvent);
    }
}
