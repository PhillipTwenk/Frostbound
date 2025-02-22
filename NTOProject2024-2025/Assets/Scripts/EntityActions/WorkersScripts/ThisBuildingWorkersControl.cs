using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public class ThisBuildingWorkersControl : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective CreateNewWorkerTutorial;
    
    [Header("Workers control")]
    public int CurrentNumberWorkersInThisBuilding;
    public int MaxValueOfWorkersInThisBuilding;
    public GameObject WorkerPrefab;

    [Header("Points")]
    public Transform buildingSpawnWorkerPointTransform;

    /// <summary>
    /// Обновление текста панели
    /// </summary>
    /// <param name="text"></param>
    public void TextChanger(TextMeshPro text)
    {
        text.text = $"Нажмите E чтобы выгрузить одного рабочего ({CurrentNumberWorkersInThisBuilding}/2)";
    }
    
    /// <summary>
    /// Спавн рабочего около здания 
    /// </summary>
    /// <param name="text"></param>
    public void SpawnWorkersInThisBuilding(TextMeshPro text)
    {
        if (CurrentNumberWorkersInThisBuilding > 0)
        {
            WorkersInterBuildingControl.Instance.NumberOfFreeWorkers += 1;
            Debug.Log($"<color=green>Свободные рабочие + 1: {WorkersInterBuildingControl.Instance.NumberOfFreeWorkers}</color>");
            CurrentNumberWorkersInThisBuilding -= 1;
            GameObject newWorker = Instantiate(WorkerPrefab, null);
            newWorker.transform.position = buildingSpawnWorkerPointTransform.position;
            newWorker.transform.rotation = buildingSpawnWorkerPointTransform.rotation;
            newWorker.transform.SetParent(null);
            newWorker.transform.GetChild(0).GetComponent<WorkerMovementController>().MainCamera = WorkersInterBuildingControl.MainCamera;
            TextChanger(text);
            if (GetComponent<BuildingData>().Title == "Жилой модуль")
            {
                CreateNewWorkerTutorial.CheckAndUpdateTutorialState();
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
