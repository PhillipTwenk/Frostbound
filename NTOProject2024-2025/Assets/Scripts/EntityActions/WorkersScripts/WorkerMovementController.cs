using System;
using UnityEngine;
using UnityEngine.AI;
public class WorkerMovementController : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective MovementWorkerTutorial;
    [SerializeField] private TutorialObjective WorkerStartMovementToApiaryTutorial;
    private bool IsWorkerMove;
    private bool IsWorkerMovetoApiary;

    [SerializeField] private string NameOfTTS;
    public Transform WorkerPointOfDestination;
    private NavMeshAgent agent;
    public bool ReadyForWork;
    public bool ArriveForBuildBuidling;
    public bool isSelected;
    public bool isSelecting; // Мышь наведена на персонажа
    public bool possibilityClickOnWorker;
    private Animator anim;
    public GameObject SelectedBuilding; // techTriggerScripts
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask workerLayerMask;
    public Camera MainCamera;
    [SerializeField] private Transform currentWalkingPoint;
    //[SerializeField] private Material OutlineMaterial;
    //[SerializeField] private Color OutlineColor;
    //[SerializeField] private Color BasedOutlineColor;
    public GameObject OutlineRotate;
    public GameObject OutlinePOD;
    
    private Rigidbody _rb;
    void Start()
    {
        possibilityClickOnWorker = true;
        currentWalkingPoint.gameObject.SetActive(false);
        ReadyForWork = true;
        agent = GetComponent<NavMeshAgent>();
        isSelected = false;
        isSelecting = false;
        anim = GetComponent<Animator>();
        Debug.Log(agent);
        _rb = GetComponent<Rigidbody>(); 
        OutlinePOD.SetActive(false);
        OutlineRotate.SetActive(false);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // Обнуляем горизонтальную скорость
    }

    void Update()
    {
        if(isSelected && possibilityClickOnWorker){
            
            if (Input.GetMouseButtonDown(0) && !isSelecting)
            {
                Vector3 point = GetSelectedMapPosition();
                currentWalkingPoint.gameObject.SetActive(true);
                
                // Если клинкули не на здание
                if(SelectedBuilding == null){
                    currentWalkingPoint.transform.position = new Vector3(point.x, point.y, point.z);
                    ArriveForBuildBuidling = false;
                    if (!IsWorkerMove)
                    {
                        MovementWorkerTutorial.CheckAndUpdateTutorialState();
                        IsWorkerMove = true;
                        Debug.Log("Рабочий начал движение");
                    }
                } else {
                    // Если выбранное здание в процессе строительства и рабочий свободен, он идет его строить
                    if (!SelectedBuilding.gameObject.GetComponent<BuildingData>().IsThisBuilt)
                    {
                        if (ReadyForWork)
                        {
                            ArriveForBuildBuidling = true;
                        }
                    }
                    else
                    {
                        // Если выбранное здание уже построено, проверяем есть ли у него возможность содержать рабочих
                        // Если нет, рабочий не двинется
                        if (!SelectedBuilding.GetComponent<ThisBuildingWorkersControl>())
                        {
                            return;
                        }
                        // Рабочий не побежит к зданию с возможностью содержать рабочих, если там не осталось места
                        else if (SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= SelectedBuilding.GetComponent<ThisBuildingWorkersControl>().MaxValueOfWorkersInThisBuilding)
                        {
                            return;
                        }
                    }
                    
                    
                    currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
                    
                    if (!IsWorkerMovetoApiary)
                    {
                        WorkerStartMovementToApiaryTutorial.CheckAndUpdateTutorialState();
                        IsWorkerMovetoApiary = true;
                    }
                }
                SetWorkerDestination(currentWalkingPoint.transform, false);
            }
        }
        // if (!Input.GetMouseButtonDown(0))
        // {
        //     MouseDownOnWorker(false); // Наводим мышку на персонажа
        //     return;
        // }
        // if (Input.GetMouseButtonDown(0) && isSelecting)
        // {
        //     MouseDownOnWorker(true); // Нажимаем на рабочего
        //     return;
        // }

        
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
            //if (SelectedBuilding){
                //SelectedBuilding = null;
            //}
        }
    }

    // public void MouseDownOnWorker(bool OnClick)
    // {
    //     Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition); 
    //     RaycastHit hit;
    //     Debug.DrawRay(ray.origin, ray.direction * 10000f, Color.yellow, 5f);
    //
    //     if (Physics.Raycast(ray, out hit, 10000f, workerLayerMask))
    //     {
    //         if (hit.collider.CompareTag("ClickOnWorker"))
    //         {
    //             if (OnClick)
    //             {
    //                 if (possibilityClickOnWorker)
    //                 {
    //                     Debug.Log("Нажали на рабочего");
    //                     if (!isSelected) 
    //                     {
    //                         if (WorkersInterBuildingControl.SelectedWorker != null)
    //                         {
    //                             WorkersInterBuildingControl.SelectedWorker.isSelected = false;
    //                             WorkersInterBuildingControl.SelectedWorker.isSelecting = false;
    //                         }
    //                         OutlineRotate.SetActive(true);
    //                         isSelected = true;
    //                         WorkersInterBuildingControl.SelectedWorker = this;
    //                     } 
    //                     else 
    //                     {
    //                         OutlineRotate.SetActive(false);
    //                         isSelected = false;
    //                         WorkersInterBuildingControl.SelectedWorker = null;
    //                     }
    //                 }
    //             }
    //             else // Наведение без клика
    //             {
    //                 isSelecting = true;
    //                 if (!isSelected && possibilityClickOnWorker)
    //                 {
    //                     OutlineRotate.SetActive(true);
    //                 }
    //             }
    //             return; // Выход из метода, если Raycast попал в рабочего
    //         }
    //     }
    //
    //     // Если Raycast НЕ попал в рабочего, сбрасываем выделение
    //     isSelecting = false;
    //     if (!isSelected && possibilityClickOnWorker)
    //     {
    //         OutlineRotate.SetActive(false);
    //     }
    // }

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
            }
            else
            {
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
    // private void OnMouseDown() {
    //     if (possibilityClickOnWorker)
    //     {
    //         if (!isSelected) {
    //             if (WorkersInterBuildingControl.SelectedWorker != null)
    //             {
    //                 WorkersInterBuildingControl.SelectedWorker.isSelected = false;
    //                 WorkersInterBuildingControl.SelectedWorker.isSelecting = false;
    //             }
    //             OutlineRotate.SetActive(true);
    //             isSelected = true;
    //             WorkersInterBuildingControl.SelectedWorker = this;
    //         } else {
    //             OutlineRotate.SetActive(false);
    //             isSelected = false;
    //             WorkersInterBuildingControl.SelectedWorker = this;
    //         }
    //     }
    // }

    // private void OnMouseEnter() {
    //     isSelecting = true;
    //     if(!isSelected && possibilityClickOnWorker)
    //     {
    //         OutlineRotate.SetActive(true);
    //         //OutlineMaterial.color = OutlineColor;
    //     }
    // }
    //
    // private void OnMouseExit() {
    //     isSelecting = false;
    //     if(!isSelected && possibilityClickOnWorker){
    //         OutlineRotate.SetActive(false);
    //         //OutlineMaterial.color = BasedOutlineColor;
    //     }
    // }

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
}
