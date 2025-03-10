using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using EntityActions.Movement_Control;

public class WorkersInterBuildingControl : MonoBehaviour
{
    public static WorkersInterBuildingControl Instance { get; private set;}
    
    [Header("Texts in building hint")]
    [TextArea] [SerializeField] public string HintAwaitArriveWorker;
    [TextArea] [SerializeField] public string HintAwaitBuilding;
    [TextArea] [SerializeField] public string HintAwaitTimeWorker;
    [TextArea] public string HintNotNeededWorkerType; 
    [TextArea] public string HintNoBeAbleToBuildWorker;
    [TextArea] public string FullWorkerInThisBuilding;
 
    [Header("Control workers & players")]
    public int CurrentValueOfWorkers; // Общее текущее количество рабочих
    public int MaxValueOfWorkers; // Максимальное количество рабочих при параметрах потребления еды
    public int NumberOfFreeWorkers; // количество рабочих, участвующий на данный момент в постройке здания или на работе в пасеке
    
    [Header("Selected entity")]
    // public static WorkerMovementController SelectedWorker;
    // public static PlayerMovementController SelectedPlayer;
    // private WorkerMovementController thisWorker;
    // private PlayerMovementController thisPlayer;
    public static IUnitMovement SelectedUnit;
    private IUnitMovement SelectingUnit;

    
    [Header("Flags")]
    private bool IsWorkersHere;
    private bool firstMouseEnterOutlineIndicator; // Если нажали на рабочего/игрока для снятия с него выделения, то выделение при наведении будет работать только при повторном выделении
    public static bool possiilityControlEntities;
    

    [Header("Control building")]
    public List<ThisBuildingWorkersControl> listOfActiveBuildingWithWorkers;
    public static BuildingData CurrentBuilding;
    
    [Header("Camera")]
    public Camera mainCamera;
    public static Camera MainCamera;
    
    [Header("Layer masks")]
    [SerializeField] private LayerMask workerLayerMask;
    
    public event Action IsWorkerHereEvent; // Игрок прибыл


    private void Awake()
    {
        Instance = this;
        possiilityControlEntities = true;
        MainCamera = mainCamera;
        CurrentBuilding = null;
        // thisWorker = null;
        firstMouseEnterOutlineIndicator = true;
        SelectedUnit = null;
        SelectingUnit = null;
    }

    private void Update()
    {
        // Каждый кадр проверяем: если нажата левая кнопка, то пытаемся выделить (OnClick),
        // иначе просто обновляем наведение (OnClick == false)
        if (!Input.GetMouseButtonDown(0))
        {
            MouseHoverOnUnit(); // Наведение
        }
        else
        {
            MouseClickOnUnit(); // Клик
        }
    }

