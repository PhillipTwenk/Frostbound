using System;
using EntityActions.Movement_Control;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class WorkerMovementController : MonoBehaviour, IUnitMovement
{
    [Header("Properties")]
    public bool isSelected { get; set; }
    public GameObject OutlineRotate { get { return outlineRotate; } }
    public bool isSelecting { get; set; } // Мышь наведена на персонажа
    public UnitType ThisUnitType { get { return unitType; } }
    
    
    [Header("Flags")]
    public bool ReadyForWork;
    private bool IsClickOnOtherEntity; // Кликнули на другой тип сущности
    public bool ArriveForBuildBuidling;
    // public bool isSelected;
    public bool possibilityClickOnWorker;
    
    [Header("Visual")]
    public GameObject outlineRotate;
    public GameObject OutlinePOD;
    private Animator anim;
    
    [Header("System")]
    [SerializeField] private string NameOfTTS;
    public Transform WorkerPointOfDestination;
    private NavMeshAgent agent;
    public GameObject SelectedBuilding; // techTriggerScripts
    public Camera MainCamera;
    [SerializeField] private Transform currentWalkingPoint;
    private Rigidbody _rb;
    private WorkerData _thisWorkerData;
    private UnitType unitType;
    
    [Header("LayerMasks")]
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask workerLayerMask;

    [Header("Texts for hints")] [TextArea] [SerializeField]
    private string notNeededWorkerAttention;
    void Start()
    {
        IsClickOnOtherEntity = false;
        possibilityClickOnWorker = true;
        currentWalkingPoint.gameObject.SetActive(false);
        ReadyForWork = true;
        agent = GetComponent<NavMeshAgent>();
        isSelected = false;
        isSelecting = false;
        anim = GetComponent<Animator>();
        Debug.Log(agent);
        _rb = GetComponent<Rigidbody>();
        _thisWorkerData = GetComponent<WorkerData>();
        OutlinePOD.SetActive(false);
        OutlineRotate.SetActive(false);
        unitType = _thisWorkerData.unitType;
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // Обнуляем горизонтальную скорость
    }

    void Update()
    {
        MovementHandler();
    }

    /// <summary>
    /// Управление движением юнита
    /// </summary>
    public void MovementHandler()
    {
        if(isSelected && WorkersInterBuildingControl.possiilityControlEntities){
            
            if (Input.GetMouseButtonDown(0) && !isSelecting)
            {
                Vector3 point = GetSelectedMapPosition();
                currentWalkingPoint.gameObject.SetActive(true);
                
                // Если клинкули не на здание и не на другую сущность
                if(SelectedBuilding == null && !IsClickOnOtherEntity){
                    currentWalkingPoint.transform.position = new Vector3(point.x, point.y, point.z);
                    ArriveForBuildBuidling = false;
                } else if (SelectedBuilding != null){
                    // Если выбранное здание в процессе строительства и рабочий свободен, он идет его строить
                    if (!SelectedBuilding.gameObject.GetComponent<BuildingData>().IsThisBuilt)
                    {
                        if (_thisWorkerData.unitType == UnitType.Constructor)
                        {
                            if (ReadyForWork)
                            {
                                ArriveForBuildBuidling = true;
                            }
                        }
                        else
                        {
                            HintBuildingUpdate(WorkersInterBuildingControl.Instance.HintNoBeAbleToBuildWorker, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не является конструктором, он не может потроить это здание </color>", 0);
                            return;
                        }
                    }
                    else 
                    {
                        // Рабочий не побежит к зданию с возможностью содержать рабочих, если там не осталось места
                        if (SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().MaxValueOfWorkersInThisBuilding)
                        {
                            HintBuildingUpdate(WorkersInterBuildingControl.Instance.FullWorkerInThisBuilding, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не пролезет в здании нет места </color>", 1);
                            return;
                        }
                        // Чувак, я пасечник а не работяга ! 
                        else if (SelectedBuilding.GetComponent<EnergyProduction>() && _thisWorkerData.unitType != SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().suitableUnitDataForThisBuilding)
                        {
                            HintBuildingUpdate(WorkersInterBuildingControl.Instance.HintNotNeededWorkerType, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не подходи по роли для данного здания </color>",  0);
                            return;
                        }
                    }
                    currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
                    // if (!IsWorkerMovetoApiary)
                    // {
                    //     //WorkerStartMovementToApiaryTutorial.CheckAndUpdateTutorialState();
                    //     IsWorkerMovetoApiary = true;
                    // }
                }
                SetUnitDestination(currentWalkingPoint.transform, false);
            }
        }

        
        if (WorkerPointOfDestination) 
        {
            
            // Рабочий идет до точки назначения
            anim.SetBool("Idle", false);
            anim.SetBool("Running", true);
            agent.isStopped = false;
            agent.destination = new Vector3(WorkerPointOfDestination.position.x, WorkerPointOfDestination.position.y, WorkerPointOfDestination.position.z);
            
        } 
        else 
        {
            // Рабочий дошел до точки назначения
            agent.isStopped = true;
            anim.SetBool("Running", false);
            anim.SetBool("Idle", true);
            OutlinePOD.SetActive(false);
        }
    }

    /// <summary>
    /// Получение информации о нажатой на карте точке
    /// Учитывается только нажатие по указанным в placementLayerMask слоям
    /// </summary>
    /// <returns> Позиция нажатой точки </returns>
    public Vector3 GetSelectedMapPosition()
    {
        Vector3 lastPosition = Vector3.zero;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition); 
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10000f, Color.red, 5f);
        if (Physics.Raycast(ray, out hit, 10000f, placementLayerMask, QueryTriggerInteraction.Ignore))
        {
            lastPosition = hit.point; // Выбранная точка

            if (hit.collider.CompareTag("ClickOnBuilding"))
            {
                SelectedBuilding = hit.collider.gameObject.transform.parent.gameObject; // Выбранное здание
                Debug.Log($"Текущее здание этого юнита {SelectedBuilding.GetComponent<BuildingData>().Title}");
                IsClickOnOtherEntity = false;
            }
            else if (hit.collider.CompareTag("ClickOnWorker") || hit.collider.CompareTag("Player"))
            {
                Debug.Log("Кликнули на игрока или другого рабочего");
                IsClickOnOtherEntity = true;
                SelectedBuilding = null;
            }
            else
            {
                IsClickOnOtherEntity = false;
                SelectedBuilding = null;
                OutlinePOD.SetActive(true);
            }
        }

        return lastPosition;
    }

    
    /// <summary>
    /// Задать направление пути юниту
    /// </summary>
    /// <param name="point"></param>
    /// <param name="isAutomatic"></param>
    public void SetUnitDestination(Transform point, bool isAutomatic){
        if(isAutomatic && SelectedBuilding != null){
            currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
            WorkerPointOfDestination = currentWalkingPoint.transform;
            //Debug.Log($"Setting destination to: {currentWalkingPoint.transform.position}");
        } else {
            WorkerPointOfDestination = point;
            //Debug.Log($"Setting destination to: {point.position}");
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "WalkingPoint"){
            Debug.Log("Рабочий дошел до точки назначения");
            currentWalkingPoint.gameObject.SetActive(false);
            WorkerPointOfDestination = null;
            anim.SetBool("Running", false);
            anim.SetBool("Idle", true);
        } 
    }

    /// <summary>
    /// Добваление текста в подскакзки при выводе информации об ошибке, связанной с типами рабочих
    /// </summary>
    /// <param name="buildingData"></param>
    private void HintBuildingUpdate(string WhichTypeActionText, BuildingData buildingData, string debug, int mode)
    {
        Debug.Log(debug);

        if (buildingData.AwaitBuildingThisTMPro.text != $"{WhichTypeActionText}:\n{notNeededWorkerAttention} " && !buildingData.gameObject.GetComponent<InteractionBuildingController>().IsTextStartWorkingActive)
        {
            string oldText = buildingData.AwaitBuildingThisTMPro.text;
            bool oldTMPStateBool = buildingData.AwaitBuildingThisTMPro.gameObject.activeSelf;
            
            buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(true);
            switch (mode)
            {
                case 0:
                    buildingData.AwaitBuildingThisTMPro.text 
                        = $"{WhichTypeActionText}:\n{notNeededWorkerAttention} ";
                    break;
                case 1:
                    buildingData.AwaitBuildingThisTMPro.text 
                        = $"{WhichTypeActionText}";
                    break;
            }
            Utility.Invoke(this, () =>
            {
                buildingData.AwaitBuildingThisTMPro.text = oldText;
                Debug.Log($"Прошлое состояние текста над зданием {buildingData.Title}: {oldTMPStateBool}");
                if (!oldTMPStateBool)
                {
                    buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(false);
                }
            }, 4f);
        }
    }

    private void OnDisable()
    {
        if (isSelected)
        {
            WorkersInterBuildingControl.SelectedUnit = null;
            UIManager.CancelLastOpenPanelEvent -= WorkersInterBuildingControl.Instance.ResetSelectedUnit;
        }
    }
}
