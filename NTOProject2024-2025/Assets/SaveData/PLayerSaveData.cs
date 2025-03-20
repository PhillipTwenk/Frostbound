using System.Collections.Generic;
using EntityActions.Movement_Control;
using EntityActions.WorkersScripts;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

/// <summary>
/// Сохранения данных о зданиях 
/// </summary>
[CreateAssetMenu(menuName = "SaveData/PlayerSaveData")]
public class PlayerSaveData : ScriptableObject, ISerializableSO
{

    public GameEvent endOfInitializationDataEvent;

    #region Реализация ISerializableSO 

    public string SerializeToJson()
    {
        // Сохраняем только данные для сериализации, а не ссылки на объекты
        var serializableData = new SerializableData
        {
            buildingNames = playerBuildings.ConvertAll(b => b.name),
            buildingsTransform = buildingsTransform,
            BuildingDatas = BuildingDatas,
            WorkersContolSaveDatas = BuildingWorkersInformationList,
            workerNames = workers.ConvertAll(w => w.name),
            WorkersTransform = workersTransform,
            workersDatas = workerDatas,
            playerName = player.name,
            playerTransform = transformPlayer
        };
        return JsonUtility.ToJson(serializableData, true);
    }

    public void DeserializeFromJson(string json)
    {
        var serializableData = JsonUtility.FromJson<SerializableData>(json);
        
        // Восстанавливаем здания по именам префабов
        playerBuildings = new List<GameObject>();
        foreach (var buildingName in serializableData.buildingNames)
        {
            GameObject prefab = Resources.Load<GameObject>($"BuildingPrefabs/{buildingName}/{buildingName}");
            Debug.Log($"BuildingPrefabs/{buildingName}/{buildingName}");
            if (prefab != null)
            {
                playerBuildings.Add(prefab);
            }
        }
        
        buildingsTransform = serializableData.buildingsTransform;
        BuildingDatas = serializableData.BuildingDatas;
        BuildingWorkersInformationList = serializableData.WorkersContolSaveDatas;

        // Восстанавливаем префабы рабочих по именам
        workers = new List<GameObject>();
        foreach (var workerName in serializableData.workerNames)
        {
            GameObject prefab = Resources.Load<GameObject>($"UnitsPrefabs/{workerName}");
            Debug.Log($"UnitsPrefabs/{workerName}");
            if (prefab != null)
            {
                workers.Add(prefab);
            }
        }

        workersTransform = serializableData.WorkersTransform;
        workerDatas = serializableData.workersDatas;
        
        
        // Восстановление префаба игрока по имени
        GameObject playerPrefab = Resources.Load<GameObject>($"UnitsPrefabs/{serializableData.playerName}");
        Debug.Log($"UnitsPrefabs/{serializableData.playerName}");
        if (playerPrefab != null)
        {
            player = playerPrefab;
        }

        transformPlayer = serializableData.playerTransform;
    }

    #endregion  
    
    
    [Header("Building Save Data")]
    [Tooltip("Игровые объекты построенных зданий")] public List<GameObject> playerBuildings;
    [Tooltip("Расположения построенных зданий")] public List<TransformData> buildingsTransform;
    [Tooltip("Игровая информация о построенных зданиях")] public List<BuildingSaveData> BuildingDatas;
    [Tooltip("Информация о рабочих внутри здания, если здание может их хранить")] public List<WorkersControlSaveData> BuildingWorkersInformationList;

    [Header("Workers Save Data")]
    [Tooltip("Игровые объекты рабочих")] public List<GameObject> workers;
    [Tooltip("Расположения рабочих")] public List<TransformData> workersTransform;
    [Tooltip("Информации о ролях и состоянии рабочих")] public List<WorkersDataSaveData> workerDatas;

    [Header("Player Save Data")] 
    [Tooltip("Игровой объект игрока")] public GameObject player;
    [Tooltip("Расположение игрока")] public TransformData transformPlayer;

    private bool IsDeleteBuidlingProcessActive;

