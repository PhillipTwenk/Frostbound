using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сохранения данных о зданиях 
/// </summary>
[CreateAssetMenu(menuName = "SaveData/PlayerSaveData")]
public class PlayerSaveData : ScriptableObject, ISerializableSO
{
    [SerializeField] private string BuildingPrefabsPath;

    [SerializeField] private GameEvent UpdateResourcesEvent;

    [SerializeField] private GameEvent TabletInitializationEvent;
    [SerializeField] private GameEvent ShieldInitializationEvent;
    
    // Реализация ISerializableSO   
    public string SerializeToJson()
    {
        // Сохраняем только данные для сериализации, а не ссылки на объекты
        var serializableData = new SerializableData
        {
            buildingNames = playerBuildings.ConvertAll(b => b.name),
            buildingsTransform = buildingsTransform,
            BuildingDatas = BuildingDatas,
            WorkersContolSaveDatas = BuildingWorkersInformationList
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
    }
    
    
    
    
    
    public List<GameObject> playerBuildings;
    public List<TransformData> buildingsTransform;
    public List<BuildingSaveData> BuildingDatas;
    public List<WorkersContolSaveData> BuildingWorkersInformationList;

    private bool IsDeleteBuidlingProcessActive;

    /// <summary>
    /// Инициализирует все построеные здания в игре
    /// </summary>
    public async void InitializeBuildings()
    {
        IsDeleteBuidlingProcessActive = false;
        if (playerBuildings is not null)
        {
            int i = 0;
            foreach (var building in playerBuildings)
            {
                Debug.Log($"Здание номеееерррр {i}");
                GameObject newBuilding = Instantiate(building);
                newBuilding.transform.position = buildingsTransform[i].position;
                newBuilding.transform.rotation = buildingsTransform[i].rotation;
                newBuilding.transform.localScale = buildingsTransform[i].scale;

                GameObject ComponentContainingBuilding = newBuilding.transform.GetChild(0).gameObject;

                BuildingData buildingData = ComponentContainingBuilding.GetComponent<BuildingData>();
                buildingData.Level = BuildingDatas[i].Level; 
                buildingData.Durability = BuildingDatas[i].Durability;
                buildingData.Storage = BuildingDatas[i].Storage;
                buildingData.SaveListIndex = BuildingDatas[i].SaveListIndex;
                buildingData.HoneyConsumption = BuildingDatas[i].HoneyConsumption;
                buildingData.Production = BuildingDatas[i].Production;
                buildingData.IsThisBuilt = true;
                if (i == 0)
                {
                    BaseUpgradeConditionManager.buildingDataMB = buildingData;
                    BaseUpgradeConditionManager.CurrentBaseLevel = buildingData.Level;
                    TabletInitializationEvent.TriggerEvent();
                    ShieldInitializationEvent.TriggerEvent();
                }

                if (ComponentContainingBuilding.GetComponent<ThisBuildingWorkersControl>())
                {
                    Debug.Log($"Здание {buildingData.name}, количество рабочих в нем: <color=green>{BuildingWorkersInformationList[i].CurrentNumberOfWorkersInThisBuilding}</color>");
                    ThisBuildingWorkersControl workers = ComponentContainingBuilding.GetComponent<ThisBuildingWorkersControl>();
                    workers.CurrentNumberWorkersInThisBuilding = BuildingWorkersInformationList[i].CurrentNumberOfWorkersInThisBuilding;
                    workers.MaxValueOfWorkersInThisBuilding = BuildingWorkersInformationList[i].MaxValueOfWorkersInThisBuilding;

                    WorkersInterBuildingControl.Instance.AddNewBuilding(workers);
                }
                else
                {
                    WorkersInterBuildingControl.Instance.AddNewBuilding(null);
                }
                i++;
            }
        }
        
        await BuildingManager.Instance._navMeshSurface.UpdateNavMesh(BuildingManager.Instance._navMeshSurface.navMeshData);
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
                WorkersInterBuildingControl.Instance.RemoveNewBuilding(buildingData.gameObject.GetComponent<ThisBuildingWorkersControl>());
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
public class WorkersContolSaveData
{
    public int CurrentNumberOfWorkersInThisBuilding;
    public int MaxValueOfWorkersInThisBuilding;

    public WorkersContolSaveData(ThisBuildingWorkersControl buildingWorkersControl)
    {
        CurrentNumberOfWorkersInThisBuilding = buildingWorkersControl.CurrentNumberWorkersInThisBuilding;
        MaxValueOfWorkersInThisBuilding = buildingWorkersControl.MaxValueOfWorkersInThisBuilding;
    }
}

[System.Serializable]
public class SerializableData
{
    public List<string> buildingNames;
    public List<TransformData> buildingsTransform;
    public List<BuildingSaveData> BuildingDatas;
    public List<WorkersContolSaveData> WorkersContolSaveDatas;
}
