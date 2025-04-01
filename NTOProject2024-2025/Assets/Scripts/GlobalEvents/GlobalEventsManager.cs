using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GlobalEvents
{
    [Serializable]
    public enum EventType
    {
        Snowblizzard,
        Dronecrash
    }

    [Serializable]
    public class GlobalEvent
    {
        public string name;
        public string description;
        public string notifiationDescription;
        public int durationInMinutes;
        public EventType type;
        public Sprite sprite;
    }

    public class GlobalEventsManager : MonoBehaviour
    {
        public static event Action<GlobalEvent> OnNewEventStarted;
        public static event Action<GlobalEvent> OnNewEventEnded;
        public static event Action<GlobalEvent> OnNotificationPanelActive;

        [Header("Parameters")]
        [SerializeField, Min(1)] public int notificationTimeAttentionSec;
        [SerializeField, Min(1)] public int downAwaitLimitTimeMinutes;
        [SerializeField, Min(2)] public int upAwaitLimitTimeMinutes;
        [SerializeField, Min(1)] public int notificationBeforehandTimeMinutes;

        [Header("Global Events")]
        [SerializeField] private List<GlobalEvent> globalEvents;

        private CancellationTokenSource _cts;
        private bool _isPaused;
        private float _pauseTimeRemaining;

        private void OnValidate()
        {
            if (downAwaitLimitTimeMinutes >= upAwaitLimitTimeMinutes)
            {
                upAwaitLimitTimeMinutes = downAwaitLimitTimeMinutes + 1;
                Debug.LogWarning("upAwaitLimitTimeHours must be greater than downAwaitLimitTimeHours. Adjusted automatically.");
            }
        }

        private void OnDestroy()
        {
            Stop();
        }

        public async void Initialize()
        {
            Debug.Log("[GlobalEventsManager] Initializing global events system...");
            Stop(); // Stop any existing operations

            if (globalEvents == null || globalEvents.Count == 0)
            {
                Debug.LogError("[GlobalEventsManager] No global events defined in the list!");
                return;
            }

            _cts = new CancellationTokenSource();
            await StartEventCycle(_cts.Token);
        }

        public void Stop()
        {
            Debug.Log("[GlobalEventsManager] Stopping global events system...");
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _isPaused = false;
            _pauseTimeRemaining = 0f;
        }

        public void SetPaused(bool paused)
        {
            if (_isPaused == paused) return;

            _isPaused = paused;
            Debug.Log($"[GlobalEventsManager] System {(paused ? "paused" : "resumed")}");
        }

        private async Task StartEventCycle(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    int awaitTimeMs = SetRandomAwaitTime();
                    Debug.Log($"[GlobalEventsManager] Set random await time: {awaitTimeMs} ms");
                    await LaunchAwaitEvent(awaitTimeMs, ct);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[GlobalEventsManager] Event cycle was canceled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlobalEventsManager] Error in event cycle: {ex.Message}");
            }
        }

        private int SetRandomAwaitTime()
        {
            int awaitTime = UnityEngine.Random.Range(downAwaitLimitTimeMinutes, upAwaitLimitTimeMinutes);
            Debug.Log($"[GlobalEventsManager] Generated random await time: {awaitTime} minutes");
            
            // Convert hours to milliseconds with overflow protection
            long totalMs = (long)awaitTime * 60L * 1000L;
            return totalMs > int.MaxValue ? int.MaxValue : (int)totalMs;
        }

        private GlobalEvent SetRandomGlobalEvent()
        {
            if (globalEvents == null || globalEvents.Count == 0)
            {
                Debug.LogError("[GlobalEventsManager] No global events available to select from!");
                return null;
            }

            int randomIndex = UnityEngine.Random.Range(0, globalEvents.Count);
            Debug.Log($"[GlobalEventsManager] Selected global event index: {randomIndex} - {globalEvents[randomIndex].name}");
            return globalEvents[randomIndex];
        }

        private async Task LaunchAwaitEvent(int awaitTime, CancellationToken ct)
        {
            Debug.Log($"[GlobalEventsManager] Starting event await sequence. Total wait time: {awaitTime} ms");

            int awaitBeforeNotification = awaitTime - notificationBeforehandTimeMinutes * 60 * 1000;
            if (awaitBeforeNotification < 0)
            {
                Debug.LogWarning("[GlobalEventsManager] Notification beforehand time is greater than total await time!");
                awaitBeforeNotification = 0;
            }

            await WaitWithPause(awaitBeforeNotification, ct);
            if (ct.IsCancellationRequested) return;

            GlobalEvent globalEvent = SetRandomGlobalEvent();
            
            Debug.Log("[GlobalEventsManager] Triggering notification panel...");
            OnNotificationPanelActive?.Invoke(globalEvent);

            await WaitWithPause(notificationBeforehandTimeMinutes * 60 * 1000, ct);
            if (ct.IsCancellationRequested) return;
            
            if (globalEvent == null)
            {
                Debug.LogError("[GlobalEventsManager] Failed to start event - no valid event selected!");
                return;
            }

            Debug.Log($"[GlobalEventsManager] Starting global event: {globalEvent.name} (Duration: {globalEvent.durationInMinutes} minutes)");
            OnNewEventStarted?.Invoke(globalEvent);

            await AwaitEndGlobalEvent(globalEvent, ct);
        }

        private async Task AwaitEndGlobalEvent(GlobalEvent globalEvent, CancellationToken ct)
        {
            if (globalEvent == null)
            {
                Debug.LogError("[GlobalEventsManager] Cannot await end of null event!");
                return;
            }

            Debug.Log($"[GlobalEventsManager] Waiting {globalEvent.durationInMinutes} minutes for event to end...");
            await WaitWithPause(globalEvent.durationInMinutes * 60 * 1000, ct);
            if (ct.IsCancellationRequested) return;

            Debug.Log($"[GlobalEventsManager] Ending global event: {globalEvent.name}");
            OnNewEventEnded?.Invoke(globalEvent);
        }

        private async Task WaitWithPause(int milliseconds, CancellationToken ct)
        {
            float remainingTime = milliseconds;
            float startTime = Time.unscaledTime;
            
            while (remainingTime > 0 && !ct.IsCancellationRequested)
            {
                if (_isPaused)
                {
                    await Task.Yield();
                    continue;
                }

                float elapsed = (Time.unscaledTime - startTime) * 1000;
                remainingTime = milliseconds - elapsed;
                
                if (remainingTime > 0)
                {
                    await Task.Delay(Mathf.Min(100, (int)remainingTime), ct);
                }
            }
        }
    }
}