using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityActions.Movement_Control;
using EntityActions.WorkersScripts;
using RTS_Cam;
using TMPro;
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
    [Tooltip("Функции, срабатывающие при нахождении игрока рядом со зданием")] [SerializeField] private UnityEvent TextOnEvent;
    [Tooltip("Позволяет ли данное здание подключиться к магазину")] [SerializeField] private bool IsThereBarterHere;
    public GameEvent OpenBarterMenuEvent;
    public GameEvent CloseBarterMenuEvent;
    [Tooltip("Участвует ли данное здание в системе логистики")] [SerializeField] private bool IsThisLogisticsIncluded;
    [Tooltip("Сколько ждать около зданий ( для асинхрона )")] [SerializeField] private int awaitDroneLogistics;
    
    [Header("Flags")]
    [NonSerialized]public bool CanPutE;
    [NonSerialized]public bool IsTextStartWorkingActive;

    [Header("Building Data")]
    public List<Transform> PointsOfBuildings;
    public Transform spawnWorker;
    private BuildingData _buildingData;

    [Header("Layer masks")]
    [SerializeField] private LayerMask placementLayerMask; // Для клика по зданию
    
    [Header("Hint")]
    public GameObject Texthint;

    private void Start()
    {
        _buildingData = GetComponent<BuildingData>();
        CanPutE = false;
        IsTextStartWorkingActive = false;
        // if (_buildingData.buildingTypeSO.IDoB == 3)
        // {
        //     Texthint.SetActive(false);
        // }
    }

    private void Update()
    {
        // Если у здания можно нажать на Е, то при нажатии вызываем ивент, содержащий функционал здания
        if (Input.GetButtonDown("InteractionWithBuilding") && CanPutE)
        {
            InteractionEvent?.Invoke();
        }
        
        // Нажатие на здание 
        Ray ray = WorkersInterBuildingControl.MainCamera.ScreenPointToRay(Input.mousePosition); 
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000f, placementLayerMask))
        {
            if (hit.collider.CompareTag("ClickOnBuilding") && hit.collider.transform.parent.gameObject == this.gameObject && Input.GetMouseButtonDown(0) && WorkersInterBuildingControl.SelectedUnit == null)
            {
                OnMouseDownBuilding();
            }
        }
    }

    private async void OnTriggerEnter(Collider other)
    {
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
            if (!_buildingData.IsThisBuilt && unitMovementController.ArriveForBuildBuidling && unitMovementController.SelectedBuilding.GetComponent<BuildingData>().buildingTypeSO.IDoB == GetComponent<BuildingData>().buildingTypeSO.IDoB)
            {
                // у рабочего пропадает цель следования
                IWorkerUnit movementController = other.gameObject.GetComponent<IWorkerUnit>();
                movementController.UnitPointOfDestination = null;
                    
                other.transform.LookAt(WorkersInterBuildingControl.CurrentBuilding.transform);
                    
                Debug.Log(WorkersInterBuildingControl.CurrentBuilding.Title);
                    
                Debug.Log("Рабочий добрался, начинает строить здание");
                WorkersInterBuildingControl.Instance.NotifyWorkerArrival();

                GameObject worker = other.gameObject;
                WorkersInterBuildingControl.Instance.StartAnimationBuilding(worker.GetComponent<IWorkerUnit>(), GetComponent<BuildingData>(), spawnWorker, worker.GetComponent<WorkerData>());
                
                worker.SetActive(false);
                return;
            }
            // Рабочий прибыл не для строительства
            else if (_buildingData.IsThisBuilt && !unitMovementController.ArriveForBuildBuidling && unitMovementController.SelectedBuilding.GetComponent<BuildingData>().buildingTypeSO.IDoB == GetComponent<BuildingData>().buildingTypeSO.IDoB)
            {
                // Здание может содержать рабочих
                if (GetComponent<ThisBuildingWorkersControl>())
                {
                    WorkersInterBuildingControl.Instance.NumberOfFreeWorkers -= 1;
                    Debug.Log($"<color=green>Свободные рабочие - 1: {WorkersInterBuildingControl.Instance.NumberOfFreeWorkers}</color>");
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
                            if (!text.gameObject.activeSelf)
                            {
                                text.gameObject.SetActive(true);
                                IsTextStartWorkingActive = true;
                                Utility.Invoke(this, () =>
                                {
                                    if (text.text == newText)
                                    {
                                        IsTextStartWorkingActive = false;
                                        text.gameObject.SetActive(false);
                                    }
                                }, 4f);
                            }

                            text.text = newText;
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
                        
                        JSONSerializeManager.Instance.JSONSave();
                        return;
                    }
                } 
                // Здание добывает ресурсы
                else if (IsThisLogisticsIncluded && unitMovementController is IUnitLogistics)
                {
                    Debug.Log($"<color=yellow> К зданию {_buildingData.Title} подлетел дрон");
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
                            
                                if (!droneMovementController.IsLogisticsCycleActive && droneMovementController.buildingDataLogistics == null)
                                {
                                    Debug.Log($"<color=yellow> Дрон не был задействован в логистике, ЗАПУСК");
                                    droneMovementController.IsLogisticsCycleActive = true;
                                    droneMovementController.buildingDataLogistics = _buildingData;
                                    droneMovementController.isSelected = false;
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
                                Debug.Log($"<color=yellow>Ресурсы дрона: {droneMovementController.LogisticsStorage}, здание назначения: {droneMovementController.SelectedBuilding.GetComponent<BuildingData>().Title}, {droneMovementController.IsLogisticsCycleActive}");
                            }
                        }
                    }
                    else if (_buildingData.buildingTypeSO.buildingType == BuildingsTypes.MobileBase)
                    {
                        Debug.Log($"<color=yellow> Дрон у базы, его характеристики: хранилище - {droneMovementController.LogisticsStorage}");
                        if (droneMovementController.LogisticsStorage > 0  &&  droneMovementController.SelectedBuilding == this.gameObject && droneMovementController.buildingDataLogistics != null && droneMovementController.buildingDataLogistics.GetComponent<ResourceMiner>())
                        {
                            await Task.Delay(awaitDroneLogistics);
                            
                            if (!droneMovementController.IsLogisticsCycleActive)
                            {
                                Debug.Log($"<color=yellow> Дрон не был задействован в логистике, ЗАПУСК");
                                droneMovementController.IsLogisticsCycleActive = true;
                                droneMovementController.isSelected = false;
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

                            await SyncManager.Enqueue(async () =>
                            {
                                await APIManager.Instance.PutPlayerResources(currentPlayer, playerResources.Iron, playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                            });
                            
                            droneMovementController.UpdateResourcesEvent.TriggerEvent();

                            Debug.Log($"<color=yellow> Летим обратно к добытчику, {droneMovementController.IsLogisticsCycleActive}");
                            droneMovementController.SelectedBuilding =
                                droneMovementController.buildingDataLogistics.gameObject;

                        }
                    }
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && (PossiblityPutEInThisBuilding || OnlyShowText))
        {
            CanPutE = false;
            Texthint.SetActive(false);
        }
    }

    /// <summary>
    /// Нажатие на здание
    /// </summary>
    public void OnMouseDownBuilding()
    {
        AddTextToDescriptionPanel.buildingData = _buildingData;
        AddTextToDescriptionPanel.buildingTransform = gameObject.transform;
        AddTextToDescriptionPanel.buildingSO = _buildingData.buildingTypeSO;
        
        OpenDescriptionPanel.TriggerEvent();
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
        UIManager.CancelLastOpenPanelEvent += UIManager.Instance.CloseBarterMenu;
    }
    
    /// <summary>
    /// Закрытие меню бартера
    /// </summary>
    public void CloseBarterMenu()
    {
        CloseBarterMenuEvent.TriggerEvent();
    }
}
