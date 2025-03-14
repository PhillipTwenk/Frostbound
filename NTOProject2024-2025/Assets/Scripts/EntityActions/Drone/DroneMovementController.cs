using EntityActions.Movement_Control;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using EntityActions.WorkersScripts;
using UnityEngine.Serialization;

public class DroneMovementController : MonoBehaviour, IUnitMovement, IWorkerUnit
{
    [Header("Properties")]
    public bool isSelected { get; set; }
    public GameObject OutlineRotate { get { return outlineRotate; } }
    public bool isSelecting { get; set; }
    
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
    
    
    [Header("Flags")]
    private bool IsClickOnOtherEntity;
    public bool isFlyNow;
    public bool isPlaceNow;
    public bool isTakingOff; 
    public bool isLanding;
    
    [Header("Visual")]
    public GameObject outlineRotate;
    public GameObject outlinePOD;

    [Header("Animations")]
    [SerializeField] private string droneFly_AK;
    private Animator anim;
    
    [Header("Core")]
    [SerializeField] private string NameOfTTS;

    public Transform unitPointOfDestination;
    public GameObject selectedBuilding;
    [SerializeField] private Transform currentWalkingPoint;
    private UnitType unitType;
    
    [Header("Components")]
    private NavMeshAgent agent;
    private Rigidbody _rb;
    public Camera MainCamera;
    private WorkerData _thisWorkerData;
    
    [Header("LayerMasks")]
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask workedLayerMask;
    [SerializeField] private LayerMask placementAfterFlyLayerMask;

    [Header("Drone parameters")]
    [SerializeField] private float awaitBeforeFly;
    [SerializeField] private float awaitBeforePlace;
    [SerializeField] private float upSpeed;
    [SerializeField] private float downSpeed;
    [SerializeField] private float droneFlyHeight;
    [SerializeField] private Vector3 landingBoxSize; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _thisWorkerData = GetComponent<WorkerData>(); 
        
        ReadyForWork = true;
        isSelected = false;
        isSelecting = false;
        IsClickOnOtherEntity = false;
        PossibilityClickOnUnit = true;
        
        currentWalkingPoint.gameObject.SetActive(false);
        OutlinePOD.SetActive(false);
        OutlineRotate.SetActive(false);

        unitType = _thisWorkerData.unitType;

        agent.enabled = true; 
        _rb.useGravity = false;
    }

    void Update()
    {
        if (isFlyNow && isSelected)
        {
            MovementHandler();
        }
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 lastPosition = Vector3.zero;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition); 
        Debug.DrawRay(ray.origin, ray.direction * 10000f, Color.red, 5f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000f, placementLayerMask, QueryTriggerInteraction.Ignore))
        {
            lastPosition = hit.point;

            if (hit.collider.CompareTag("ClickOnBuilding"))
            {
                SelectedBuilding = hit.collider.gameObject.transform.parent.gameObject;
                IsClickOnOtherEntity = false;
            }
            else if (hit.collider.CompareTag("ClickOnWorker") || hit.collider.CompareTag("Player"))
            {
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

    public void SetUnitDestination(Transform point, bool isAutomatic)
    {
        if(isAutomatic && SelectedBuilding != null)
        {
            currentWalkingPoint.transform.position = new Vector3(SelectedBuilding.transform.position.x, SelectedBuilding.transform.position.y + droneFlyHeight, SelectedBuilding.transform.position.z);
            UnitPointOfDestination = currentWalkingPoint.transform;
        } 
        else 
        {
            UnitPointOfDestination = point;
        }
    }

    public void MovementHandler()
    {
        if (isSelected && WorkersInterBuildingControl.possiilityControlEntities && isFlyNow)
        {
            if (Input.GetMouseButtonDown(0) && !isSelecting)
            {
                Vector3 point = GetSelectedMapPosition();
                currentWalkingPoint.gameObject.SetActive(true);
                
                if (SelectedBuilding == null && !IsClickOnOtherEntity)
                {
                    currentWalkingPoint.transform.position = new Vector3(point.x, droneFlyHeight, point.z);
                    ArriveForBuildBuidling = false;
                }
                else if (SelectedBuilding != null)
                {
                    if (!SelectedBuilding.GetComponent<BuildingData>().IsThisBuilt)
                    {
                        if (_thisWorkerData.unitType == UnitType.MainDrone)
                        {
                            if (ReadyForWork)
                            {
                                ArriveForBuildBuidling = true;
                            }
                        }
                    }
                    currentWalkingPoint.transform.position = new Vector3(SelectedBuilding.transform.position.x, SelectedBuilding.transform.position.y + droneFlyHeight, SelectedBuilding.transform.position.z);
                }
                SetUnitDestination(currentWalkingPoint.transform, false);
            }
        }

        if (UnitPointOfDestination != null)
        {
            // Проверяем, активен ли агент и на NavMesh
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(UnitPointOfDestination.position);
            }
            else
            {
                Debug.LogWarning("Агент неактивен или не на NavMesh!");
            }
        }
        else
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }
    }

    public void StartTakeoff()
    {
        if (!isTakingOff && !isFlyNow && !isLanding)
        {
            Debug.Log("Дрон взлетает");
            isTakingOff = true;
            isFlyNow = true;
            isPlaceNow = false;
            isLanding = false;
            StartCoroutine(TakeoffCoroutine());
        }
    }

    private IEnumerator TakeoffCoroutine()
    {
        agent.enabled = false;
        
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, droneFlyHeight, startPosition.z);
        
        while (Vector3.Distance(transform.position, targetPosition) >= 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * upSpeed);
            Debug.Log(".");
            yield return null;
        }
        
        agent.enabled = true;
        agent.Warp(targetPosition);
        
        yield return new WaitUntil(() => agent.isOnNavMesh);

        
        if (UnitPointOfDestination != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(UnitPointOfDestination.position);
        }
        
        isTakingOff = false;
    }

    public void StartLanding()
    {
        if (!isLanding && isFlyNow && !isTakingOff)
        {
            Debug.Log("Дрон садится");
            isLanding = true;
            isFlyNow = false;
            StartCoroutine(LandingCoroutine());
        }
    }

    private IEnumerator LandingCoroutine()
    {
        agent.enabled = false;
        Vector3 groundPosition = FindGroundPosition();

        // Плавное опускание
        while (Vector3.Distance(transform.position, groundPosition) >= 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, groundPosition, Time.deltaTime * downSpeed);
            Debug.Log(".");
            yield return null;
        }

        // Включаем агент и синхронизируем с NavMesh
        agent.enabled = true;
        agent.Warp(groundPosition);

        isLanding = false;
        isPlaceNow = true;
    }

    private Vector3 FindGroundPosition()
    {
        int groundAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
        
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1000f, groundAreaMask))
        {
            Debug.Log($"Найдена точка посадки: {hit.position}");
            return hit.position;
        }
        else
        {
            Debug.LogError("Не удалось найти точку на NavMesh для посадки!");
            return transform.position; // Возвращаем текущую позицию как fallback
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 boxPosition = transform.position + Vector3.up * 2;
        Gizmos.DrawWireCube(boxPosition, landingBoxSize);
    }
}
