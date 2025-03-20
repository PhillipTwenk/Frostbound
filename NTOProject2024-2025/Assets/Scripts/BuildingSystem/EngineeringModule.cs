using System.Threading.Tasks;
using EntityActions.WorkersScripts;
using TMPro;
using Unitilities;
using UnityEngine;
using UnityEngine.AI;

public class EngineeringModule : MonoBehaviour
{
    [Header("Production parameters")] 
    [Tooltip("Время постройки дрона ( [мс] / для async")] public float productionRate;
    [Tooltip("Цена дрона / [металл]")] public int droneBuildingPrice;
    [Tooltip("Время, на которое загораются некоторые виды текста / [сек]")] public float textOnTime;
    [Tooltip("Префаб дрона")] public GameObject dronePrefab;
    
    [Header("Components")]
    private BuildingData buildingData;
    private Building building;
    private InteractionBuildingController interactionBuildingController;
    
    [Header("Texts")]
    [Tooltip("Подсказка при создании дрона при игроке около ИМ")] [TextArea] public string mainDroneProductionText;
    [Tooltip("Подсказка что слотов нет")] [TextArea] public string maximumSlotsDroneProductionText;
    [Tooltip("Подсказка как новые слоты получить")] [TextArea] public string maximumSlotsDroneProductionText2;
    [Tooltip("Подсказка что нужно убрать дрон")] [TextArea] public string droneLeaveHint;
    [Tooltip("Подсказка что недостаточно металла для покупки дрона")] [TextArea] public string notEnoughIron;
    [Tooltip("Подсказка что создание дрона превысит лимит по юнитам")] [TextArea] public string unitLimitHint;

    [Header("Flags")] 
    public bool isSpawnPointFree;
    
    [Header("Objects")]
    public Transform spawnPoint;

    [Header("Events")] public GameEvent UpdateResourcesEvent;

    private void Start()
    {
        buildingData = GetComponent<BuildingData>();
        building = buildingData.buildingTypeSO;
        interactionBuildingController = GetComponent<InteractionBuildingController>();
    }

    /// <summary>
    /// Контроль текста при подходе игрока к зданию
    /// </summary>
    /// <param name="text"></param>
    public void TextOn(TextMeshPro text)
    {
        interactionBuildingController.PlayerNearBuilding((() =>
        {
            ResourceData resourceData = building.StorageLimit(BaseUpgradeConditionManager.CurrentBaseLevel);
            int MaximumSlots = resourceData.resources[0];
            int currentSlot = buildingData.Storage[0];

            if (currentSlot < MaximumSlots)
            {
                text.text = $"{mainDroneProductionText}" +
                            "\n" +
                            $"Слоты: {currentSlot}/{MaximumSlots}" +
                            "\n" +
                            $"Цена постройки дрона: {droneBuildingPrice} <sprite=2>";
            }
            else if (currentSlot >= MaximumSlots)
            {
                text.text = $"{maximumSlotsDroneProductionText}" +
                            "\n" +
                            $"{maximumSlotsDroneProductionText2}";
            }
        }));
    }