    public void MouseClickOnUnit()
    {
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10000f, workerLayerMask) && Time.timeScale > 0f && possiilityControlEntities)
        {
            if (hit.collider.CompareTag("ClickOnWorker") || hit.collider.CompareTag("Player"))
            {
                IUnitMovement selectedUnit = hit.collider.GetComponent<IUnitMovement>();
                if (selectedUnit == SelectedUnit)
                {
                    ResetSelectedUnit();
                    return;
                }
                ResetSelectedUnit();
                
                Debug.Log($"<color=purple> Выделен юнит: {hit.collider.tag} </color>");

                SelectedUnit = selectedUnit;
                SelectedUnit.isSelected = true;
                SelectedUnit.OutlineRotate.SetActive(true);

                UIManager.CancelLastOpenPanelEvent += ResetSelectedUnit;
                return;
            }
        }
    }

    
    public void MouseHoverOnUnit()
    {
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10000f, workerLayerMask) && Time.timeScale > 0f && possiilityControlEntities)
        {
            IUnitMovement hoveredUnit = hit.collider.GetComponent<IUnitMovement>();
            if (hoveredUnit != null && hoveredUnit != SelectedUnit)
            {
                Debug.Log($"<color=purple> навели курсор на юнита: {hit.collider.tag} </color>");
                hoveredUnit.OutlineRotate.SetActive(true);
                hoveredUnit.isSelecting = true;
                SelectingUnit = hoveredUnit;
            }
        }
        else
        {
            if (SelectingUnit != null)
            {
                Debug.Log($"<color=purple> убрали курсор с юнита </color>");
                if (SelectedUnit != SelectingUnit)
                {
                    SelectingUnit.OutlineRotate.SetActive(false);
                }
                SelectingUnit.isSelecting = false;
                SelectingUnit = null;
            }
        }
    }

    public void ResetSelectedUnit()
    {
        if (SelectedUnit != null)
        {
            Debug.Log($"<color=yellow> Снято выделение с выбранного юнита </color>");
            SelectedUnit.isSelected = false;
            SelectedUnit.isSelecting = false;
            SelectedUnit.OutlineRotate.SetActive(false);
            SelectedUnit = null;
            UIManager.CancelLastOpenPanelEvent -= ResetSelectedUnit;
        }
    }


    /// <summary>
    /// Обновление общего количество рабочих при постройке нового здания
    /// </summary>
    /// <param name="newBuilding"></param>
    public void AddNewBuilding(ThisBuildingWorkersControl newBuilding)
    {
        if (newBuilding != null) 
        {
            listOfActiveBuildingWithWorkers.Add(newBuilding);
            // MaxValueOfWorkers += newBuilding.MaxValueOfWorkersInThisBuilding;
            CurrentValueOfWorkers += newBuilding.CurrentNumberWorkersInThisBuilding;
        }
        else
        {
            listOfActiveBuildingWithWorkers.Add(newBuilding);
        }
    }

    /// <summary>
    /// Обновление общего количество рабочих при разрушении здания
    /// </summary>
    /// <param name="newBuilding"></param>
    public void RemoveNewBuilding(ThisBuildingWorkersControl newBuilding)
    {
        if (newBuilding != null)
        {
            listOfActiveBuildingWithWorkers.Remove(newBuilding);
            MaxValueOfWorkers -= newBuilding.MaxValueOfWorkersInThisBuilding;
            CurrentValueOfWorkers -= newBuilding.CurrentNumberWorkersInThisBuilding;
        }
        else
        {
            listOfActiveBuildingWithWorkers.Remove(newBuilding);
        }
    }

    ///<summary> 
    /// Отправляет рабочих на строительство / возвращает их обратно
    ///</summary>
    public async Task SendWorkerToBuilding(bool IsSend, BuildingData buildingData)
    {
        if(IsSend) // Ержана дернули с кровати и отправили строить крымский мост
        {
            CurrentBuilding = buildingData;
            
            Debug.Log("Рабочий отправился строить здание, ожидаем его прибытия");

            buildingData.TextPanelBuildingControl(true, HintAwaitArriveWorker);
            
            
            //Ожидаем прибытия рабочего
            await WaitForWorkerArrival();
            
        }else if(!IsSend) // Отправка рабочего обратно на базу
        {
            CurrentBuilding = null;
        }else
        {
            //ShowHint(HintTextNotEnoughtWorkers);
            Debug.Log("Нет рабочих");
        }
    }

    ///<summary> 
    /// Ожидание прибытия рабочего
    ///</summary>
    private async Task WaitForWorkerArrival()
    {
        // Создаем задачу, которая завершится при вызове события
        var taskCompletionSource = new TaskCompletionSource<bool>();

        void OnWorkerHere()
        {
            IsWorkerHereEvent -= OnWorkerHere;
            taskCompletionSource.SetResult(true);
        }

        IsWorkerHereEvent += OnWorkerHere;

        // Ждем завершения задачи
        await taskCompletionSource.Task;
    }

    ///<summary> 
    /// Вызывается из триггера здания, когда рабочий добежал до здания
    ///</summary>
    public void NotifyWorkerArrival()
    {
        IsWorkersHere = true;
        IsWorkerHereEvent?.Invoke();
    }


    ///<summary> 
    /// Ожидание завершения строительства
    ///</summary>
    public async Task WorkerEndWork(BuildingData buildingData)
    {
        //buildingData.TextPanelBuildingControl(true, HintAwaitBuilding);

        await AwaitEndWorking(buildingData);

        //Debug.Log("Рабочий достроил, идет обратно");
        
        buildingData.TextPanelBuildingControl(false, "");
    }

    private async Task AwaitEndWorking(BuildingData buildingData)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        Utility.Invoke(this, () => taskCompletionSource.SetResult(true), buildingData.buildingTypeSO.TimeAwaitBuildingThis);
        
        for (float i = buildingData.buildingTypeSO.TimeAwaitBuildingThis; i > 0; )
        {
            string newTimeText = $"{HintAwaitBuilding}\n {i} {HintAwaitTimeWorker}";
            i--;
            buildingData.TextPanelBuildingControl(true, newTimeText);
            await Task.Delay(1000);
        }

        await taskCompletionSource.Task;
    }


    /// <summary>
    /// Находит свободного рабочего к постройке здания
    /// </summary>
    public void SendWorkerToBuildingAnimationControl(Transform building)
    {
        foreach (var buildingControl in listOfActiveBuildingWithWorkers)
        {
            if (buildingControl != null)
            {
                if (buildingControl.CurrentNumberWorkersInThisBuilding > 0)
                {
                    //buildingControl.NumberOfActiveWorkersInThisBuilding += 1;
                    buildingControl.CurrentNumberWorkersInThisBuilding -= 1;
                    
                    Transform buildingSpawnWorkerPointTransform = buildingControl.buildingSpawnWorkerPointTransform;

                    GameObject newWorker = Instantiate(buildingControl.WorkerPrefab);
                    newWorker.transform.position = buildingSpawnWorkerPointTransform.position;
               
                    WorkerMovementController workerMovementController =
                        newWorker.GetComponent<WorkerMovementController>();
                    Animator animator = newWorker.GetComponent<Animator>();
                    buildingControl.StartMovementWorkerToBuilding(false, building, workerMovementController, animator);

                    return;
                }
            }
        }
    }

    /// <summary>
    /// Начинает анимацию строительства
    /// </summary>
    public async void StartAnimationBuilding(WorkerMovementController movementController, BuildingData buildingData, Transform spawnWorkerPosition)
    {
        movementController.ReadyForWork = false;
        
        NumberOfFreeWorkers -= 1;
        Debug.Log($"<color=green>Свободные рабочие - 1: {NumberOfFreeWorkers}</color>");
        
        await AwaitEndWorking(buildingData);
        
        buildingData.StartBuildingFunctionEvent?.Invoke();

        EndWorkingAnimationControl(movementController, spawnWorkerPosition);
    }

    public void EndWorkingAnimationControl(WorkerMovementController movementController, Transform spawnWorkerPosition)
    {
        movementController.transform.position = spawnWorkerPosition.position;
        movementController.ReadyForWork = true;
        movementController.SelectedBuilding = null;
        movementController.ArriveForBuildBuidling = false;
        movementController.isSelected = false;
        movementController.isSelecting = false;
        movementController.possibilityClickOnWorker = true;
        movementController.OutlineRotate.SetActive(false);
        movementController.OutlinePOD.SetActive(false);
        movementController.gameObject.SetActive(true);
        
        NumberOfFreeWorkers += 1;
        Debug.Log($"<color=green>Свободные рабочие + 1: {NumberOfFreeWorkers}</color>");
        return;
    }

    /// <summary>
    /// Снятие выделения с рабочего/Игрока и отписка от ивента ESC
    /// </summary>
    // public void ResetSelectedWorker()
    // {
    //     if (SelectedWorker != null)
    //     {
    //         Debug.Log($"<color=yellow> Снято выделение с выбранного рабочего </color>");
    //         firstMouseEnterOutlineIndicator = false;
    //         SelectedWorker.OutlineRotate.SetActive(false);
    //         SelectedWorker.isSelected = false;
    //         SelectedWorker = null;
    //         UIManager.CancelLastOpenPanelEvent -= ResetSelectedWorker;
    //     }
    // }
    // public void ResetSelectedPlayer()
    // {
    //     if (SelectedPlayer != null)
    //     {
    //         Debug.Log($"<color=yellow> Снято выделение с игрока </color>");
    //         firstMouseEnterOutlineIndicator = false;
    //         SelectedPlayer.OutlineRotate.SetActive(false);
    //         SelectedPlayer.isSelected = false;
    //         SelectedPlayer = null;
    //         UIManager.CancelLastOpenPanelEvent -= ResetSelectedPlayer;
    //     }
    // }

}
