using EntityActions.WorkersScripts;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[System.Serializable]
public enum UnitType
{
    Player,
    Beekeeper,
    Constructor,
    MainDrone
}

[System.Serializable]
public class WorkerData : MonoBehaviour
{
    public UnitType unitType;
    public bool IsWorkerAtWork;
    public int SaveListIndex;
    public DroneSaveData droneSaveData;

    private void Start()
    {
        JSONSerializeManager.streamingDataSaveEvent += SaveWorkerPositionEndGame;
        JSONSerializeManager.streamingDataSaveEvent += SaveLogisticsStorage;
    }
    private void OnDestroy()
    {
        JSONSerializeManager.streamingDataSaveEvent -= SaveWorkerPositionEndGame;
        JSONSerializeManager.streamingDataSaveEvent -= SaveLogisticsStorage;
    }

    #region Methods

    /// <summary>
    /// Сохранение расположения рабочего после выхода при сохранении JSON
    /// </summary>
    public void SaveWorkerPositionEndGame()
    {
        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        TransformData transformData = new TransformData(this.gameObject.transform);
        WorkersDataSaveData workersDataSaveData = new WorkersDataSaveData(this.gameObject.GetComponent<WorkerData>());

        playerSaveData.workersTransform[SaveListIndex] = transformData;
        playerSaveData.workerDatas[SaveListIndex] = workersDataSaveData;
        
        Debug.Log($"<color=green> Сохранено расположение рабочего\nНомер в списке: {SaveListIndex}\nКоординаты: x:{transformData.position.x}, y:{transformData.position.y}, z:{transformData.position.z}");
    }

    /// <summary>
    /// Сохранение значения локального логистического хранилища 
    /// </summary>
    public void SaveLogisticsStorage()
    {
        IWorkerUnit unitController = GetComponent<IWorkerUnit>();
        if (unitController is IUnitLogistics)
        {
            DroneMovementController droneMovementController = unitController as DroneMovementController;
            droneSaveData = new DroneSaveData(droneMovementController);
        }
    }


    /// <summary>
    /// Инициализация логистического храналища
    /// </summary>
    public void InitializeLogisticsStorage()
    {
        IWorkerUnit unitController = GetComponent<IWorkerUnit>();
        if (unitController is IUnitLogistics)
        {
            DroneMovementController droneMovementController = unitController as DroneMovementController;
            foreach (var buildingData in CurrentPlayersDataControl.currentBuildingsDatas)
            {
                if (buildingData.SaveListIndex == droneSaveData.buildingDataLogistics.SaveListIndex)
                {
                    droneMovementController.buildingDataLogistics = buildingData;
                }
            }

            droneMovementController.isFlyNow = droneSaveData.isFlyNow;
            droneMovementController.isLanding = droneSaveData.isLanding;
            droneMovementController.isPlaceNow = droneSaveData.isPlaceNow;
            droneMovementController.isTakingOff = droneSaveData.isTakingOff;
            droneMovementController.isMovingToLandingSpot = droneSaveData.isMovingToLandingSpot;
            droneMovementController.IsLogisticsCycleActive = droneSaveData.IsLogisticsCycleActive;
            droneMovementController.LogisticsStorage = droneSaveData.LogisticsStorage;

            Animator anim = droneMovementController.gameObject.GetComponent<Animator>();
            NavMeshAgent navMeshAgent = droneMovementController.gameObject.GetComponent<NavMeshAgent>();
            if (droneMovementController.isFlyNow)
            {
                navMeshAgent.enabled = true;
                anim.SetBool(droneMovementController.droneFly_AK, true);
                navMeshAgent.Warp(droneMovementController.transform.position);
                if (droneMovementController.UnitPointOfDestination != null && navMeshAgent.isActiveAndEnabled)
                {
                    navMeshAgent.SetDestination(droneMovementController.UnitPointOfDestination.position);
                }
                if (!droneMovementController.IsLogisticsCycleActive)
                {
                    GeneralWorkersControl.SelectedUnit = droneMovementController;
                    GeneralWorkersControl.SelectedUnit.isSelected = true;
                    GeneralWorkersControl.SelectedUnit.OutlineRotate.SetActive(true);
                }
                else if (droneMovementController.buildingDataLogistics != null)
                {
                    if (droneMovementController.LogisticsStorage == 0)
                    {
                        droneMovementController.SelectedBuilding = droneMovementController.buildingDataLogistics.gameObject;
                    }
                    else if (droneMovementController.LogisticsStorage > 0)
                    {
                        droneMovementController.SelectedBuilding =
                            BaseUpgradeConditionManager.buildingDataMB.gameObject;
                    }
                    droneMovementController.LogisticsCycleMovementHandler();
                }
            }
            else
            {
                anim.SetBool(droneMovementController.droneFly_AK, false);
                navMeshAgent.enabled = false;
            }
        }
    }

    #endregion
}


