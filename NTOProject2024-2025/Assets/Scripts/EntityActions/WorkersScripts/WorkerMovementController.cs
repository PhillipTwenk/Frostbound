using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class WorkerMovementController : MonoBehaviour
{
    // [Header("Tutorial")]
    // [SerializeField] private TutorialObjective MovementWorkerTutorial;
    // [SerializeField] private TutorialObjective WorkerStartMovementToApiaryTutorial;
    // private bool IsWorkerMove;
    // private bool IsWorkerMovetoApiary;
    
    [Header("Flags")]
    public bool ReadyForWork;
    private bool IsClickOnOtherEntity; // Кликнули на игрока
    public bool ArriveForBuildBuidling;
    public bool isSelected;
    public bool isSelecting; // Мышь наведена на персонажа
    public bool possibilityClickOnWorker;
    
    [Header("Visual")]
    public GameObject OutlineRotate;
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
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // Обнуляем горизонтальную скорость
    }

    void Update()
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
                    // if (!IsWorkerMove)
                    // {
                    //     //MovementWorkerTutorial.CheckAndUpdateTutorialState();
                    //     IsWorkerMove = true;
                    //     Debug.Log("Рабочий начал движение");
                    // }
                } else {
                    // Если выбранное здание в процессе строительства и рабочий свободен, он идет его строить
                    if (SelectedBuilding != null && !SelectedBuilding.gameObject.GetComponent<BuildingData>().IsThisBuilt)
                    {
                        if (_thisWorkerData.workerType == WorkersType.Constructor)
                        {
                            if (ReadyForWork)
                            {
                                ArriveForBuildBuidling = true;
                            }
                        }
                        else
                        {
                            HintBuildingUpdate(WorkersInterBuildingControl.Instance.HintNoBeAbleToBuildWorker, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не является конструктором, он не может потроить это здание </color>");
                            return;
                        }
                    }
                    else if (SelectedBuilding != null)
                    {
                        // Если выбранное здание уже построено, проверяем есть ли у него возможность содержать рабочих
                        // Если нет, рабочий не двинется
                        if (!SelectedBuilding.GetComponent<ThisBuildingWorkersControl>())
                        {
                            return;
                        }
                        // Рабочий не побежит к зданию с возможностью содержать рабочих, если там не осталось места
                        else if (SelectedBuilding != null && SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().MaxValueOfWorkersInThisBuilding)
                        {
                            return;
                        }
                        // Чувак, я пасечник а не работяга ! 
                        else if (SelectedBuilding != null && SelectedBuilding.GetComponent<EnergyProduction>() && _thisWorkerData.workerType != SelectedBuilding.GetComponent<EnergyProduction>().suitableWorkerDataForThisBuilding)
                        {
                            HintBuildingUpdate(WorkersInterBuildingControl.Instance.HintNotNeededWorkerType, SelectedBuilding.gameObject.GetComponent<BuildingData>(), "<color=blue> Данный рабочий не подходи по роли для данного здания </color>");
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
                SetWorkerDestination(currentWalkingPoint.transform, false);
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
                Debug.Log($"текущее здание для пострйоки{SelectedBuilding}");
                IsClickOnOtherEntity = false;
            }
            else if (hit.collider.CompareTag("ClickOnWorker") || hit.collider.CompareTag("Player"))
            {
                Debug.Log("Кликнуи на игрока или другого рабочего");
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

    
    
    public void SetWorkerDestination(Transform point, bool isAutomatic){
        if(isAutomatic && SelectedBuilding != null){
            currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
            WorkerPointOfDestination = currentWalkingPoint.transform;
            //Debug.Log($"Setting destination to: {currentWalkingPoint.transform.position}");
        } else {
            WorkerPointOfDestination = point;
            //Debug.Log($"Setting destination to: {point.position}");
        }
    }

    private void OnDisable()
    {
        if (isSelected)
        {
            WorkersInterBuildingControl.SelectedWorker = null;
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
    private void HintBuildingUpdate(string WhichTypeActionText, BuildingData buildingData, string debug)
    {
        Debug.Log(debug);

        if (buildingData.AwaitBuildingThisTMPro.text != $"{WhichTypeActionText}:\n{notNeededWorkerAttention} ")
        {
            string oldText = buildingData.AwaitBuildingThisTMPro.text;
            buildingData.AwaitBuildingThisTMPro.text 
                = $"{WhichTypeActionText}:\n{notNeededWorkerAttention} ";
            Utility.Invoke(this, () => SelectedBuilding.gameObject.GetComponent<BuildingData>().AwaitBuildingThisTMPro.text = oldText, 4f);
        }
    }
    
}
