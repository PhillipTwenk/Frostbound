using System;
using GlobalEvents.Cataclysm_Services.interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace GlobalEvents.Cataclysm_Services
{
    public class DroneCrashGlobalEventService : MonoBehaviour, ICataclysmService
    {
        [SerializeField] private GlobalEvents.EventType neededEventType;
        public GlobalEvents.EventType NeededEventType
        {
            get
            {
                return neededEventType;
            }
        }
        
        [SerializeField] private UnityEvent activateVisual; 
        [SerializeField] private UnityEvent deactivateVisual;

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

        public static event Action ChangeDroneVisual;
        public static event Action RevertDroneVisual;

        public static event Action OnDroneBroke;
        public static event Action RevertDroneBroke;
        
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
                ChangeDroneVisual?.Invoke();
                OnDroneBroke?.Invoke();
                ActivateVisual?.Invoke();
            }
        }

        public void RevertGlobalEvent(GlobalEvent globalEvent)
        {
            if (NeededEventType == globalEvent.type)
            {
                RevertDroneVisual?.Invoke();
                RevertDroneBroke?.Invoke();
                DeactivateVisual?.Invoke();
            }
        }
        
    }
}