    /// <summary>
    /// Функция здания. Создание дрона
    /// </summary>
    public async void CreateDrone(TextMeshPro text)
    {
        ResourceData resourceData = building.StorageLimit(BaseUpgradeConditionManager.CurrentBaseLevel);
        int MaximumSlots = resourceData.resources[0];
        int currentSlot = buildingData.Storage[0];
        PlayerResources playerResources = await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
        if ((GeneralWorkersControl.Instance.CurrentValueOfUnits + 1) <= GeneralWorkersControl.Instance.MaxValueOfUnits)
        {
            if (currentSlot < MaximumSlots)
            {
                if (isSpawnPointFree)
                {
                    if (playerResources.Iron >= droneBuildingPrice)
                    {
                        SpawnNewDrone();
                    
                        GeneralWorkersControl.Instance.CurrentValueOfUnits += 1;
                        GeneralWorkersControl.Instance.NumberOfFreeUnits += 1;

                        buildingData.Storage[0] += 1;
                    
                        await SyncManager.Enqueue(async () =>
                        {
                            await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron - droneBuildingPrice,
                                playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                            UpdateResourcesEvent.TriggerEvent();
                        });
                    
                        await JSONSerializeManager.Instance.JSONSave();
                    
                        TextOn(buildingData.AwaitBuildingThisTMPro);
                    }
                    else
                    {
                        string ironHint = $"{notEnoughIron}" +
                                          "\n" +
                                          $"{playerResources.Iron}/{droneBuildingPrice} <sprite=2>";
                        TemporaryText(text, ironHint);
                    }
                
                }
                else
                {
                    TemporaryText(text, droneLeaveHint);
                }
            }
            else
            {
                string newText = $"{maximumSlotsDroneProductionText}" +
                                 "\n" +
                                 $"{maximumSlotsDroneProductionText2}";
                TemporaryText(text, newText);
            }
        }
        else
        {
            TemporaryText(text, unitLimitHint);
        }
        
    }


    private void TemporaryText(TextMeshPro text, string whichText)
    {
        text.gameObject.SetActive(true);
        text.text = whichText;
        Utility.Invoke(this, () =>
        {
            foreach (var obj in interactionBuildingController.objectsInTrigger)
            {
                if (obj.gameObject.CompareTag("Player"))
                {
                    interactionBuildingController.TextOnEvent?.Invoke();
                    return;
                }
            }
            text.gameObject.SetActive(false);
        }, textOnTime);
    }

    /// <summary>
    /// Спавн нового дрона около здания
    /// </summary>
    private void SpawnNewDrone()
    {
        GameObject drone = Instantiate(dronePrefab);
        // Иницилиазация игровых данных
        GameObject newWorkerСomponentsContainingObject = drone.transform.GetChild(0).gameObject;
    
        //Инициализация расположения
        NavMeshAgent agent = newWorkerСomponentsContainingObject.GetComponent<NavMeshAgent>();
        agent.enabled = false;
            
        drone.transform.position = Vector3.zero;
        drone.transform.rotation = Quaternion.Euler(0,0,0);
        drone.transform.localScale = Vector3.one;

        newWorkerСomponentsContainingObject.transform.position = spawnPoint.position;
        
        
        DroneMovementController droneMovementController = newWorkerСomponentsContainingObject.GetComponent<DroneMovementController>();
        droneMovementController.isFlyNow = false;
        droneMovementController.isLanding = false;
        droneMovementController.isPlaceNow = true;
        droneMovementController.isTakingOff = false;
        droneMovementController.isMovingToLandingSpot = false;
        droneMovementController.IsLogisticsCycleActive = false;
        droneMovementController.LogisticsStorage = 0;
        droneMovementController.buildingDataLogistics = null;
        
        Animator animator = newWorkerСomponentsContainingObject.GetComponent<Animator>();
        animator.SetBool(droneMovementController.droneFly_AK, false);
        
        WorkerData workerData = newWorkerСomponentsContainingObject.GetComponent<WorkerData>();
        workerData.IsWorkerAtWork = false;
        workerData.unitType = UnitType.MainDrone;
        DroneSaveData droneSaveData = new DroneSaveData(droneMovementController);
        workerData.droneSaveData = droneSaveData;
        workerData.gameObject.GetComponent<IWorkerUnit>().MainCamera =
            GeneralWorkersControl.MainCamera;

        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        playerSaveData.workers.Add(dronePrefab);
        TransformData transformData = new TransformData(newWorkerСomponentsContainingObject.transform);
        playerSaveData.workersTransform.Add(transformData);
        WorkersDataSaveData workersDataSaveData = new WorkersDataSaveData(workerData);
        playerSaveData.workerDatas.Add(workersDataSaveData);

        workerData.SaveListIndex = playerSaveData.workerDatas.IndexOf(workersDataSaveData);
        playerSaveData.workerDatas[workerData.SaveListIndex].SaveListIndex = workerData.SaveListIndex;
    }
}
