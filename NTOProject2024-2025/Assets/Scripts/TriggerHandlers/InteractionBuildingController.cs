using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class InteractionBuildingController : MonoBehaviour
{
    private bool CanPutE;
    [SerializeField] private bool PossiblityPutEInThisBuilding;
    
    [SerializeField] private GameEvent OpenDescriptionPanel;
    public GameEvent OpenBarterMenuEvent;
    public GameEvent CloseBarterMenuEvent;
    private BuildingData _buildingData;

    [Header("InteractionEvents")]
    [SerializeField] private UnityEvent InteractionEvent;
    [SerializeField] private UnityEvent TextOnEvent;
    

    public GameObject Texthint;

    public List<Transform> PointsOfBuildings;

    public Transform spawnWorker;

    [SerializeField] private LayerMask placementLayerMask; // Для клика по зданию

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
                        
                        PlayerSaveData playerSaveData = UIManagerLocation.Instance.WhichPlayerDataUse();
                        playerSaveData.BuildingWorkersInformationList[_buildingData.SaveListIndex]
                                .CurrentNumberOfWorkersInThisBuilding =
                            thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding;
                        return;
                    }
                }
            }
            
            
            // Если данное здание является текущим у данного рабочего
            //if (WorkersInterBuildingControl.CurrentBuilding == gameObject.GetComponent<BuildingData>() && WorkersInterBuildingControl.CurrentBuilding.Title == GetComponent<BuildingData>().Title)
            //{
                
                // Если данное здание может содержать рабочих, и при этом рабочий не занят постройкой здания + здание вообще построено
                //if (GetComponent<ThisBuildingWorkersControl>() && !other.gameObject.GetComponent<WorkerMovementController>().ReadyForWork && BuildingManager.Instance.ProcessWorkerBuildingActive && _buildingData.IsThisBuilt)
                //{
                    //BuildingManager.Instance.ProcessWorkerBuildingActive = false;
                    //return;
                //} 
            //}
            
            // Рабочий прибыл для работы на пасеке/пристани
            //if ((other.gameObject.GetComponent<WorkerMovementController>().SelectedBuilding.GetComponent<BuildingData>().buildingTypeSO.IDoB == GetComponent<BuildingData>().buildingTypeSO.IDoB) && other.gameObject.GetComponent<WorkerMovementController>().ReadyForWork)
            //{
                
            //}
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
    }
    
    /// <summary>
    /// Закрытие меню бартера
    /// </summary>
    public void CloseBarterMenu()
    {
        CloseBarterMenuEvent.TriggerEvent();
    }
}
