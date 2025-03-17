using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using EntityActions.Movement_Control;
using EntityActions.WorkersScripts;

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

    [Header("Drone")] 
    [SerializeField] private List<UnitType> DroneTypes;
    
    public event Action IsWorkerHereEvent; // Игрок прибыл


    #region Инициализация

    private void Awake()
    {
        Instance = this;
        possiilityControlEntities = true;
        MainCamera = mainCamera;
        CurrentBuilding = null;
        // thisWorker = null;
        firstMouseEnterOutlineIndicator = true;
        SelectingUnit = null;
    }

    #endregion

    #region Методы общего контроля движения юнитов

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
                
                //Debug.Log($"<color=purple> Выделен юнит: {hit.collider.tag} </color>");

                SelectedUnit = selectedUnit;
                SelectedUnit.isSelected = true;
                SelectedUnit.OutlineRotate.SetActive(true);

                if (DroneTypes.Contains(SelectedUnit.ThisUnitType))
                {
                    DroneMovementController droneMovementController = SelectedUnit as DroneMovementController;
                    if (droneMovementController != null)
                    {
                        droneMovementController.StartTakeoff(); // Начало подъема дрона 
                    }
                }

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
                //Debug.Log($"<color=purple> навели курсор на юнита: {hit.collider.tag} </color>");
                hoveredUnit.OutlineRotate.SetActive(true);
                hoveredUnit.isSelecting = true;
                SelectingUnit = hoveredUnit;
            }
        }
        else
        {
            if (SelectingUnit != null)
            {
                //Debug.Log($"<color=purple> убрали курсор с юнита </color>");
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
            if (DroneTypes.Contains(SelectedUnit.ThisUnitType))
            {
                DroneMovementController droneMovementController = SelectedUnit as DroneMovementController;
                if (droneMovementController != null && !droneMovementController.CheckForNonGroundObjects())
                {
                    droneMovementController.StartLanding(); // Начало посадки дрона 
                    //Debug.Log($"<color=yellow> Снято выделение с выбранного юнита </color>");
                    SelectedUnit.isSelected = false;
                    SelectedUnit.isSelecting = false;
                    SelectedUnit.OutlineRotate.SetActive(false);
                    SelectedUnit = null;
                    UIManager.CancelLastOpenPanelEvent -= ResetSelectedUnit;
                }
            }
            else
            {
                //Debug.Log($"<color=yellow> Снято выделение с выбранного юнита </color>");
                SelectedUnit.isSelected = false;
                SelectedUnit.isSelecting = false;
                SelectedUnit.OutlineRotate.SetActive(false);
                SelectedUnit = null;
                UIManager.CancelLastOpenPanelEvent -= ResetSelectedUnit;
            }
        }
    }

    #endregion
    
    #region Методы учета жителей в здании

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

    #endregion

    #region Методы основного игрового цикла постройки

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
            
        }else // Отправка рабочего обратно на базу
        {
            CurrentBuilding = null;
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
        //buildingDataLogistics.TextPanelBuildingControl(true, HintAwaitBuilding);

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
    /// Начинает анимацию строительства
    /// </summary>
    public async void StartAnimationBuilding(IWorkerUnit movementController, BuildingData buildingData, Transform spawnWorkerPosition, WorkerData workerData)
    {
        movementController.ReadyForWork = false;
        
        NumberOfFreeWorkers -= 1;
        Debug.Log($"<color=green>Свободные рабочие - 1: {NumberOfFreeWorkers}</color>");
        
        await AwaitEndWorking(buildingData);
        
        buildingData.StartBuildingFunctionEvent?.Invoke();

        EndWorkingAnimationControl(movementController, spawnWorkerPosition, workerData);
    }

    public void EndWorkingAnimationControl(IWorkerUnit movementController, Transform spawnWorkerPosition, WorkerData workerData)
    {
        workerData.transform.position = spawnWorkerPosition.position;
        movementController.ReadyForWork = true;
        movementController.SelectedBuilding = null;
        movementController.ArriveForBuildBuidling = false;
        movementController.isSelected = false;
        movementController.isSelecting = false;
        movementController.PossibilityClickOnUnit = true;
        movementController.OutlineRotate.SetActive(false);
        movementController.OutlinePOD.SetActive(false);
        workerData.gameObject.SetActive(true);
        
        NumberOfFreeWorkers += 1;
        Debug.Log($"<color=green>Свободные рабочие + 1: {NumberOfFreeWorkers}</color>");
        return;
    }

    #endregion
}
