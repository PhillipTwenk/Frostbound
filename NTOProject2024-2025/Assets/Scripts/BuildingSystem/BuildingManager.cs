using System;
using System.Collections.Generic;
using Dialogues;
using EntityActions.WorkersScripts;
using RTS_Cam;
using UnityEngine;
using TMPro;
using Unitilities;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class BuildingManager : MonoBehaviour
{
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

    public NavMeshSurface _navMeshSurfaceUnit;
    public NavMeshSurface _navMeshSurfaceDrone;

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
        
        PlayerResources playerResources =
            await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);

        int HoneyConsumptionBuilding =
            MouseIndicator.GetComponent<PreviouslyBuidlingTriggerDataStorage>().HoneyConsumption;
        int FoodConsumptionBuilding =
            MouseIndicator.GetComponent<PreviouslyBuidlingTriggerDataStorage>().NumberOfWorkers;
        
        if (playerResources.Iron >= priceBuilding)
        {
            if(BaseUpgradeConditionManager.CurrentBaseLevel >= buildingPrefabSO.MBLevelForBuidlingthisIron)
            {
                int CNoW = GeneralWorkersControl.Instance.CurrentValueOfUnits;
                int MVoW = GeneralWorkersControl.Instance.MaxValueOfUnits;
                int AW = GeneralWorkersControl.Instance.NumberOfFreeUnits;
                if ((playerResources.Energy - HoneyConsumptionBuilding) >= 0)
                {
                    if ((playerResources.Food - FoodConsumptionBuilding * GeneralWorkersControl.CurrentFoodConsumptionByWorkers) >= 0)
                    {
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
                        CompletionOfConstructionController componentContainingBuilding = ComponentContainingBuilding.GetComponent<CompletionOfConstructionController>();

                        buildingData.IsThisBuilt = false;

                        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
                        
                        await _navMeshSurfaceUnit.UpdateNavMesh(_navMeshSurfaceUnit.navMeshData);
                        await _navMeshSurfaceDrone.UpdateNavMesh(_navMeshSurfaceDrone.navMeshData);
                        Debug.Log("NavMesh updated");

                        DialogueManager.OnBuildingPlaced?.Invoke(buildingData.buildingTypeSO, ActionTypeInteractWithObject.PlacementBuilding);
                        componentContainingBuilding.StartCompletionOfConstruction(playerResources);
                        
                        
                        //TutorialPLacementBuildingsCheck(buildingDataLogistics);
                    }
                    else
                    {
                        UpdateTextWhileBuild(HintNoFoodText);
                    }
                }
                    else
                    {
                        UpdateTextWhileBuild(HintNoEnergyText);
                    }
            }
            else
            {
                UpdateTextWhileBuild(HintNotEnoughtLevelBaseText);
            }
        }
        else
        {
            UpdateTextWhileBuild(HintNotEnoughtResourcesText);
        }
        
        UpdateResourcesEvent.TriggerEvent();
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }


    /// <summary>
    /// Показывает сообщение, выводящее информацию о том почему мы не можем построить здание
    /// </summary>
    private void UpdateTextWhileBuild(string text)
    {
        TextNotEnoughResource.SetActive(true);
        Utility.Invoke(this, () => TextNotEnoughResource.SetActive(false), TimeHint);
        TextHintTMPRoUGUI.text = text;
    }
    // private void TutorialPLacementBuildingsCheck(BuildingData buildingDataLogistics)
    // {
    //     string BuildingName = buildingDataLogistics.Title;
    //     switch (BuildingName)
    //     {
    //         case "Пасека":
    //             ApiaryPlacementTutorial.CheckAndUpdateTutorialState();
    //             break;
    //         case "Жилой модуль":
    //             HomePlacementTutorial.CheckAndUpdateTutorialState();
    //             break;
    //         case "Добытчик":
    //             MinerPlacementTutorial.CheckAndUpdateTutorialState();
    //             break;
    //     }
    // }
    
    // private void TutorialWaitWorkersCheck(BuildingData buildingDataLogistics)
    // {
    //     string BuildingName = buildingDataLogistics.Title;
    //     switch (BuildingName)
    //     {
    //         case "Пасека":
    //             WaitWorkerApiaryTutorial.CheckAndUpdateTutorialState();
    //             break;
    //         case "Жилой модуль":
    //             WaitWorkerHomeTutorial.CheckAndUpdateTutorialState();
    //             break;
    //         case "Добытчик":
    //             WaitWorkerMinerTutorial.CheckAndUpdateTutorialState();
    //             break;
    //     }
    // }
}

