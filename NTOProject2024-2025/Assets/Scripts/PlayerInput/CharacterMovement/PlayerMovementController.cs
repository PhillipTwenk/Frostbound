using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective WASDTutorial;
    private bool IsPlayerMove;
    
    [Header("LayerMasks")]
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Flags")]
    public bool isSelected;
    public bool isSelecting; // Мышь наведена на персонажа
    public bool possibilityClickOnPlayer;
    private bool IsClickOnOtherEntity; // Кликнуи на рабочего
    
    [Header("System")]
    public Transform PlayerPointOfDestination;
    private EntityID playerID;
    private bool IsSceneLoaded;
    [SerializeField] private string NameOfTTS;
    public GameObject SelectedBuilding; // techTriggerScripts
    [SerializeField] private Transform currentWalkingPoint;
    private NavMeshAgent agent;
    public Camera MainCamera;
    private Rigidbody _rb;
    
    [Header("Visual")]
    public GameObject OutlineRotate;
    public GameObject OutlinePOD;
    private Animator anim;
    void Start()
    {
        possibilityClickOnPlayer = true;
        IsClickOnOtherEntity = false;
        currentWalkingPoint.gameObject.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        isSelected = false;
        isSelecting = false;
        anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>(); 
        OutlinePOD.SetActive(false);
        OutlineRotate.SetActive(false);
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

    /// <summary>
    /// Управление движением
    /// </summary>
    void Update()
    {
        
        
        if(isSelected && WorkersInterBuildingControl.possiilityControlEntities){
            
            if (Input.GetMouseButtonDown(0) && !isSelecting)
            {
                Vector3 point = GetSelectedMapPosition();
                currentWalkingPoint.gameObject.SetActive(true);
                
                // Если клинкули не на здание и не на рабочего
                if(SelectedBuilding == null && !IsClickOnOtherEntity){
                    currentWalkingPoint.transform.position = new Vector3(point.x, point.y, point.z);
                    if (!IsPlayerMove)
                    {
                        IsPlayerMove = true;
                        Utility.Invoke(this, () => WASDTutorial.CheckAndUpdateTutorialState(), 2f);
                    }
                } else {
                    currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
                }
                SetWorkerDestination(currentWalkingPoint.transform, false);
            }
        }
        
        if (PlayerPointOfDestination) 
        {
            
            // Игрок идет до точки назначения
            anim.SetBool("Idle", false);
            anim.SetBool("Running", true);
            agent.isStopped = false;
            agent.destination = new Vector3(PlayerPointOfDestination.position.x, PlayerPointOfDestination.position.y, PlayerPointOfDestination.position.z);
            
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
                Debug.Log($"текущее здание для постройки {SelectedBuilding.GetComponent<BuildingData>().Title}");
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
    public void SetWorkerDestination(Transform point, bool isAutomatic){
        if(isAutomatic && SelectedBuilding != null){
            currentWalkingPoint.transform.position = SelectedBuilding.transform.parent.transform.Find("EndPointWalk").transform.position;
            PlayerPointOfDestination = currentWalkingPoint.transform;
            //Debug.Log($"Setting destination to: {currentWalkingPoint.transform.position}");
        } else {
            PlayerPointOfDestination = point;
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
    
    /// <summary>
    /// Установка при достижении точки назначения
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "WalkingPoint"){
            Debug.Log("Рабочий дошел до точки назначения");
            currentWalkingPoint.gameObject.SetActive(false);
            PlayerPointOfDestination = null;
            anim.SetBool("Running", false);
            anim.SetBool("Idle", true);
        } 
    }
}
