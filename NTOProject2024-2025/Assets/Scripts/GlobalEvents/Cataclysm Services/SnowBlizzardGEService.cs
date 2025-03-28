using System;
using GlobalEvents.Cataclysm_Services.interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace GlobalEvents.Cataclysm_Services
{
    public class SnowBlizzardGEService : MonoBehaviour, ICataclysmService
    {
        [SerializeField] private EventType neededEventType;
        [SerializeField] private UnityEvent activateVisual; 
        [SerializeField] private UnityEvent deactivateVisual;
        
        public GlobalEvents.EventType NeededEventType
        {
            get
            {
                return neededEventType;
            }
        }
        
        public UnityEvent ActivateVisual
        {
            get
            {
                return activateVisual;
            }
        }
        
        public UnityEvent DeactivateVisual
        {
            get
            {
                return deactivateVisual;
            }
        }
        
        public static event Action ChangeParametersSnowBlizzardGeEvent;
        public static event Action RevertParametersSnowBlizzardGeEvent;

        private void OnEnable()
        {
            GlobalEventsManager.OnNewEventStarted += LaunchGlobalEvent;
            GlobalEventsManager.OnNewEventEnded += RevertGlobalEvent;
        }

        private void OnDisable()
        {
            GlobalEventsManager.OnNewEventStarted -= LaunchGlobalEvent;
            GlobalEventsManager.OnNewEventEnded -= RevertGlobalEvent;
        }

        public void LaunchGlobalEvent(GlobalEvent globalEvent)
        {
            if (NeededEventType == globalEvent.type)
            {
                ChangeParametersSnowBlizzardGeEvent?.Invoke();
                ActivateVisual?.Invoke();
            }
        }

        public void RevertGlobalEvent(GlobalEvent globalEvent)
        {
            if (NeededEventType == globalEvent.type)
            {
                RevertParametersSnowBlizzardGeEvent?.Invoke();
                DeactivateVisual?.Invoke();
            }
        }
    }
}
