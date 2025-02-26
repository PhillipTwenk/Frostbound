using System.Collections.Generic;
using RTS_Cam;
using UnityEngine;
using TMPro;
using Unity.AI.Navigation;

public class BuildingManager : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective ApiaryPlacementTutorial;
    [SerializeField] private TutorialObjective WaitWorkerApiaryTutorial;
    [SerializeField] private TutorialObjective HomePlacementTutorial;
    [SerializeField] private TutorialObjective WaitWorkerHomeTutorial;
    [SerializeField] private TutorialObjective MinerPlacementTutorial;
    [SerializeField] private TutorialObjective WaitWorkerMinerTutorial;
    
    public static BuildingManager Instance { get; private set; }

    [TextArea] [SerializeField] private string HintNotEnoughtResourcesText;
    [TextArea] [SerializeField] private string HintNotEnoughtLevelBaseText;
    [TextArea] [SerializeField] private string HintNotFreeWorkersText;
    [TextArea] [SerializeField] private string HintNoEnergyText;
    [TextArea] [SerializeField] private string HintNoFoodText;
    [SerializeField] private float TimeHint;
    [SerializeField] private GameObject TextNotEnoughResource;
    [SerializeField] private TextMeshProUGUI TextHintTMPRoUGUI;

    public bool IsBuildingActive;
    public bool CanBuilding;
    public bool ProcessWorkerBuildingActive;
    
    [SerializeField] private Camera MainCamera;
    private Vector3 lastPosition;
    
    public GameObject MouseIndicator;
    [SerializeField] private LayerMask placementLayerMask;

    [SerializeField] private float YplaceVector;

    public GameObject CurrentBuilding;

    [SerializeField] private float awaitValueBuild;

    [SerializeField] private GameEvent UpdateResourcesEvent;

    public NavMeshSurface _navMeshSurface;

    public void StartPlacingBuild()
    {
        IsBuildingActive = true;
    }

    public void EndPlacingBuild()
    {
        IsBuildingActive = false;
    }

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        ProcessWorkerBuildingActive = false;
        IsBuildingActive = false;
        CanBuilding = true;
    }

    
    private void Update()
    {
        if (IsBuildingActive)
        {
            Vector3 mousePosition = GetSelectedMapPosition();
            MouseIndicator.transform.position = new Vector3(mousePosition.x, mousePosition.y + 0.2f, mousePosition.z);
            
            if (Input.GetMouseButtonDown(0) && CanBuilding)
            {
                PlaceBuilding(mousePosition);
            }
        }
    }

    /// <summary>
    /// Возвращает позицию мыши
    /// </summary>
    /// <returns></returns>
    public Vector3 GetSelectedMapPosition()
    {
        lastPosition = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        Ray ray = MainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, placementLayerMask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
    
    /// <summary>
    /// Размещение строения
    /// </summary>
    /// <param name="mousePosition"></param>
    public async void PlaceBuilding(Vector3 mousePosition)
    {
        ProcessWorkerBuildingActive = true;
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);

        
        Building buildingPrefabSO = CurrentBuilding.gameObject.transform.GetChild(0).GetComponent<BuildingData>().buildingTypeSO;
        int priceBuilding = buildingPrefabSO.priceBuilding;

        string playerName = UIManagerLocation.WhichPlayerCreate.Name;
        PlayerResources playerResources =
            await APIManager.Instance.GetPlayerResources(UIManagerLocation.WhichPlayerCreate);

        int HoneyConsumptionBuilding =
            MouseIndicator.GetComponent<PreviouslyBuidlingTriggerDataStorage>().HoneyConsumption;
        int FoodConsumptionBuilding =
            MouseIndicator.GetComponent<PreviouslyBuidlingTriggerDataStorage>().NumberOfWorkers;
        
        if (playerResources.Iron >= priceBuilding)
        {
            if(BaseUpgradeConditionManager.CurrentBaseLevel >= buildingPrefabSO.MBLevelForBuidlingthisIron)
            {
                int CNoW = WorkersInterBuildingControl.Instance.CurrentValueOfWorkers;
                int MVoW = WorkersInterBuildingControl.Instance.MaxValueOfWorkers;
                int AW = WorkersInterBuildingControl.Instance.NumberOfFreeWorkers;
                if(CNoW <= MVoW && AW > 0)
                {
                    if ((playerResources.Energy - HoneyConsumptionBuilding) >= 0)
                    {
                        if ((playerResources.Food - FoodConsumptionBuilding * 20) >= 0)
                        {
                            int OldEnergyValue = playerResources.Energy;
                            int OldFoodValue = playerResources.Food;
                            playerResources.Energy -= HoneyConsumptionBuilding;
                            playerResources.Food -= FoodConsumptionBuilding * 20; 
                            
                            Dictionary<string,string> buildingDictionary = new Dictionary<string, string>();
                            buildingDictionary.Add("EnergyValueUpdate", $"{playerResources.Energy - OldEnergyValue}");
                            buildingDictionary.Add("FoodValueUpdate", $"{playerResources.Food - OldFoodValue}");
                            buildingDictionary.Add("IronValueUpdate", $"{(playerResources.Iron - priceBuilding) - playerResources.Iron}");
                            APIManager.Instance.CreatePlayerLog("Начата стройка нового здания, потрачен металл, энергия, и еда, если здание обладает рабочими", playerName, buildingDictionary);

                            await SyncManager.Enqueue(async () =>
                            {
                                await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerResources.Iron - priceBuilding,
                                    playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                            });
                            UpdateResourcesEvent.TriggerEvent();
                            
                            //Создаем новое здание, устанавливаем его позицию и удаляем триггер для строительства
                            MouseIndicator.transform.position = new Vector3(mousePosition.x, YplaceVector, mousePosition.z);
                            GameObject newBuildingObject = Instantiate(CurrentBuilding);
                            newBuildingObject.transform.position = MouseIndicator.transform.position;
                            Destroy(MouseIndicator);
                            
                            IsBuildingActive = false;
                            CurrentBuilding = null;
                            CanBuilding = true;

                            //Получение некорых данных о здании
                            GameObject ComponentContainingBuilding = newBuildingObject.transform.GetChild(0).gameObject;
                            BuildingData buildingData = ComponentContainingBuilding.GetComponent<BuildingData>();

                            buildingData.IsThisBuilt = false;

                            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
                            
                            await _navMeshSurface.UpdateNavMesh(_navMeshSurface.navMeshData);
                            Debug.Log("NavMesh updated");

                            TutorialPLacementBuildingsCheck(buildingData);
                            
                            //Ожидаем прибытия рабочего 
                            await WorkersInterBuildingControl.Instance.SendWorkerToBuilding(true, buildingData);
                            
                            //Ожидаем завершения его строительства
                            await WorkersInterBuildingControl.Instance.WorkerEndWork(buildingData);

                            TutorialWaitWorkersCheck(buildingData);
                            
                            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
                            
                            //Сохранение данных здания в SO сохранения
                            PlayerSaveData pLayerSaveData = UIManagerLocation.Instance.WhichPlayerDataUse();
                            pLayerSaveData.playerBuildings.Add(buildingData.buildingTypeSO.PrefabBuilding);
                
                            TransformData transformData = new TransformData(newBuildingObject.transform);
                            pLayerSaveData.buildingsTransform.Add(transformData);
                            
                            Debug.Log("=========================================================================");
                            Debug.Log(transformData);
                            
                            BuildingSaveData buildingSaveData = new BuildingSaveData(buildingData);
                            pLayerSaveData.BuildingDatas.Add(buildingSaveData);
                            buildingData.SaveListIndex = pLayerSaveData.BuildingDatas.IndexOf(buildingSaveData);
                            pLayerSaveData.BuildingDatas[buildingData.SaveListIndex].SaveListIndex = buildingData.SaveListIndex;
                            Debug.Log(buildingSaveData);
                            Debug.Log("=========================================================================");

                            if (ComponentContainingBuilding.GetComponent<ThisBuildingWorkersControl>())
                            {
                                ThisBuildingWorkersControl thisBuildingWorkersControl = ComponentContainingBuilding.GetComponent<ThisBuildingWorkersControl>();
                                WorkersContolSaveData worlersSaveData = new WorkersContolSaveData(thisBuildingWorkersControl);
                                pLayerSaveData.BuildingWorkersInformationList.Add(worlersSaveData);

                                WorkersInterBuildingControl.Instance.AddNewBuilding(thisBuildingWorkersControl);
                            }
                            else
                            {
                                ThisBuildingWorkersControl thisBuildingWorkersControl = null;
                                
                                pLayerSaveData.BuildingWorkersInformationList.Add(null);
                                WorkersInterBuildingControl.Instance.AddNewBuilding(thisBuildingWorkersControl); 
                            }
                            
                            buildingData.IsThisBuilt = true;
                            
                            buildingData.BuildingVE.Stop();
                            
                            JSONSerializeManager.Instance.JSONSave();
                        }
                        else
                        {
                            TextNotEnoughResource.SetActive(true);
                            Utility.Invoke(this, () => TextNotEnoughResource.SetActive(false), TimeHint);
                            TextHintTMPRoUGUI.text = HintNoFoodText;
                        }
                    }
                    else
                    {
                        TextNotEnoughResource.SetActive(true);
                        Utility.Invoke(this, () => TextNotEnoughResource.SetActive(false), TimeHint);
                        TextHintTMPRoUGUI.text = HintNoEnergyText;
                    }
                }
                else
                {
                    Debug.Log($"Количество свободных рабочих: <color=blue>{AW}</color>");
                    Debug.Log($"Всего рабочих: <color=blue>{CNoW}</color>");
                    Debug.Log($"Максимальное количество рабочих: <color=blue>{MVoW}</color>");
                    TextNotEnoughResource.SetActive(true);
                    Utility.Invoke(this, () => TextNotEnoughResource.SetActive(false), TimeHint);
                    TextHintTMPRoUGUI.text = HintNotFreeWorkersText;
                }
            }
            else
            {
                TextNotEnoughResource.SetActive(true);
                Utility.Invoke(this, () => TextNotEnoughResource.SetActive(false), TimeHint);
                TextHintTMPRoUGUI.text = HintNotEnoughtLevelBaseText;
            }
        }
        else
        {
            TextNotEnoughResource.SetActive(true);
            Utility.Invoke(this, () => TextNotEnoughResource.SetActive(false), TimeHint);
            TextHintTMPRoUGUI.text = HintNotEnoughtResourcesText;
        }
        
        UpdateResourcesEvent.TriggerEvent();
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }

    private void TutorialPLacementBuildingsCheck(BuildingData buildingData)
    {
        string BuildingName = buildingData.Title;
        switch (BuildingName)
        {
            case "Пасека":
                ApiaryPlacementTutorial.CheckAndUpdateTutorialState();
                break;
            case "Жилой модуль":
                HomePlacementTutorial.CheckAndUpdateTutorialState();
                break;
            case "Добытчик":
                MinerPlacementTutorial.CheckAndUpdateTutorialState();
                break;
        }
    }
    
    private void TutorialWaitWorkersCheck(BuildingData buildingData)
    {
        string BuildingName = buildingData.Title;
        switch (BuildingName)
        {
            case "Пасека":
                WaitWorkerApiaryTutorial.CheckAndUpdateTutorialState();
                break;
            case "Жилой модуль":
                WaitWorkerHomeTutorial.CheckAndUpdateTutorialState();
                break;
            case "Добытчик":
                WaitWorkerMinerTutorial.CheckAndUpdateTutorialState();
                break;
        }
    }
}

