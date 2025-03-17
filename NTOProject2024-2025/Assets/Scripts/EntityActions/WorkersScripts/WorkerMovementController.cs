using EntityActions.Movement_Control;
using EntityActions.WorkersScripts;
using TMPro;
using Unitilities;
using UnityEngine;
using UnityEngine.AI;

public class WorkerMovementController : MonoBehaviour, IUnitMovement, IWorkerUnit
{
    [Header("Properties")]
    public bool isSelected { get; set; }
    public GameObject OutlineRotate { get { return outlineRotate; } }
    public bool isSelecting { get; set; } // Мышь наведена на персонажа
    public UnitType ThisUnitType { get { return unitType; } }
    
    public GameObject SelectedBuilding
    {
        get
        {
            return selectedBuilding;
        }
        set
        {
            selectedBuilding = value;
        }
    }
    
    public bool PossibilityClickOnUnit { get; set; }
    
    public bool ReadyForWork { get; set; }
    
    public bool ArriveForBuildBuidling {get; set;}
    
    public GameObject OutlinePOD {get{ return outlinePOD;}}
    
    public Transform UnitPointOfDestination
    {
        get
        {
            return unitPointOfDestination;
        }
        set
        {
            unitPointOfDestination = value;
        }
    }
    
    public Camera MainCamera
    {
        get
        {
            return camera;
        }
        set
        {
            camera = value;
        }
    }
    
    
    [Header("Flags")]
    private bool IsClickOnOtherEntity; // Кликнули на другой тип сущности
    
    [Header("Visual")]
    public GameObject outlineRotate;
    public GameObject outlinePOD;
    private Animator anim;
    
    [Header("System")]
    [SerializeField] private string NameOfTTS;
    public Transform unitPointOfDestination;
    private NavMeshAgent agent;
    public GameObject selectedBuilding; // techTriggerScripts
    public Camera camera;
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
        PossibilityClickOnUnit = true;
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
        if(isSelected && GeneralWorkersControl.possiilityControlEntities){
            
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
                            HintBuildingUpdate(GeneralWorkersControl.Instance.HintNoBeAbleToBuildWorker, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не является конструктором, он не может потроить это здание </color>", 0);
                            return;
                        }
                    }
                    else 
                    {
                        // Рабочий не побежит к зданию с возможностью содержать рабочих, если там не осталось места
                        if (SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().MaxValueOfWorkersInThisBuilding)
                        {
                            HintBuildingUpdate(GeneralWorkersControl.Instance.FullWorkerInThisBuilding, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не пролезет в здании нет места </color>", 1);
                            return;
                        }
                        // Чувак, я пасечник а не работяга ! 
                        else if (SelectedBuilding.GetComponent<EnergyProduction>() && _thisWorkerData.unitType != SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().suitableUnitDataForThisBuilding)
                        {
                            HintBuildingUpdate(GeneralWorkersControl.Instance.HintNotNeededWorkerType, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не подходи по роли для данного здания </color>",  0);
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

        
        if (unitPointOfDestination) 
        {
            
            // Рабочий идет до точки назначения
            anim.SetBool("Idle", false);
            anim.SetBool("Running", true);
            agent.isStopped = false;
            agent.destination = new Vector3(unitPointOfDestination.position.x, unitPointOfDestination.position.y, unitPointOfDestination.position.z);
            
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
            unitPointOfDestination = currentWalkingPoint.transform;
            //Debug.Log($"Setting destination to: {currentWalkingPoint.transform.position}");
        } else {
            unitPointOfDestination = point;
            //Debug.Log($"Setting destination to: {point.position}");
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "WalkingPoint"){
            Debug.Log("Рабочий дошел до точки назначения");
            currentWalkingPoint.gameObject.SetActive(false);
            unitPointOfDestination = null;
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
            InteractionBuildingController interactionBuildingController = buildingData.gameObject.GetComponent<InteractionBuildingController>();
            TextMeshPro text = buildingData.AwaitBuildingThisTMPro;
            string newText = " ";
            switch (mode)
            {
                case 0:
                    newText 
                        = $"{WhichTypeActionText}:\n{notNeededWorkerAttention} ";
                    break;
                case 1:
                    newText 
                        = $"{WhichTypeActionText}";
                    break;
            }
            TemporaryText(interactionBuildingController, text, newText);
        }
    }

    private void OnDisable()
    {
        if (isSelected)
        {
            GeneralWorkersControl.SelectedUnit = null;
            UIManager.CancelLastOpenPanelEvent -= GeneralWorkersControl.Instance.ResetSelectedUnit;
        }
    }
    
    /// <summary>
    /// Показ текста, который пропадет через определенное время 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="whichText"></param>
    private void TemporaryText(InteractionBuildingController interactionBuildingController, TextMeshPro text, string whichText)
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
        }, interactionBuildingController.textOnTime);
    }
}
