using System;
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
    public bool IsClickOnOtherEntity;
    public bool isFlyNow;
    public bool isPlaceNow;
    public bool isTakingOff; 
    public bool isLanding;
    public bool isMovingToLandingSpot;
    
    [Header("Visual")]
    public GameObject outlineRotate;
    public GameObject outlinePOD;

    [Header("Animations")]
    public string droneFly_AK;
    [NonSerialized] public Animator anim;
    
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
        isSelecting = false;
        IsClickOnOtherEntity = false;
        PossibilityClickOnUnit = true;
        
        currentWalkingPoint.gameObject.SetActive(false);
        OutlinePOD.SetActive(false);

        unitType = _thisWorkerData.unitType;
        
        _rb.useGravity = false;
    }

    #endregion

    #region Общий контроль движения ( реализация IWorkerUnit )

    void Update()
    {
        if (isFlyNow)
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
                isSelected = false;
                SelectedBuilding = null;
            }
            else
            {
                IsClickOnOtherEntity = false;
                IsLogisticsCycleActive = false;
                Debug.Log($"IsLogisticsCycleActive={IsLogisticsCycleActive}");
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
        
        
        if (isSelected && GeneralWorkersControl.possiilityControlEntities && isFlyNow)
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
            
            StartCoroutine(LandingCoroutine(landingPos));
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
    
    public bool CheckForNonGroundObjects()
    {
        // Создаём маску для слоёв, которые нужно проверять
        int forbiddenLayers = LayerMask.GetMask("Env", "Building", "LayerClickOnBuilding", "LayerClickOnPlayer", "LayerClickOnWorker");

        // Получаем все коллайдеры в области landingBoxSize на указанных слоях
        Collider[] colliders = Physics.OverlapBox(transform.position, landingBoxSize / 2, Quaternion.identity, forbiddenLayers);

        // Исключаем из проверки сам объект и его дочерние объекты
        foreach (var collider in colliders)
        {
            // Проверяем, что коллайдер не принадлежит текущему объекту или его дочерним объектам
            if (!collider.transform.IsChildOf(transform))
            {
                // Если найден объект на запрещённом слое, возвращаем true
                return true;
            }
        }

        // Если запрещённых объектов не найдено, возвращаем false
        return false;
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
    public void LogisticsCycleMovementHandler()
    {
        if (IsLogisticsCycleActive && SelectedBuilding != null)
        {
            currentWalkingPoint.transform.position 
                = new Vector3(SelectedBuilding.transform.position.x, 
                    SelectedBuilding.transform.position.y + droneFlyHeight, 
                    SelectedBuilding.transform.position.z);
            
            SetUnitDestination(currentWalkingPoint.transform, false);
        }
    }

    #endregion
    
}
