using EntityActions.Movement_Control;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DroneMovementController : MonoBehaviour, IUnitMovement
{
    [Header("Properties")]
    public bool isSelected { get; set; }
    public GameObject OutlineRotate { get { return outlineRotate; } }
    public bool isSelecting { get; set; }
    
    public UnitType ThisUnitType { get { return unitType; } }
    
    [Header("Flags")]
    public bool ReadyForWork;
    private bool IsClickOnOtherEntity;
    public bool ArriveForBuildBuidling;
    public bool possibilityClickOnWorker;
    public bool isFlyNow;
    public bool isPlaceNow;
    public bool isTakingOff; 
    public bool isLanding;
    
    [Header("Visual")]
    public GameObject outlineRotate;
    public GameObject OutlinePOD;

    [Header("Animations")]
    [SerializeField] private string droneFly_AK;
    private Animator anim;
    
    [Header("Core")]
    [SerializeField] private string NameOfTTS;
    public Transform DronePointOfDestination;
    public GameObject SelectedBuilding;
    [SerializeField] private Transform currentWalkingPoint;
    private Vector3 targetLandingPosition = Vector3.zero;
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
        possibilityClickOnWorker = true;
        
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
            DronePointOfDestination = currentWalkingPoint.transform;
        } 
        else 
        {
            DronePointOfDestination = point;
        }
    }

    public void MovementHandler()
    {
        if (isSelected && WorkersInterBuildingControl.possiilityControlEntities)
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

        if (DronePointOfDestination)
        {
            agent.isStopped = false;
            agent.destination = DronePointOfDestination.position;
        } 
        else 
        {
            agent.isStopped = true;
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
        Debug.Log("Начата корутина взлета");
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, droneFlyHeight, startPosition.z);

        float j = 0;
        float k = 0.77f;
        while (Vector3.Distance(transform.position, targetPosition) > 0.5f && transform.position.y < droneFlyHeight)
        {
            // transform.position = Vector3.Lerp(transform.position, targetPosition, upSpeed * Time.deltaTime);
            agent.baseOffset += (upSpeed - (j+k)) * Time.deltaTime;
            Debug.Log("Взлет");
            j = j + k;
            yield return null;
        }

        isTakingOff = false;
        agent.enabled = true;
    }

    public void StartLanding()
    {
        if (!isLanding && isFlyNow && !isTakingOff)
        {
            targetLandingPosition = FindLandingSpot();
            Debug.Log("Дрон садится");
            isLanding = true;
            isFlyNow = false;
            StartCoroutine(LandingCoroutine());
        }
    }

    private IEnumerator LandingCoroutine()
    {
        Debug.Log("Начата корутина посадки");

        float j = 0;
        float k = 0.77f;
        while (Vector3.Distance(transform.position, targetLandingPosition) > 0.5f && transform.position.y > 2 && agent.baseOffset <= 1)
        {
            Debug.Log("Посадка");
            //transform.position = Vector3.Lerp(transform.position, targetLandingPosition, downSpeed * Time.deltaTime);
            agent.baseOffset -= (upSpeed - (j+k)) * Time.deltaTime;
            j = j + k;
            yield return null;
        }

        isLanding = false;
        isPlaceNow = true;
    }

    private Vector3 FindLandingSpot()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position;

        if (Physics.BoxCast(rayStart, landingBoxSize / 2, Vector3.down, out hit, Quaternion.identity, placementAfterFlyLayerMask))
        {
            Debug.Log("Найдено место посадки");
            return hit.point;
        }

        Debug.Log("не Найдено место посадки");
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 boxPosition = transform.position + Vector3.up * 2;
        Gizmos.DrawWireCube(boxPosition, landingBoxSize);
    }
}
