using EntityActions.Movement_Control;
using EntityActions.WorkersScripts;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class PlayerMovementController : MonoBehaviour, IUnitMovement
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

    [Header("LayerMasks")]
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Flags")]
    private bool IsClickOnOtherEntity; // Кликнуи на рабочего
    
    [Header("System")]
    public Transform unitPointOfDestination;
    private EntityID playerID;
    private bool IsSceneLoaded;
    [SerializeField] private string NameOfTTS;
    public GameObject selectedBuilding;  // techTriggerScripts
    [SerializeField] private Transform currentWalkingPoint;
    private NavMeshAgent agent;
    public Camera camera;
    private Rigidbody _rb;
    private UnitType unitType;
    
    [Header("Visual")]
    public GameObject outlineRotate;
    public GameObject outlinePOD;
    private Animator anim;
    void Start()
    {
        PossibilityClickOnUnit = true;
        IsClickOnOtherEntity = false;
        currentWalkingPoint.gameObject.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        isSelected = false;
        isSelecting = false;
        anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>(); 
        OutlinePOD.SetActive(false);
        OutlineRotate.SetActive(false);

        unitType = UnitType.Player;
    }
    
    public void InitializePlayer()
    {
        playerID = CurrentPlayersDataControl.WhichPlayerCreate;
        IsSceneLoaded = true;
        Debug.Log("Персонаж готов");
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
                
                // Если клинкули не на здание и не на рабочего
                if(SelectedBuilding == null && !IsClickOnOtherEntity){
                    currentWalkingPoint.transform.position = new Vector3(point.x, point.y, point.z);
                } else {
                    currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
                }
                SetUnitDestination(currentWalkingPoint.transform, false);
            }
        }
        
        if (unitPointOfDestination) 
        {
            
            // Игрок идет до точки назначения
            anim.SetBool("Idle", false);
            anim.SetBool("Running", true);
            agent.isStopped = false;
            agent.destination = new Vector3(unitPointOfDestination.position.x, unitPointOfDestination.position.y, unitPointOfDestination.position.z);
            
        } 
        else 
        {
            // Игрок дошел до точки назначения
            agent.isStopped = true;
            anim.SetBool("Running", false);
            anim.SetBool("Idle", true);
            OutlinePOD.SetActive(false);
        }
    }
    
    /// <summary>
    /// Получение точки клика
    /// </summary>
    /// <returns></returns>
    public Vector3 GetSelectedMapPosition()
    {
        Vector3 lastPosition = Vector3.zero;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition); 
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10000f, Color.green, 5f);
        if (Physics.Raycast(ray, out hit, 10000f, placementLayerMask, QueryTriggerInteraction.Ignore))
        {
            lastPosition = hit.point; // Выбранная точка

            if (hit.collider.CompareTag("ClickOnBuilding"))
            {
                SelectedBuilding = hit.collider.gameObject.transform.parent.gameObject; // Выбранное здание
                Debug.Log($"Текущее здание этого юнита: {SelectedBuilding.GetComponent<BuildingData>().Title}");
                IsClickOnOtherEntity = false;
            }
            else if (hit.collider.CompareTag("ClickOnWorker"))
            {
                Debug.Log("Кликнули на рабочего");
                IsClickOnOtherEntity = true;
                SelectedBuilding = null;
            }
            else
            {
                SelectedBuilding = null;
                OutlinePOD.SetActive(true);
                IsClickOnOtherEntity = false;
            }
        }

        return lastPosition;
    }

    
    /// <summary>
    /// Установка пути
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
    
    /// <summary>
    /// Установка при достижении точки назначения
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "WalkingPoint"){
            Debug.Log("Рабочий дошел до точки назначения");
            currentWalkingPoint.gameObject.SetActive(false);
            unitPointOfDestination = null;
            anim.SetBool("Running", false);
            anim.SetBool("Idle", true);
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
}
