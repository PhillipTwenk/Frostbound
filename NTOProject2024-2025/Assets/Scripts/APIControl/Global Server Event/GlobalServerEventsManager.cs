using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.UIManagers;
using UnityEngine;

namespace APIControl.Global_Server_Event
{
    public class GlobalServerEventsManager : MonoBehaviour
    {
        public List<ServerEvent> ActiveEvents = new List<ServerEvent>();
        private Dictionary<string, ServerEvent> _eventDictionary = new Dictionary<string, ServerEvent>();
        private Coroutine _eventCheckCoroutine;

        public static event Action<ServerEvent> OnEventAdded;
        public static event Action<ServerEvent> OnEventRemoved;
        public static event Action<ServerEvent> OnEventUpdated;

        private void Start()
        {
            StartEventMonitoring();
        }

        /// <summary>
        /// Инициализация подписок ивентов и начало мониоринга 
        /// </summary>
        public async void StartEventMonitoring()
        {
            await InitialLoadEvents();
            await EventMonitoringRoutine();
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task InitialLoadEvents()
        {
            var events = await APIManager.Instance.GetServerEventList();
            foreach (var serverEvent in events)
            {
                await RegisterEvent(serverEvent);
            }
        }
        
        /// <summary>
        /// Регистрация ивента, подписка скриптов, использующих глобальные ивенты, на нужные им Action
        /// </summary>
        /// <param name="serverEvent"></param>
        private async Task RegisterEvent(ServerEvent serverEvent)
        {
            ActiveEvents.Add(serverEvent);
            _eventDictionary.Add(serverEvent.name, serverEvent);
            OnEventAdded?.Invoke(serverEvent);
            
            // Запускаем мониторинг времени для этого ивента
            await MonitorEventTime(serverEvent);
        }

        private async Task EventMonitoringRoutine()
        {
            while (true)
            {
                await Task.Delay(120*1000); // Проверка каждые 30 секунд
                await CheckForEventUpdates();
            }
        }

        private async Task CheckForEventUpdates()
        {
            var currentEvents = await APIManager.Instance.GetServerEventList();
            
            // Проверка новых/обновленных ивентов
            foreach (var serverEvent in currentEvents)
            {
                if (_eventDictionary.TryGetValue(serverEvent.name, out var existingEvent))
                {
                    if (!EventsEqual(existingEvent, serverEvent))
                    {
                        UpdateEvent(existingEvent, serverEvent);
                    }
                }
                else
                {
                    await RegisterEvent(serverEvent);
                }
            }

            // Проверка удаленных ивентов
            List<string> toRemove = new List<string>();
            foreach (var kvp in _eventDictionary)
            {
                if (!currentEvents.Exists(e => e.name == kvp.Key))
                {
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
            ActiveEvents.Remove(serverEvent);
            _eventDictionary.Remove(serverEvent.name);
            OnEventRemoved?.Invoke(serverEvent);
            
            // Принудительно завершаем ивент если он был активен
            serverEvent.OnEventEnd?.Invoke();
        }

        private void UpdateEvent(ServerEvent existing, ServerEvent updated)
        {
            // Обновляем только изменяемые поля
            existing.text = updated.text;
            existing.duration_in_minutes = updated.duration_in_minutes;
            existing.start_date_time = updated.start_date_time;
            
            OnEventUpdated?.Invoke(existing);
        }

        private async Task MonitorEventTime(ServerEvent serverEvent)
        {
            DateTime startTime = DateTime.Parse(serverEvent.start_date_time);
            DateTime endTime = startTime.AddMinutes(serverEvent.duration_in_minutes);

            // Ожидаем начала
            while (DateTime.Now < startTime)
            {
                // Уведомление за 5 минут до начала
                if ((startTime - DateTime.Now).TotalMinutes <= 5)
                {
                    UIManager.Instance.ShowNotificationPanel();
                }

                await Task.Delay(1000);
            }

            // Запускаем ивент
            serverEvent.OnEventStart?.Invoke();
            
            // Ожидаем окончания (с динамической проверкой длительности)
            while (DateTime.Now < endTime)
            {
                // Если длительность изменилась - обновляем endTime
                endTime = startTime.AddMinutes(serverEvent.duration_in_minutes);
                await Task.Delay(1000);
            }

            // Завершаем ивент
            serverEvent.OnEventEnd?.Invoke();
            UnregisterEvent(serverEvent);
        }

        private bool EventsEqual(ServerEvent a, ServerEvent b)
        {
            return a.text == b.text &&
                   a.duration_in_minutes == b.duration_in_minutes &&
                   a.start_date_time == b.start_date_time;
        }
    }
}