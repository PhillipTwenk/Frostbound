using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dialogues;
using EntityActions.WorkersScripts;
using UI.UIManagers;
using Unitilities;
using UnityEngine;
using UnityEngine.AI;

public class RocketFlightControl : MonoBehaviour
{
    [Header("Flight Parameters")] 
    [Tooltip("Какое время ракета будет отсутствовать между анимациями")] public float FlightBetweetAnimationsTime;
    
    [Header("Game Events")]
    [Tooltip("Ивент запуска полета ракеты")] public GameEvent StartFlyRocketGameEvent;
    [Tooltip("Ивент окончания ожидания ракеты из космоса ( сколько она будет деактивирована ) ")] public GameEvent EndSpacewalkRocketAwaitStartEvent;
    
    [Header("Components")]
    public Animator animator;

    [Header("Prefabs")] 
    public List<GameObject> WorkersPrefab;

    [Header("Points")] 
    public Transform SpawnWorker;

    [Header("Temporary name")] 
    public int currentWorkerType;
    
    
    
    
    
    private void Start()
    {
        currentWorkerType = -1;
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
        currentWorkerType = workerType;
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

    /// <summary>
    /// Вызвается при окончании анимаций полета
    /// </summary>
    public async void SpawnWorkerStart()
    {
        await SpawnNewWorker(currentWorkerType);
    }

    /// <summary>
    /// Спавн нового раочего около космопорта
    /// </summary>
    /// <param name="workerType"></param>
    public async Task SpawnNewWorker(int workerType)
    {
        GameObject newWorker = Instantiate(WorkersPrefab[workerType - 1]);
        
        //Инициализация расположения
        NavMeshAgent agent = newWorker.transform.GetChild(0).GetComponent<NavMeshAgent>();
        agent.enabled = false;
                
        newWorker.transform.position = Vector3.zero;
        newWorker.transform.rotation = Quaternion.Euler(0,0,0);
        newWorker.transform.localScale = Vector3.one;
                
        newWorker.transform.GetChild(0).transform.position = SpawnWorker.position;
                
        agent.enabled = true;
                
        // Иницилиазация игровых данных
        GameObject newWorkerСomponentsContainingObject = newWorker.transform.GetChild(0).gameObject;
        WorkerData workerData = newWorkerСomponentsContainingObject.GetComponent<WorkerData>();
        workerData.IsWorkerAtWork = false;
        workerData.unitType = (UnitType)workerType;

        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        playerSaveData.workers.Add(WorkersPrefab[workerType - 1]);
        TransformData transformData = new TransformData(newWorkerСomponentsContainingObject.transform);
        playerSaveData.workersTransform.Add(transformData);
        WorkersDataSaveData workersDataSaveData = new WorkersDataSaveData(workerData);
        playerSaveData.workerDatas.Add(workersDataSaveData);

        workerData.SaveListIndex = playerSaveData.workerDatas.IndexOf(workersDataSaveData);
        playerSaveData.workerDatas[workerData.SaveListIndex].SaveListIndex = workerData.SaveListIndex;
        
        workerData.gameObject.GetComponent<IWorkerUnit>().MainCamera =
            GeneralWorkersControl.MainCamera;

        await GeneralWorkersControl.Instance?.IncreasedFoodIntake(UIManager.Instance.CurrentConstNumberOfNewWorkersAfterCalling);
        
        
        
        GeneralWorkersControl.Instance.NumberOfFreeUnits += 1;
        GeneralWorkersControl.Instance.CurrentValueOfUnits += 1;
        GeneralWorkersControl.Instance.CurrentValueOfWorkers += 1;
        Debug.Log($"<color=green>Свободные рабочие + 1: {GeneralWorkersControl.Instance.NumberOfFreeUnits}</color>");

        currentWorkerType = -1;
        
        DialogueManager.OnWorkerCalled?.Invoke(ActionTypeCallWorker.EndAwaitRocket);
    }
}
