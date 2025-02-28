using System;
using System.Collections.Generic;
using RTS_Cam;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class InteractionBuildingController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent OpenDescriptionPanel;

    [Header("InteractionSystem")]
    [SerializeField] private bool PossiblityPutEInThisBuilding;
    [ShowIfBool("PossiblityPutEInThisBuilding")][SerializeField] private UnityEvent InteractionEvent;
    [ShowIfBool("PossiblityPutEInThisBuilding")][SerializeField] private UnityEvent TextOnEvent;
    [ShowIfBool("PossiblityPutEInThisBuilding")][SerializeField] private bool IsThereBarterHere;
    [ShowIfBool("IsThereBarterHere")]public GameEvent OpenBarterMenuEvent;
    [ShowIfBool("IsThereBarterHere")]public GameEvent CloseBarterMenuEvent;
    
    [Header("Flags")]
    private bool CanPutE;

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
        if (_buildingData.Title == "Мобильная база")
        {
            Texthint.SetActive(false);
        }
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
            if (hit.collider.CompareTag("ClickOnBuilding") && hit.collider.transform.parent.gameObject == this.gameObject && Input.GetMouseButtonDown(0) && WorkersInterBuildingControl.SelectedWorker == null)
            {
                OnMouseDownBuilding();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если игрок около здания, вызываем подсказку о нажатии на Е и позволяем использование функционала
        if (other.gameObject.CompareTag("Player") && PossiblityPutEInThisBuilding)
        {
            if (GetComponent<BuildingData>().IsThisBuilt)
            {
                CanPutE = true;
                TextOnEvent?.Invoke();
                Texthint.SetActive(true);
            }
        }
        
        // Если рабочий около здания
        if(other.gameObject.CompareTag("ClickOnWorker") && other.gameObject.GetComponent<WorkerMovementController>().SelectedBuilding != null)
        {
            WorkerMovementController workerMovementController =
                other.gameObject.GetComponent<WorkerMovementController>();
            
            
            Debug.Log("Рабочий около здания");
            
            // Если данное здание не построено, прибежавший рабочий занят постройкой, и это здание является для него выделенным
            if (!_buildingData.IsThisBuilt && workerMovementController.ArriveForBuildBuidling && workerMovementController.SelectedBuilding.GetComponent<BuildingData>().buildingTypeSO.IDoB == GetComponent<BuildingData>().buildingTypeSO.IDoB)
            {
                // у рабочего пропадает цель следования
                WorkerMovementController movementController = other.gameObject.GetComponent<WorkerMovementController>();
                movementController.WorkerPointOfDestination = null;
                    
                other.transform.LookAt(WorkersInterBuildingControl.CurrentBuilding.transform);
                    
                // Установка анимаций
                //Animator animator = other.gameObject.GetComponent<Animator>();
                //animator.SetBool("Running", false);
                //animator.SetBool("Building", true);
                //animator.SetBool("Idle", false);
                    
                Debug.Log(WorkersInterBuildingControl.CurrentBuilding.Title);
                    
                Debug.Log("Рабочий добрался, начинает строить здание");
                WorkersInterBuildingControl.Instance.NotifyWorkerArrival();

                GameObject worker = other.gameObject;
                WorkersInterBuildingControl.Instance.StartAnimationBuilding(worker.GetComponent<WorkerMovementController>(), GetComponent<BuildingData>(), spawnWorker);
                
                worker.SetActive(false);
                return;
            }
            // Рабочий прибыл не для строительства
            else if (_buildingData.IsThisBuilt && !workerMovementController.ArriveForBuildBuidling && workerMovementController.SelectedBuilding.GetComponent<BuildingData>().buildingTypeSO.IDoB == GetComponent<BuildingData>().buildingTypeSO.IDoB)
            {
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
                            text.text = $"{_buildingData.Title} запущена ({thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding}/1) \n Нажмите E чтобы выгрузить рабочего";
                        }
                        else
                        {
                            text.text = $"Нажмите E чтобы выгрузить одного рабочего ({thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding}/2)";
                        }

                        Destroy(other.gameObject.transform.parent.gameObject);
                        
                        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
                        playerSaveData.BuildingWorkersInformationList[_buildingData.SaveListIndex]
                                .CurrentNumberOfWorkersInThisBuilding =
                            thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding;
                        return;
                    }
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && PossiblityPutEInThisBuilding)
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
