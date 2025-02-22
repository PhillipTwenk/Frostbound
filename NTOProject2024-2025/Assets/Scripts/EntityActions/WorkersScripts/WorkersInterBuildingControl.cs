using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;

public class WorkersInterBuildingControl : MonoBehaviour
{
    public static WorkersInterBuildingControl Instance { get; set;}
    
    [Header("Texts in building hint")]
    [TextArea] [SerializeField] private string HintAwaitArriveWorker;
    [TextArea] [SerializeField] private string HintAwaitBuilding;

    [Header("Control workers & players")]
    public int CurrentValueOfWorkers; // Общее текущее количество рабочих
    public int MaxValueOfWorkers; // Максимальное количество рабочих при параметрах потребления еды
    public int NumberOfFreeWorkers; // количество рабочих, участвующий на данный момент в постройке здания или на работе в пасеке
    
    [Header("Selected entity")]
    public static WorkerMovementController SelectedWorker;
    public static PlayerMovementController SelectedPlayer;
    private WorkerMovementController thisWorker;
    private PlayerMovementController thisPlayer;
    
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
        NumberOfFreeWorkers = 1;
        thisWorker = null;
        firstMouseEnterOutlineIndicator = true;
    }

    private void Update()
    {
        // Каждый кадр проверяем: если нажата левая кнопка, то пытаемся выделить (OnClick),
        // иначе просто обновляем наведение (OnClick == false)
        if (!Input.GetMouseButtonDown(0))
        {
            MouseDownOnWorker(false); // Наведение
        }
        else
        {
            MouseDownOnWorker(true); // Клик
        }
    }

    public void MouseDownOnWorker(bool OnClick)
    {
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10000f, workerLayerMask) && Time.timeScale == 1f && possiilityControlEntities)
        {
            // Если попали в рабочего
            if (hit.collider.CompareTag("ClickOnWorker"))
            {
                thisWorker = hit.collider.GetComponent<WorkerMovementController>();
                thisPlayer = null; // Сбрасываем игрока

                if (OnClick)
                {
                    // Если до этого был выбран игрок, сбрасываем его выбор
                    if (SelectedPlayer != null)
                    {
                        SelectedPlayer.isSelected = false;
                        SelectedPlayer.isSelecting = false;
                        SelectedPlayer.OutlineRotate.SetActive(false);
                        SelectedPlayer = null;
                    }
                    
                    // Можно убрать проверку isSelecting, если клик уже гарантирует попадание
                    if (possiilityControlEntities)
                    {
                        Debug.Log("Нажали на рабочего");
                        if (!thisWorker.isSelected)
                        {
                            // Если был выбран другой рабочий, сбрасываем его выбор
                            if (SelectedWorker != null)
                            {
                                SelectedWorker.isSelected = false;
                                SelectedWorker.isSelecting = false;
                                SelectedWorker.OutlineRotate.SetActive(false);
                            }
                            thisWorker.OutlineRotate.SetActive(true);
                            thisWorker.isSelected = true;
                            SelectedWorker = thisWorker;
                        }
                        else
                        {
                            firstMouseEnterOutlineIndicator = false;
                            thisWorker.OutlineRotate.SetActive(false);
                            thisWorker.isSelected = false;
                            SelectedWorker = null;
                        }
                    }
                }
                else // Наведение без клика
                {
                    thisWorker.isSelecting = true;
                    if (!thisWorker.isSelected && possiilityControlEntities && firstMouseEnterOutlineIndicator)
                    {
                        thisWorker.OutlineRotate.SetActive(true);
                    }
                }
                return; // Если попали в рабочего – выходим
            }
            // Если попали в игрока
            else if (hit.collider.CompareTag("Player"))
            {
                thisPlayer = hit.collider.GetComponent<PlayerMovementController>();
                thisWorker = null; // Сбрасываем рабочего

                if (OnClick)
                {
                    // Сбрасываем выбор рабочего, если он был выбран
                    if (SelectedWorker != null)
                    {
                        SelectedWorker.isSelected = false;
                        SelectedWorker.isSelecting = false;
                        SelectedWorker.OutlineRotate.SetActive(false);
                        SelectedWorker = null;
                    }

                    
                    if (possiilityControlEntities)
                    {
                        Debug.Log("Нажали на игрока");
                        if (!thisPlayer.isSelected)
                        {
                            // Если уже выбран другой игрок, сбрасываем его
                            if (SelectedPlayer != null)
                            {
                                SelectedPlayer.isSelected = false;
                                SelectedPlayer.isSelecting = false;
                                SelectedPlayer.OutlineRotate.SetActive(false);
                            }
                            thisPlayer.OutlineRotate.SetActive(true);
                            thisPlayer.isSelected = true;
                            SelectedPlayer = thisPlayer;
                        }
                        else
                        {
                            firstMouseEnterOutlineIndicator = false;
                            thisPlayer.OutlineRotate.SetActive(false);
                            thisPlayer.isSelected = false;
                            SelectedPlayer = null;
                        }
                    }
                }
                else // Наведение без клика
                {
                    thisPlayer.isSelecting = true;
                    if (!thisPlayer.isSelected && possiilityControlEntities && firstMouseEnterOutlineIndicator)
                    {
                        thisPlayer.OutlineRotate.SetActive(true);
                    }
                }
                return; // Выходим, если попали в игрока
            }
        }

        // Если луч не попал ни в одного из объектов,
        // сбрасываем состояния для обоих (рабочего и игрока), чтобы убрать выделение при уходе курсора.
        if (thisWorker != null)
        {
            thisWorker.isSelecting = false;
            if (!thisWorker.isSelected && thisWorker.possibilityClickOnWorker)
            {
                firstMouseEnterOutlineIndicator = true;
                thisWorker.OutlineRotate.SetActive(false);
            }
            thisWorker = null;
        }
        if (thisPlayer != null)
        {
            firstMouseEnterOutlineIndicator = true;
            thisPlayer.isSelecting = false;
            if (!thisPlayer.isSelected && thisPlayer.possibilityClickOnPlayer)
            {
                thisPlayer.OutlineRotate.SetActive(false);
            }
            thisPlayer = null;
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
            MaxValueOfWorkers += newBuilding.MaxValueOfWorkersInThisBuilding;
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

            // SendWorkerToBuildingAnimationControl(buildingTransform);
            
            //Ожидаем прибытия рабочего
            await WaitForWorkerArrival();
            
        }else if(!IsSend) // Отправка рабочего обратно на базу
        {
            //NumberOfFreeWorkers -= 1;
            //CurrentValueOfWorkers += 1;
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
        buildingData.TextPanelBuildingControl(true, HintAwaitBuilding);

        await AwaitEndWorking(buildingData);

        //Debug.Log("Рабочий достроил, идет обратно");
        
        buildingData.TextPanelBuildingControl(false, HintAwaitBuilding);
    }

    private async Task AwaitEndWorking(BuildingData buildingData)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        Utility.Invoke(this, () => taskCompletionSource.SetResult(true), buildingData.buildingTypeSO.TimeAwaitBuildingThis);

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
        movementController.gameObject.SetActive(true);
        
        NumberOfFreeWorkers += 1;
        Debug.Log($"<color=green>Свободные рабочие + 1: {NumberOfFreeWorkers}</color>");
        return;
    }
    
    ///<summary> 
    /// Открытие / закрытие панели с подсказкой
    ///</summary>
    // private void ShowHint(string message)
    // {
    //     TextHintGameObject.SetActive(true);
    //     Utility.Invoke(this, () => TextHintGameObject.SetActive(false), TimeHintActive);
    //     HintTextTMPro.text = message;
    // }
}
