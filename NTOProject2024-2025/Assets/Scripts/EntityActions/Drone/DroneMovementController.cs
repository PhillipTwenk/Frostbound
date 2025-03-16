using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using EntityActions.WorkersScripts;
using Unity.VisualScripting;

public class DroneMovementController : MonoBehaviour, IWorkerUnit, IUnitLogistics
{
    #region Свойства

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

    public BuildingData buildingDataLogistics
    {
        get
        {
            return _buidlingData;
        }
        set
        {
            _buidlingData = value;
        }
    }
    
    public bool IsLogisticsCycleActive { get; set; }
    
    public int LogisticsStorage { get; set; }

    public List<int> MaximumLogisticsStorage
    {
        get
        {
            return limitsDroneResources;
        }
    }

    #endregion

    #region Переменные

    [Header("Flags")]
    private bool IsClickOnOtherEntity;
    public bool isFlyNow;
    public bool isPlaceNow;
    public bool isTakingOff; 
    public bool isLanding;
    public bool isMovingToLandingSpot;
    
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
    private WorkerData _thisWorkerData;
    public Camera camera;
    private BuildingData _buidlingData;
    
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
    public AnimationCurve takeoffCurve;
    [SerializeField] private List<int> limitsDroneResources;
    
    [Header("GameEvents")]
    public GameEvent UpdateResourcesEvent;

    #endregion

    #region Инициализация

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
        isPlaceNow = true;
        IsLogisticsCycleActive = false;
        
        currentWalkingPoint.gameObject.SetActive(false);
        OutlinePOD.SetActive(false);
        OutlineRotate.SetActive(false);

        unitType = _thisWorkerData.unitType;

        agent.enabled = false; 
        _rb.useGravity = false;
    }

    #endregion

    #region Общий контроль движения ( реализация IWorkerUnit )

    void Update()
    {
        if (isFlyNow && (isSelected || IsLogisticsCycleActive))
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
        if (Physics.Raycast(ray, out hit, 10000f, placementLayerMask, QueryTriggerInteraction.Ignore) && isSelected)
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
                IsLogisticsCycleActive = false;
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
        if (isMovingToLandingSpot || isLanding) return;
        
        
        if ((isSelected || IsLogisticsCycleActive) && WorkersInterBuildingControl.possiilityControlEntities && isFlyNow)
        {
            if ((Input.GetMouseButtonDown(0) && !isSelecting) || IsLogisticsCycleActive)
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
        Debug.Log($"Состояние дрона: isFlyNow={isFlyNow}, isLanding={isLanding}, isPlaceNow={isPlaceNow}, isTakingOff={isTakingOff}");
        if (!isTakingOff && !isFlyNow && !isLanding && isPlaceNow)
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

        anim.SetBool(droneFly_AK, true);
        
        yield return new WaitForSeconds(awaitBeforeFly);
        
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, droneFlyHeight, startPosition.z);
        
        float duration = 5f; // Общее время взлета
        float elapsedTime = 0f;
        
        while (Vector3.Distance(transform.position, targetPosition) >= 3f)
        {
            float t = takeoffCurve.Evaluate(elapsedTime / duration);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t );
            
            elapsedTime += Time.deltaTime;
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
        Debug.Log($"Состояние дрона: isFlyNow={isFlyNow}, isLanding={isLanding}, isPlaceNow={isPlaceNow}, isTakingOff={isTakingOff}");
        if (!isLanding && isFlyNow && !isTakingOff && !IsLogisticsCycleActive)
        {
            
            Vector3 landingPos = FindNearestLandingPosition();

            // Если точка не найдена, отмена
            if (landingPos == Vector3.zero)
            {
                Debug.LogError("Нет валидной точки посадки!");
                return;
            }

            // Если точка не под дроном, летим к ней
            Vector3 projectedPos = new Vector3(
                transform.position.x,
                landingPos.y,
                transform.position.z
            );

            if (Vector3.Distance(projectedPos, landingPos) > 1f)
            {
                StartCoroutine(MoveToLandingSpot(landingPos));
            }
            else
            {
                // Если точка под дроном, садимся сразу
                StartCoroutine(LandingCoroutine(landingPos));
            }
        }
    }

    private IEnumerator LandingCoroutine(Vector3 targetGroundPosition)
    {
        isLanding = true;
        agent.enabled = false;

        // Плавное снижение
        while (Vector3.Distance(transform.position, targetGroundPosition) >= 5f)
        {
            Debug.Log($"Процесс посадки");
            transform.position = Vector3.Lerp(
                transform.position,
                targetGroundPosition,
                Time.deltaTime * downSpeed
            );
            yield return null;
        }

        yield return new WaitForSeconds(awaitBeforePlace);
        
        anim.SetBool(droneFly_AK, false);
        
        //agent.enabled = true;
        //agent.Warp(targetGroundPosition); // Синхронизация с NavMesh
        isLanding = false;
        isFlyNow = false;
        isPlaceNow = true; // Убедитесь, что флаг посадки установлен
    }

    private Vector3 FindNearestLandingPosition()
    {
        int groundLayer = LayerMask.GetMask("Ground");
        int walkableArea = 1 << NavMesh.GetAreaFromName("Walkable");
        float searchRadius = 1000f; // Радиус поиска точки посадки

        // Ищем ближайшую точку на NavMesh в радиусе
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, searchRadius, walkableArea))
        {
            // Проверяем, что точка находится на слое Ground
            if (Physics.Raycast(hit.position + Vector3.up * 5, Vector3.down, out RaycastHit groundHit, searchRadius, groundLayer))
            {
                return groundHit.point;
            }
        }
        return transform.position;
    }

    private IEnumerator MoveToLandingSpot(Vector3 targetGroundPosition)
    {
        Debug.Log($"Процесс полета к нужной точке перед посадкой");
        // Вычисляем позицию над точкой посадки (на высоте полета)
        Vector3 targetAirPosition = new Vector3(
            targetGroundPosition.x,
            droneFlyHeight,
            targetGroundPosition.z
        );

        // Включаем режим перемещения к точке
        isMovingToLandingSpot = true;

        // Летим к позиции над целью
        agent.enabled = true;
        agent.SetDestination(targetAirPosition);

        // Ждем, пока дрон долетит
        while (Vector3.Distance(transform.position, targetAirPosition) > 0.5f)
        {
            yield return null;
        }

        // Начинаем посадку
        isMovingToLandingSpot = false;
        StartCoroutine(LandingCoroutine(targetGroundPosition));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 boxPosition = transform.position + Vector3.up * 2;
        Gizmos.DrawWireCube(boxPosition, landingBoxSize);
    }

    #endregion

    #region Логистика ( реализация IUnitLogistics )

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ToTheBase"></param>
    public void LogisticsCycleToggle(bool ToTheBase, BuildingData buildingTransform)
    {
       
    }

    #endregion
    
}
