using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APIControl.Semaphore;
using Dialogues;
using EntityActions.WorkersScripts;
using RTS_Cam;
using TMPro;
using UI;
using UI.UIManagers;
using Unitilities;
using UnityEngine;
using UnityEngine.Events;

public class InteractionBuildingController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent OpenDescriptionPanel;

    [Header("Interaction System")]
    [Tooltip("Есть ли функицонал у этого здания")] public bool PossiblityPutEInThisBuilding;
    [Tooltip("При подходе игрока текст может просто отображать какую-либо инфорамацию о нем")] public bool OnlyShowText;
    [Tooltip("Название функционала")] public string nameOfFunction;
    [Tooltip("Функции, срабатывающие при нажатии E около здания")] public UnityEvent InteractionEvent;
    [Tooltip("Функции, срабатывающие при нахождении игрока рядом со зданием")] public UnityEvent TextOnEvent;
    [Tooltip("Позволяет ли данное здание подключиться к магазину")] [SerializeField] private bool IsThereBarterHere;
    public GameEvent OpenBarterMenuEvent;
    public GameEvent CloseBarterMenuEvent;
    [Tooltip("Участвует ли данное здание в системе логистики")] [SerializeField] private bool IsThisLogisticsIncluded;
    [Tooltip("Сколько ждать около зданий ( для асинхрона )")] [SerializeField] private int awaitDroneLogistics;
    [NonSerialized] public List<GameObject> objectsInTrigger = new List<GameObject>();
    
    [Header("Flags")]
    [NonSerialized]public bool CanPutE;
    [NonSerialized]public bool IsTextStartWorkingActive;

    [Header("Building Data")]
    public List<Transform> PointsOfBuildings;
    public Transform spawnWorker;
    private BuildingData _buildingData;
    private CompletionOfConstructionController _completionOfConstructionController;

    [Header("Layer masks")]
    [SerializeField] private LayerMask placementLayerMask; // Для клика по зданию
    
    [Header("Hint")]
    public GameObject Texthint;
    public float textOnTime;
    

    private void Start()
    {
        _buildingData = GetComponent<BuildingData>();
        _completionOfConstructionController = GetComponent<CompletionOfConstructionController>();
        CanPutE = false;
        IsTextStartWorkingActive = false;

        PlayerNearBuilding((() =>
        {
            CanPutE = true;
            if (_buildingData.IsThisBuilt)
            {
                TextOnEvent?.Invoke();
                Texthint.SetActive(true);
            }
        }));
    }

    private void Update()
    {
        // Если у здания можно нажать на Е, то при нажатии вызываем ивент, содержащий функционал здания
        if (Input.GetButtonDown("InteractionWithBuilding") && CanPutE)
        {
            InteractionEvent?.Invoke();
        }
        
        // Нажатие на здание 
        Ray ray = GeneralWorkersControl.MainCamera.ScreenPointToRay(Input.mousePosition); 
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000f, placementLayerMask))
        {
            if (hit.collider.CompareTag("ClickOnBuilding") && hit.collider.transform.parent.gameObject == this.gameObject && Input.GetMouseButtonDown(0) && GeneralWorkersControl.SelectedUnit == null)
            {
                OnMouseDownBuilding();
            }
        }
    }

    public async void OnTriggerEnter(Collider other)
    {
        if (!objectsInTrigger.Contains(other.gameObject) && (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("ClickOnWorker")))
        {
            objectsInTrigger.Add(other.gameObject);
        }
        
        // Если игрок около здания, вызываем подсказку о нажатии на Е и позволяем использование функционала
        if (other.gameObject.CompareTag("Player") && (PossiblityPutEInThisBuilding || OnlyShowText))
        {
            if (GetComponent<BuildingData>().IsThisBuilt)
            {
                CanPutE = true;
                TextOnEvent?.Invoke();
                Texthint.SetActive(true);
            }
        }
        
        // Если рабочий около здания
        if(other.gameObject.CompareTag("ClickOnWorker") && other.gameObject.GetComponent<IWorkerUnit>().SelectedBuilding != null)
        {
            IWorkerUnit unitMovementController =
                 other.gameObject.GetComponent<IWorkerUnit>();
            
            
            Debug.Log("Рабочий около здания");
            
            // Если данное здание не построено, прибежавший рабочий занят постройкой, и это здание является для него выделенным
            if (!_buildingData.IsThisBuilt && unitMovementController.ArriveForBuildBuidling && unitMovementController.SelectedBuilding.GetComponent<BuildingData>() == GetComponent<BuildingData>())
            {
                WorkerBuildBuilding(other, unitMovementController);
            }
            // Рабочий прибыл не для строительства
            else if (_buildingData.IsThisBuilt && !unitMovementController.ArriveForBuildBuidling && unitMovementController.SelectedBuilding.GetComponent<BuildingData>() == GetComponent<BuildingData>())
            {
                WorkerCameToBuilding(other, unitMovementController);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (objectsInTrigger.Contains(other.gameObject) && (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("ClickOnWorker")))
        {
            objectsInTrigger.Remove(other.gameObject);
        }
        
        if (other.gameObject.CompareTag("Player") && (PossiblityPutEInThisBuilding || OnlyShowText))
        {
            CanPutE = false;
            Texthint.SetActive(false);
        }
    }

    /// <summary>
    /// Рабочий начинает постройку здания
    /// </summary>
    public void WorkerBuildBuilding(Collider other, IWorkerUnit unitMovementController)
    {
        // у рабочего пропадает цель следования
        IWorkerUnit movementController = other.gameObject.GetComponent<IWorkerUnit>();
        GameObject worker = other.gameObject;
        WorkerData workerData = worker.GetComponent<WorkerData>();
        movementController.UnitPointOfDestination = null;
        movementController.PossibilityClickOnUnit = false;
        other.transform.LookAt(_buildingData.transform);
                    
        Debug.Log("Рабочий добрался, начинает строить здание");
        _completionOfConstructionController.NotifyWorkerArrival(workerData);

        if (workerData.unitType != UnitType.MainDrone)
        {
            worker.SetActive(false);
        }
    }

    /// <summary>
    /// Какой-либо юнит подошел к зданию
    /// </summary>
    public void WorkerCameToBuilding(Collider other, IWorkerUnit unitMovementController)
    {
        // Здание может содержать рабочих
        if (GetComponent<ThisBuildingWorkersControl>())
        {
            WorkerComeToBuilding(other);
        } 
        // Здание добывает ресурсы
        else if (IsThisLogisticsIncluded && unitMovementController is IUnitLogistics)
        {
            DroneArriveToMiner(unitMovementController);
        }
    }
    
    /// <summary>
    /// Юнит, участвующий в системе логистики подошел к зданию
    /// </summary>
    /// <param name="unitMovementController"></param>
    public async void DroneArriveToMiner(IWorkerUnit unitMovementController)
    {
        Debug.Log($"<color=yellow> К зданию {_buildingData.Title} подлетел дрон");
        DialogueManager.OnBuildingPlaced?.Invoke(_buildingData.buildingTypeSO, ActionTypeInteractWithObject.DroneCameToBuilding);
        DroneMovementController droneMovementController = unitMovementController as DroneMovementController;
        
        if (GetComponent<ResourceMiner>())
        {
            ResourceMiner resourceMiner = GetComponent<ResourceMiner>();
            BuildingData buildingDataMB = BaseUpgradeConditionManager.buildingDataMB;
            int resourceIndex = (int)resourceMiner._minerType;
            int limitsResource = droneMovementController.MaximumLogisticsStorage[resourceIndex];
        
            // Если в здании есть хотя бы какие то ресурсы
            if (_buildingData.Storage[resourceIndex] > 0)
            {
                Debug.Log($"<color=yellow> У здания есть ресурсы");
                if (droneMovementController.LogisticsStorage < limitsResource)
                {
                    Debug.Log($"<color=yellow> У дрона есть место для ресурсов");
                    await Task.Delay(awaitDroneLogistics);

                    if (CheckDroneIsHereAfterWaiting(droneMovementController))
                    {
                        Debug.Log($"<color=yellow> Дрон готов принять ресурсы");
                        if (!droneMovementController.IsLogisticsCycleActive)
                        {
                            Debug.Log($"<color=yellow> Дрон не был задействован в логистике, ЗАПУСК");
                            droneMovementController.IsLogisticsCycleActive = true;
                            droneMovementController.buildingDataLogistics = _buildingData;
                            GeneralWorkersControl.Instance.ResetSelectedUnit();
                        }

                        if (droneMovementController.LogisticsStorage <  _buildingData.Storage[resourceIndex])
                        {
                            droneMovementController.LogisticsStorage += limitsResource;
                            _buildingData.Storage[resourceIndex] -= limitsResource;
                        }
                        else
                        {
                            droneMovementController.LogisticsStorage += _buildingData.Storage[resourceIndex];
                            _buildingData.Storage[resourceIndex] = 0;
                        }
                    
                        droneMovementController.SelectedBuilding = buildingDataMB.gameObject;
                        droneMovementController.LogisticsCycleMovementHandler();
                        Debug.Log($"<color=yellow>Ресурсы дрона: {droneMovementController.LogisticsStorage}, здание назначения: {droneMovementController.SelectedBuilding.GetComponent<BuildingData>().Title}, {droneMovementController.IsLogisticsCycleActive}");
                    }
                    
                }
            }
        }
        else if (_buildingData.buildingTypeSO.buildingType == BuildingsTypes.MobileBase)
        {
            Debug.Log($"<color=yellow> Дрон у базы, его характеристики: хранилище - {droneMovementController.LogisticsStorage}");
            if (droneMovementController.LogisticsStorage > 0  &&  droneMovementController.SelectedBuilding == this.gameObject && droneMovementController.buildingDataLogistics != null && droneMovementController.buildingDataLogistics.GetComponent<ResourceMiner>())
            {
                await Task.Delay(awaitDroneLogistics);

                if (CheckDroneIsHereAfterWaiting(droneMovementController))
                {
                    Debug.Log($"<color=yellow> Дрон готов отдать ресурсы");
                    if (!droneMovementController.IsLogisticsCycleActive)
                    {
                        Debug.Log($"<color=yellow> Дрон не был задействован в логистике, ЗАПУСК");
                        droneMovementController.IsLogisticsCycleActive = true;
                        GeneralWorkersControl.Instance.ResetSelectedUnit();
                    }

                    EntityID currentPlayer = CurrentPlayersDataControl.WhichPlayerCreate;
                    PlayerResources playerResources = await APIManager.Instance.GetPlayerResources(currentPlayer);
                    ResourceMiner resourceMiner =
                        droneMovementController.buildingDataLogistics.GetComponent<ResourceMiner>();
                    int resourceIndex = (int)resourceMiner._minerType;
                    
                    if (resourceIndex == 0)
                    {
                        playerResources.Iron += droneMovementController.LogisticsStorage;
                        Debug.Log($"<color=yellow> Выгружаем металл: {playerResources.Iron}");
                    }
                    else
                    {
                        playerResources.CryoCrystal += droneMovementController.LogisticsStorage;
                        Debug.Log($"<color=yellow> Выгружаем кристаллы: {playerResources.CryoCrystal}");
                    }

                    droneMovementController.LogisticsStorage = 0;

                    await APIManager.Instance.PutPlayerResources(currentPlayer, playerResources.Iron, playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                    
                    droneMovementController.UpdateResourcesEvent.TriggerEvent();

                    Debug.Log($"<color=yellow> Летим обратно к добытчику, {droneMovementController.IsLogisticsCycleActive}");
                    droneMovementController.SelectedBuilding =
                        droneMovementController.buildingDataLogistics.gameObject;
                    droneMovementController.LogisticsCycleMovementHandler();
                    
                    DialogueManager.OnObjectInteracted?.Invoke(ActionTypeInteractWithObject.DroneGetResources);
                }

            }
        }
    }

    /// <summary>
    /// Рабочий пришел в здание, которое может содержать рабочих
    /// </summary>
    /// <param name="other"></param>
    public async void WorkerComeToBuilding(Collider other)
    {
         GeneralWorkersControl.Instance.NumberOfFreeUnits -= 1;
        Debug.Log($"<color=green>Свободные рабочие - 1: {GeneralWorkersControl.Instance.NumberOfFreeUnits}</color>");
        ThisBuildingWorkersControl thisBuildingWorkersControl = GetComponent<ThisBuildingWorkersControl>();
        TextMeshPro text = Texthint.GetComponent<TextMeshPro>();
        if (thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding < thisBuildingWorkersControl.MaxValueOfWorkersInThisBuilding)
        {
            thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding += 1;
            if (GetComponent<EnergyProduction>())
            {
                EnergyProduction energyProduction = GetComponent<EnergyProduction>();
                energyProduction.OnAddEnergy();
                string newText = $"{_buildingData.Title} запущен ({thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding}/{thisBuildingWorkersControl.MaxValueOfWorkersInThisBuilding})";
                TemporaryText(text, newText);
                DialogueManager.OnBuildingPlaced?.Invoke(_buildingData.buildingTypeSO, ActionTypeInteractWithObject.WorkerCameToBuilding);
            }
            else
            {
                text.text = $"Нажмите E чтобы выгрузить одного рабочего ({thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding}/{thisBuildingWorkersControl.MaxValueOfWorkersInThisBuilding})";
            }

            other.gameObject.GetComponent<WorkerData>().IsWorkerAtWork = true;
            
            GetComponent<ThisBuildingWorkersControl>().currentWorkerInThisBuilding =
                other.gameObject.GetComponent<WorkerMovementController>();
            GetComponent<ThisBuildingWorkersControl>().CurrentWorkerDataInThisBuilding =
                other.gameObject.GetComponent<WorkerData>();
            other.gameObject.transform.parent.gameObject.SetActive(false);
            
            
            PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
            playerSaveData.BuildingWorkersInformationList[_buildingData.SaveListIndex]
                    .CurrentNumberOfWorkersInThisBuilding =
                GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding;
            
            await JSONSerializeManager.Instance.JSONSave();
        }
    }
    
    /// <summary>
    /// Показ текста, который пропадет через определенное время 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="whichText"></param>
    private void TemporaryText(TextMeshPro text, string whichText)
    {
        text.gameObject.SetActive(true);
        text.text = whichText;
        Utility.Invoke(this, () =>
        {
            foreach (var obj in objectsInTrigger)
            {
                if (obj.gameObject.CompareTag("Player"))
                {
                    TextOnEvent?.Invoke();
                    return;
                }
            }
            text.gameObject.SetActive(false);
        }, textOnTime);
    }

    /// <summary>
    /// Нажатие на здание
    /// </summary>
    public void OnMouseDownBuilding()
    {
        DescriptionPanelController.buildingData = _buildingData;
        DescriptionPanelController.buildingTransform = gameObject.transform;
        DescriptionPanelController.buildingSO = _buildingData.buildingTypeSO;
        OpenDescriptionPanel.TriggerEvent();
        
        if (_buildingData.buildingTypeSO.buildingType == BuildingsTypes.MobileBase)
        {
            DialogueManager.OnObjectInteracted?.Invoke(ActionTypeInteractWithObject.ClickOnMobileBase);
        }
    }
    
    /// <summary>
    /// Октрытие меню бартера
    /// Вызывается из ивента в InteractionBuildingController на скрипте здания
    /// Ивент слушает UIActiveControl
    /// </summary>
    public void OpenBarterMenu()
    {
        OpenBarterMenuEvent.TriggerEvent();
        RTS_Camera.possibilityZoomCamera = false;
        DialogueManager.OnObjectInteracted?.Invoke(ActionTypeInteractWithObject.OpenBarter);
        UIManager.CancelLastOpenPanelEvent += UIManager.Instance.CloseBarterMenu;
    }
    
    /// <summary>
    /// Закрытие меню бартера
    /// </summary>
    public void CloseBarterMenu()
    {
        CloseBarterMenuEvent.TriggerEvent();
    }

    /// <summary>
    /// Проверяет, находится ли после ожидания данный участник системы логистики в триггере
    /// Если нет, то передачи ресурсов не произойдет
    /// </summary>
    /// <param name="unitLogistics"></param>
    /// <returns></returns>
    private bool CheckDroneIsHereAfterWaiting(IUnitLogistics unitLogistics)
    {
        foreach (var obj in objectsInTrigger)
        {
            var unitLogisticsObj = obj.GetComponent<IUnitLogistics>();
            if (unitLogisticsObj != null && unitLogisticsObj == unitLogistics)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Перебор коллайдеров рядом со зданием, если серди них есть игрок, выполняем логику
    /// </summary>
    /// <param name="f"></param>
    public void PlayerNearBuilding(Action f)
    {
        foreach (var obj in objectsInTrigger)
        {
            if (obj.gameObject.CompareTag("Player"))
            {
                f?.Invoke();
                return;
            }
        }
    }
    
    /// <summary>
    /// Перебор коллайдеров рядом со зданием, если серди них есть игрок, выполняем логику
    /// </summary>
    /// <param name="f"></param>
    public void WorkerNearBuilding(Action<Collider, IWorkerUnit> f)
    {
        foreach (var obj in objectsInTrigger)
        {
            if (obj.gameObject.CompareTag("ClickOnWorker"))
            {
                IWorkerUnit unitMovementController =
                    obj.gameObject.GetComponent<IWorkerUnit>();
                if (unitMovementController.SelectedBuilding.GetComponent<BuildingData>() == GetComponent<BuildingData>())
                {
                    f?.Invoke(obj.GetComponent<Collider>(), unitMovementController);
                    return;
                }
            }
        }
    } 

    /// <summary>
    /// Вызывается при размещении и при постройке здания, на случай если игрок в этот момент окажется около него
    /// Добавляется в Unityevent CompletionOfConstructionController.OnEndBuilding
    /// </summary>
    public void PlayerNearBuildingAfterConstructBuilding()
    {
        PlayerNearBuilding((() =>
        {
            CanPutE = true;
            if (_buildingData.IsThisBuilt)
            {
                TextOnEvent?.Invoke();
                Texthint.SetActive(true);
            }
        }));
    }
}