    /// <summary>
    /// Инициализирует все построеные здания в игре
    /// </summary>
    public async void InitializeData()
    {
        #region Инициализация зданий

        IsDeleteBuidlingProcessActive = false;
        if (playerBuildings is not null)
        {
            int index = 0;
            // List<int> usedWorkerdata = new List<int>() {-1};
            foreach (var building in playerBuildings)
            {
                Debug.Log($"Инициализация здания №{index}");
                
                GameObject newBuilding = Instantiate(building);
                
                //Инициализация расположения
                newBuilding.transform.position = buildingsTransform[index].position;
                newBuilding.transform.rotation = buildingsTransform[index].rotation;
                newBuilding.transform.localScale = buildingsTransform[index].scale;

                // Иницилиазация игровых данных
                GameObject componentContainingBuilding = newBuilding.transform.GetChild(0).gameObject;
                BuildingData buildingData = componentContainingBuilding.GetComponent<BuildingData>();
                buildingData.Level = BuildingDatas[index].Level; 
                buildingData.Durability = BuildingDatas[index].Durability;
                buildingData.Storage = BuildingDatas[index].Storage;
                buildingData.SaveListIndex = BuildingDatas[index].SaveListIndex;
                buildingData.HoneyConsumption = BuildingDatas[index].HoneyConsumption;
                buildingData.Production = BuildingDatas[index].Production;
                buildingData.IsThisBuilt = true;
                
                // Инициализация мобильной базы
                if (index == 0)
                {
                    BaseUpgradeConditionManager.buildingDataMB = buildingData;
                    BaseUpgradeConditionManager.CurrentBaseLevel = buildingData.Level;
                }

                // Если здание может содержать рабочих
                if (componentContainingBuilding.GetComponent<ThisBuildingWorkersControl>())
                {
                    Debug.Log($"Здание {buildingData.name}, количество рабочих в нем: <color=green>{BuildingWorkersInformationList[index].CurrentNumberOfWorkersInThisBuilding}</color>");
                    ThisBuildingWorkersControl thisBuildingWorkersControl = componentContainingBuilding.GetComponent<ThisBuildingWorkersControl>();
                    thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding = BuildingWorkersInformationList[index].CurrentNumberOfWorkersInThisBuilding;
                    thisBuildingWorkersControl.MaxValueOfWorkersInThisBuilding = BuildingWorkersInformationList[index].MaxValueOfWorkersInThisBuilding;
                    thisBuildingWorkersControl.suitableUnitDataForThisBuilding =
                        BuildingWorkersInformationList[index].suitableUnitDataForThisBuilding;

                    GeneralWorkersControl.Instance.AddNewBuilding(thisBuildingWorkersControl);
                }
                else
                {
                    GeneralWorkersControl.Instance.AddNewBuilding(null);
                }
                
                CurrentPlayersDataControl.currentBuildingsDatas.Add(buildingData);
                index++;
            }
        }

        #endregion

        #region Инициализация рабочих

        if (workers is not null)
        {
            for (int i = 0; i < workers.Count; i++)
            {
                Debug.Log($"Инициализация рабочего №{i}");
                
                if (workerDatas[i].IsWorkerAtWork)
                {
                    Debug.Log($"Рабочий №{i} работает");
                    continue;
                }
                
                GameObject newWorker = Instantiate(workers[i]);
                
                //Инициализация расположения
                NavMeshAgent agent = newWorker.transform.GetChild(0).GetComponent<NavMeshAgent>();
                agent.enabled = false;
                
                newWorker.transform.position = Vector3.zero;
                newWorker.transform.rotation = Quaternion.Euler(0,0,0);
                newWorker.transform.localScale = Vector3.one;
                
                newWorker.transform.GetChild(0).transform.position = workersTransform[i].position;
                newWorker.transform.GetChild(0).transform.rotation = workersTransform[i].rotation;
                newWorker.transform.GetChild(0).transform.localScale = workersTransform[i].scale;
                
                agent.enabled = true;
                
                // Иницилиазация игровых данных
                GameObject newWorkerСomponentsContainingObject = newWorker.transform.GetChild(0).gameObject;
                WorkerData workerData = newWorkerСomponentsContainingObject.GetComponent<WorkerData>();
                workerData.IsWorkerAtWork = workerDatas[i].IsWorkerAtWork;
                workerData.SaveListIndex = workerDatas[i].SaveListIndex;
                workerData.unitType = workerDatas[i].unitType;
                workerData.droneSaveData = workerDatas[i].droneSaveData;
                workerData.InitializeLogisticsStorage();

                workerData.gameObject.GetComponent<IWorkerUnit>().MainCamera =
                    GeneralWorkersControl.MainCamera;

                GeneralWorkersControl.Instance.CurrentValueOfUnits += 1;
                GeneralWorkersControl.Instance.NumberOfFreeUnits += 1;
            }
        }

        #endregion

        #region Инициализация игрока

        GameObject newPlayer = Instantiate(player);
        
        //Инициализация расположения
        NavMeshAgent agentPlayer = newPlayer.transform.GetChild(0).GetComponent<NavMeshAgent>();
        agentPlayer.enabled = false;
        
        newPlayer.transform.position = Vector3.zero;
        newPlayer.transform.rotation = Quaternion.Euler(0,0,0);
        newPlayer.transform.localScale = Vector3.one;
        
        newPlayer.transform.GetChild(0).transform.position = transformPlayer.position;
        newPlayer.transform.GetChild(0).transform.rotation =transformPlayer.rotation;
        newPlayer.transform.GetChild(0).transform.localScale = transformPlayer.scale;

        agentPlayer.enabled = true;
        newPlayer.gameObject.transform.GetChild(0).gameObject.GetComponent<PlayerMovementController>().MainCamera =
            GeneralWorkersControl.MainCamera;
        
        #endregion
        
        await BuildingManager.Instance._navMeshSurfaceUnit.UpdateNavMesh(BuildingManager.Instance._navMeshSurfaceUnit.navMeshData);
        await BuildingManager.Instance._navMeshSurfaceDrone.UpdateNavMesh(BuildingManager.Instance._navMeshSurfaceDrone.navMeshData);
        
        endOfInitializationDataEvent.TriggerEvent();
    }

