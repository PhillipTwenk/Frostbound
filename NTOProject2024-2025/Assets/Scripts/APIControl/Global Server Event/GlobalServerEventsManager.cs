using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UI.UIManagers;
using UnityEngine;

namespace APIControl.Global_Server_Event
{
    public class GlobalServerEventsManager : MonoBehaviour
    {
        private const string LOG_PREFIX = "[GlobalEvents] ";
        
        public List<ServerEvent> ActiveEvents = new List<ServerEvent>();
        private Dictionary<string, ServerEvent> _eventDictionary = new Dictionary<string, ServerEvent>();
        private CancellationTokenSource _cts;

        public static event Action<ServerEvent> OnEventAdded;
        public static event Action<ServerEvent> OnEventRemoved;
        public static event Action<ServerEvent> OnEventUpdated;

        public int notificationTimeAttention;

        private void Start()
        {
            Debug.Log(LOG_PREFIX + "Initializing event manager...");
            _cts = new CancellationTokenSource();
            StartEventMonitoring().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError(LOG_PREFIX + $"Monitoring failed: {t.Exception}");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnDestroy()
        {
            Debug.Log(LOG_PREFIX + "Shutting down event manager...");
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async Task StartEventMonitoring()
        {
            try
            {
                Debug.Log(LOG_PREFIX + "Starting event monitoring...");
                await InitialLoadEvents();
                await EventMonitoringRoutine();
            }
            catch (Exception ex)
            {
                Debug.LogError(LOG_PREFIX + $"Monitoring failed: {ex}");
                throw;
            }
        }

        private async Task InitialLoadEvents()
        {
            Debug.Log(LOG_PREFIX + "Loading initial events...");
            try
            {
                var events = await APIManager.Instance.GetServerEventList();
                Debug.Log(LOG_PREFIX + $"Received {events.Count} events from server");

                foreach (var serverEvent in events)
                {
                    await RegisterEvent(serverEvent);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(LOG_PREFIX + $"Failed to load initial events: {ex}");
                throw;
            }
        }

        private async Task RegisterEvent(ServerEvent serverEvent)
        {
            if (_eventDictionary.ContainsKey(serverEvent.name))
            {
                Debug.LogWarning(LOG_PREFIX + $"Event '{serverEvent.name}' already registered. Skipping...");
                return;
            }

            Debug.Log(LOG_PREFIX + $"Registering new event: {serverEvent.name} " +
                      $"(Start: {serverEvent.start_date_time}, Duration: {serverEvent.duration_in_minutes}min)");

            ActiveEvents.Add(serverEvent);
            _eventDictionary.Add(serverEvent.name, serverEvent);
            
            try
            {
                OnEventAdded?.Invoke(serverEvent);
                Debug.Log(LOG_PREFIX + $"Successfully registered event: {serverEvent.name}");
                
                await MonitorEventTime(serverEvent);
            }
            catch (Exception ex)
            {
                Debug.LogError(LOG_PREFIX + $"Failed to register event {serverEvent.name}: {ex}");
                ActiveEvents.Remove(serverEvent);
                _eventDictionary.Remove(serverEvent.name);
                throw;
            }
        }

        private async Task EventMonitoringRoutine()
        {
            Debug.Log(LOG_PREFIX + "Starting event monitoring routine...");
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    Debug.Log(LOG_PREFIX + "Checking for event updates...");
                    await CheckForEventUpdates();
                    await Task.Delay(30000, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log(LOG_PREFIX + "Monitoring routine cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.LogError(LOG_PREFIX + $"Error in monitoring routine: {ex}");
                    await Task.Delay(5000, _cts.Token); // Wait before retry
                }
            }
        }

        private async Task CheckForEventUpdates()
        {
            Debug.Log(LOG_PREFIX + "Fetching current events from server...");
            var currentEvents = await APIManager.Instance.GetServerEventList();
            Debug.Log(LOG_PREFIX + $"Server returned {currentEvents.Count} events");

            // Check for new/updated events
            foreach (var serverEvent in currentEvents)
            {
                if (_eventDictionary.TryGetValue(serverEvent.name, out var existingEvent))
                {
                    if (!EventsEqual(existingEvent, serverEvent))
                    {
                        Debug.Log(LOG_PREFIX + $"Event '{serverEvent.name}' has been updated");
                        UpdateEvent(existingEvent, serverEvent);
                    }
                }
                else
                {
                    Debug.Log(LOG_PREFIX + $"New event detected: {serverEvent.name}");
                    await RegisterEvent(serverEvent);
                }
            }

            // Check for removed events
            List<string> toRemove = new List<string>();
            foreach (var kvp in _eventDictionary)
            {
                if (!currentEvents.Exists(e => e.name == kvp.Key))
                {
                    Debug.Log(LOG_PREFIX + $"Event '{kvp.Key}' no longer exists on server");
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var eventName in toRemove)
            {
                UnregisterEvent(_eventDictionary[eventName]);
            }
        }

        private void UnregisterEvent(ServerEvent serverEvent)
        {
            Debug.Log(LOG_PREFIX + $"Unregistering event: {serverEvent.name}");
            
            ActiveEvents.Remove(serverEvent);
            _eventDictionary.Remove(serverEvent.name);
            
            try
            {
                OnEventRemoved?.Invoke(serverEvent);
                serverEvent.OnEventEnd?.Invoke();
                Debug.Log(LOG_PREFIX + $"Successfully unregistered event: {serverEvent.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError(LOG_PREFIX + $"Error unregistering event {serverEvent.name}: {ex}");
            }
        }

        private void UpdateEvent(ServerEvent existing, ServerEvent updated)
        {
            Debug.Log(LOG_PREFIX + $"Updating event {existing.name}. " +
                     $"Old duration: {existing.duration_in_minutes}min, " +
                     $"New duration: {updated.duration_in_minutes}min");

            existing.text = updated.text;
            existing.duration_in_minutes = updated.duration_in_minutes;
            existing.start_date_time = updated.start_date_time;
            
            try
            {
                OnEventUpdated?.Invoke(existing);
                Debug.Log(LOG_PREFIX + $"Event {existing.name} updated successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError(LOG_PREFIX + $"Error updating event {existing.name}: {ex}");
            }
        }

        private async Task MonitorEventTime(ServerEvent serverEvent)
        {
            Debug.Log(LOG_PREFIX + $"Starting time monitoring for event: {serverEvent.name}");
            
            try
            {
                DateTime startTime = DateTime.Parse(serverEvent.start_date_time);
                DateTime endTime = startTime.AddMinutes(serverEvent.duration_in_minutes);

                // Wait for start
                while (DateTime.Now < startTime && !_cts.Token.IsCancellationRequested)
                {
                    var timeToStart = startTime - DateTime.Now;
                    
                    // 5-minute warning
                    if (timeToStart.TotalMinutes <= 5 && timeToStart.TotalMinutes > 0)
                    {
                        Debug.Log(LOG_PREFIX + $"Event '{serverEvent.name}' starting in {timeToStart:mm\\:ss}");
                        UIManager.NotificationServerEvent?.Invoke();
                    }

                    await Task.Delay(120000, _cts.Token); // Check every 2 minutes
                }

                if (_cts.Token.IsCancellationRequested)
                    return;

                // Start event
                Debug.Log(LOG_PREFIX + $"Starting event: {serverEvent.name}");
                serverEvent.OnEventStart?.Invoke();

                // Wait for end (with dynamic duration updates)
                while (DateTime.Now < endTime && !_cts.Token.IsCancellationRequested)
                {
                    endTime = DateTime.Parse(serverEvent.start_date_time)
                        .AddMinutes(serverEvent.duration_in_minutes);
                    
                    var timeRemaining = endTime - DateTime.Now;
                    Debug.Log(LOG_PREFIX + $"Event '{serverEvent.name}' active. Time remaining: {timeRemaining:h\\:mm}");
                    
                    await Task.Delay(120000, _cts.Token); // Check every 2 minutes
                }

                if (_cts.Token.IsCancellationRequested)
                    return;

                // End event
                Debug.Log(LOG_PREFIX + $"Ending event: {serverEvent.name}");
                serverEvent.OnEventEnd?.Invoke();
                UnregisterEvent(serverEvent);
            }
            catch (OperationCanceledException)
            {
                Debug.Log(LOG_PREFIX + $"Monitoring cancelled for event: {serverEvent.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError(LOG_PREFIX + $"Error monitoring event {serverEvent.name}: {ex}");
                UnregisterEvent(serverEvent);
            }
        }

        private bool EventsEqual(ServerEvent a, ServerEvent b)
        {
            return a.text == b.text &&
                   a.duration_in_minutes == b.duration_in_minutes &&
                   a.start_date_time == b.start_date_time;
        }
    }
}