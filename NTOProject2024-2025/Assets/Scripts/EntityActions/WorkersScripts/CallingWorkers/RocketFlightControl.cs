using System;
using System.Threading.Tasks;
using Unitilities;
using UnityEngine;

public class RocketFlightControl : MonoBehaviour
{
    [Header("Flight Parameters")] 
    [Tooltip("Какое время ракета будет отсутствовать между анимациями")] public float FlightBetweetAnimationsTime;
    
    [Header("Game Events")]
    [Tooltip("Ивент запуска полета ракеты")] public GameEvent StartFlyRocketGameEvent;
    [Tooltip("Ивент окончания ожидания ракеты из космоса ( сколько она будет даективирована ) ")] public GameEvent EndSpacewalkRocketAwaitStartEvent;
    
    [Header("Components")]
    public Animator animator;
    
    
    
    
    
    private void Start()
    {
        UIManager.CallingNewWorkerEvent += StartFlyRocket;
    }

    private void OnDestroy()
    {
        UIManager.CallingNewWorkerEvent -= StartFlyRocket;
    }


    /// <summary>
    /// Метод, запускающий цикл анимаций полета ракеты
    /// Вызывается из ивента CallingNewWorkerEvent в UI Manager
    /// </summary>
    public void StartFlyRocket(int workerType)
    {
        StartFlyRocketGameEvent.TriggerEvent();
    }

    /// <summary>
    /// Запускает пропажу ракеты ( выход в космос ) на указанное время 
    /// </summary>
    public void StartSpacewalkAwait()
    {
        Utility.Invoke(this, () =>
        {
            EndSpacewalkRocketAwaitStartEvent.TriggerEvent();
        }, FlightBetweetAnimationsTime);

    } 
}