    /// <summary>
    /// Сбрасывает всю информацию о зданиях
    /// </summary>
    public void RevertBuildingsData()
    {
        playerBuildings.Clear();
        buildingsTransform.Clear();
        BuildingDatas.Clear();
        BuildingWorkersInformationList.Clear();
    }

    /// <summary>
    /// Удаляет конкретное здание
    /// </summary>
    /// <param name="building"> GameObject конкретного здания </param>
    public void DeleteBuilding(GameObject building)
    {
        if (playerBuildings is not null && !IsDeleteBuidlingProcessActive)
        {
            IsDeleteBuidlingProcessActive = true;

            BuildingData buildingData = building.GetComponent<BuildingData>(); 
            int indexBuilding = buildingData.SaveListIndex;

            playerBuildings.Remove(buildingData.buildingTypeSO.PrefabBuilding);
            buildingsTransform.Remove(buildingsTransform[indexBuilding]);
            BuildingDatas.Remove(BuildingDatas[indexBuilding]);
            BuildingWorkersInformationList.Remove(BuildingWorkersInformationList[indexBuilding]);

            if (buildingData.gameObject.GetComponent<ThisBuildingWorkersControl>())
            {
                GeneralWorkersControl.Instance.RemoveNewBuilding(buildingData.gameObject.GetComponent<ThisBuildingWorkersControl>());
            }

            foreach (var buildingDataCycle in BuildingDatas)
            {
                buildingDataCycle.SaveListIndex = BuildingDatas.IndexOf(buildingDataCycle);
            }
            Destroy(building.transform.parent.gameObject);

            IsDeleteBuidlingProcessActive = false;
        }
    }
    
}

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformData(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }
}

[System.Serializable]
public class BuildingSaveData
{
    public int Level;
    public int Durability;
    public List<int> Storage;
    public int SaveListIndex;
    public int HoneyConsumption;
    public List<int> Production;

    public BuildingSaveData(BuildingData buildingData)
    {
        Level = buildingData.Level;
        Durability = buildingData.Durability;
        Storage = buildingData.Storage;
        SaveListIndex = buildingData.SaveListIndex;
        HoneyConsumption = buildingData.HoneyConsumption;
        Production = buildingData.Production;
    }
}

[System.Serializable]
public class WorkersControlSaveData
{
    public int CurrentNumberOfWorkersInThisBuilding;
    public int MaxValueOfWorkersInThisBuilding;
    [FormerlySerializedAs("suitableWorkerDataForThisBuilding")] public UnitType suitableUnitDataForThisBuilding;

    public WorkersControlSaveData(ThisBuildingWorkersControl buildingWorkersControl)
    {
        CurrentNumberOfWorkersInThisBuilding = buildingWorkersControl.CurrentNumberWorkersInThisBuilding;
        MaxValueOfWorkersInThisBuilding = buildingWorkersControl.MaxValueOfWorkersInThisBuilding;
        suitableUnitDataForThisBuilding = buildingWorkersControl.suitableUnitDataForThisBuilding;
    }
}

[System.Serializable]
public class DroneSaveData
{
    public int LogisticsStorage;
    public BuildingSaveData buildingDataLogistics;
    public bool IsLogisticsCycleActive;
    public bool isFlyNow;
    public bool isPlaceNow;
    public bool isTakingOff; 
    public bool isLanding;
    public bool isMovingToLandingSpot;

    public DroneSaveData(DroneMovementController droneMovementController)
    {
        LogisticsStorage = droneMovementController.LogisticsStorage;
        Debug.Log($"LogisticsStorage = {LogisticsStorage}");
        if (droneMovementController.buildingDataLogistics != null)
        {
            BuildingSaveData buildingSaveData = new BuildingSaveData(droneMovementController.buildingDataLogistics);
            buildingDataLogistics = buildingSaveData;
        }
        else
        {
            buildingDataLogistics = null;
        }
        IsLogisticsCycleActive = droneMovementController.IsLogisticsCycleActive;
        isFlyNow = droneMovementController.isFlyNow;
        isPlaceNow = droneMovementController.isPlaceNow;
        isTakingOff = droneMovementController.isTakingOff;
        isMovingToLandingSpot = droneMovementController.isMovingToLandingSpot;
    }
}

[System.Serializable]
public class WorkersDataSaveData
{
    public bool IsWorkerAtWork;
    public int SaveListIndex;
    public UnitType unitType;
    public DroneSaveData droneSaveData;

    public WorkersDataSaveData(WorkerData workerData)
    {
        IsWorkerAtWork = workerData.IsWorkerAtWork;
        SaveListIndex = workerData.SaveListIndex;
        unitType = workerData.unitType;
        droneSaveData = workerData.droneSaveData;
    }
}

[System.Serializable]
public class SerializableData
{
    public List<string> buildingNames;
    public List<TransformData> buildingsTransform;
    public List<BuildingSaveData> BuildingDatas;
    public List<WorkersControlSaveData> WorkersContolSaveDatas;

    public List<string> workerNames;
    public List<TransformData> WorkersTransform;
    public List<WorkersDataSaveData> workersDatas;

    public string playerName;
    public TransformData playerTransform;
}

