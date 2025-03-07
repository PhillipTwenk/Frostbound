using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public class ThisBuildingWorkersControl : MonoBehaviour
{
    // [Header("Tutorial")]
    // [SerializeField] private TutorialObjective CreateNewWorkerTutorial;
    
    [Header("Workers control")]
    public int CurrentNumberWorkersInThisBuilding;
    public int MaxValueOfWorkersInThisBuilding;
    public GameObject WorkerPrefab;
    public WorkerMovementController currentWorkerInThisBuilding;
    public WorkerData CurrentWorkerDataInThisBuilding;

    [Header("Units")]
    public WorkersType suitableWorkerDataForThisBuilding;
    
    private void Start()
    {
        if (CurrentNumberWorkersInThisBuilding == 0 && CurrentWorkerDataInThisBuilding == null)
        {
            currentWorkerInThisBuilding = null;
        }
    }

    [Header("Points")]
    public Transform buildingSpawnWorkerPointTransform;

    /// <summary>
    /// Обновление текста панели
    /// </summary>
    /// <param name="text"></param>
    public void TextChanger(TextMeshPro text)
    {
        text.text = $"Нажмите E чтобы выгрузить одного рабочего ({CurrentNumberWorkersInThisBuilding}/{MaxValueOfWorkersInThisBuilding})";
    }
    
    /// <summary>
    /// Спавн рабочего около здания 
    /// </summary>
    /// <param name="text"></param>
    public void SpawnWorkersInThisBuilding(TextMeshPro text)
    {
        if (CurrentNumberWorkersInThisBuilding > 0 && CurrentWorkerDataInThisBuilding != null)
        {
            WorkersInterBuildingControl.Instance.NumberOfFreeWorkers += 1;
            Debug.Log($"<color=green>Свободные рабочие + 1: {WorkersInterBuildingControl.Instance.NumberOfFreeWorkers}</color>");
            CurrentNumberWorkersInThisBuilding -= 1;
            if (currentWorkerInThisBuilding != null)
            {
                currentWorkerInThisBuilding.gameObject.transform.parent.gameObject.SetActive(true);
                currentWorkerInThisBuilding.ReadyForWork = true;
                currentWorkerInThisBuilding.SelectedBuilding = null;
                currentWorkerInThisBuilding.ArriveForBuildBuidling = false;
                currentWorkerInThisBuilding.isSelected = false;
                currentWorkerInThisBuilding.isSelecting = false;
                currentWorkerInThisBuilding.possibilityClickOnWorker = true;
                currentWorkerInThisBuilding.OutlineRotate.SetActive(false);
                currentWorkerInThisBuilding.OutlinePOD.SetActive(false);
                currentWorkerInThisBuilding.gameObject.GetComponent<WorkerData>().IsWorkerAtWork = false;
            }
            //GameObject newWorker = Instantiate(WorkerPrefab, null);
            
            // newWorker.transform.position = buildingSpawnWorkerPointTransform.position;
            //
            // newWorker.transform.SetParent(null);
            // newWorker.transform.GetChild(0).GetComponent<WorkerMovementController>().MainCamera = WorkersInterBuildingControl.MainCamera;
            TextChanger(text);
            // if (GetComponent<BuildingData>().Title == "Жилой модуль")
            // {
            //     CreateNewWorkerTutorial.CheckAndUpdateTutorialState();
            // }
            
            JSONSerializeManager.Instance.JSONSave();
        }
        else if (CurrentWorkerDataInThisBuilding == null && CurrentNumberWorkersInThisBuilding > 0)
        {
            PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
            foreach (var workerData in playerSaveData.workerDatas)
            {
                if (workerData.IsWorkerAtWork && workerData.workerType == suitableWorkerDataForThisBuilding)
                {
                    workerData.IsWorkerAtWork = false;

                    GameObject newWorker = Instantiate(playerSaveData.workers[workerData.SaveListIndex]);
                    
                    //Инициализация расположения
                    NavMeshAgent agent = newWorker.transform.GetChild(0).GetComponent<NavMeshAgent>();
                    agent.enabled = false;
                
                    newWorker.transform.position = Vector3.zero;
                    newWorker.transform.rotation = Quaternion.Euler(0,0,0);
                    newWorker.transform.localScale = Vector3.one;
                
                    newWorker.transform.GetChild(0).transform.position = playerSaveData.workersTransform[workerData.SaveListIndex].position;
                    newWorker.transform.GetChild(0).transform.rotation = playerSaveData.workersTransform[workerData.SaveListIndex].rotation;
                    newWorker.transform.GetChild(0).transform.localScale = playerSaveData.workersTransform[workerData.SaveListIndex].scale;
                
                    agent.enabled = true;
                
                    // Иницилиазация игровых данных
                    GameObject newWorkerСomponentsContainingObject = newWorker.transform.GetChild(0).gameObject;
                    WorkerData workerDataNewWorker = newWorkerСomponentsContainingObject.GetComponent<WorkerData>();
                    workerDataNewWorker.SaveListIndex = workerData.SaveListIndex;
                    workerDataNewWorker.workerType = workerData.workerType;

                    newWorkerСomponentsContainingObject.GetComponent<WorkerMovementController>().MainCamera =
                        WorkersInterBuildingControl.MainCamera;
                    
                    
                    
                    
                    CurrentNumberWorkersInThisBuilding -= 1;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Рабочий начал движение к строению
    /// </summary>
    /// <param name="End"></param>
    /// <param name="buildingTransform"></param>
    /// <param name="movementController"></param>
    /// <param name="animator"></param>
    public void StartMovementWorkerToBuilding(bool End, Transform buildingTransform, WorkerMovementController movementController, Animator animator)
    {
        if (!End)
        {
            // Рабочий идет строить
            
            Transform workerTransform = movementController.transform;
            
            // Выбор ближайшей точки около здания, к которой надо бежать
            List<Transform> pointsOfBuildings = buildingTransform.gameObject.transform.GetChild(0).GetComponent<InteractionBuildingController>()
                .PointsOfBuildings;
            Transform pointForBuild = null;
            float distanceToPoint = 0;
            int i = 0;
            foreach (var point in pointsOfBuildings)
            {
                if (i == 0)
                {
                    pointForBuild = point;
                    distanceToPoint = Vector3.Distance(workerTransform.position, point.position);
                    i++;
                    continue;
                }
                if (Vector3.Distance(workerTransform.position, point.position) < distanceToPoint)
                {
                    pointForBuild = point;
                    distanceToPoint = Vector3.Distance(workerTransform.position, point.position);
                    i++;
                }
            }
            
            // Установка цели у NavMeshAgent 
            movementController.gameObject.GetComponent<NavMeshAgent>().CompleteOffMeshLink();
            movementController.SetWorkerDestination(pointForBuild, true);
        }
        else
        {
            // Рабочий идёт обратно
            
            movementController.gameObject.GetComponent<NavMeshAgent>().CompleteOffMeshLink();
            movementController.SetWorkerDestination(buildingTransform, true);
        }

        // Установка анимации бега 
        animator.SetBool("Running", true);
        animator.SetBool("Building", false);
        animator.SetBool("Idle", false);
    }

}
