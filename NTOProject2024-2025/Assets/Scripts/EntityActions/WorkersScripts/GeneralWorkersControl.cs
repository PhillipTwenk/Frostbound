using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dialogues;
using EntityActions.Movement_Control;
using GlobalEvents.Cataclysm_Services;
using UI.UIManagers;
using Unitilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace EntityActions.WorkersScripts
{
    public class GeneralWorkersControl : MonoBehaviour
    {
        public static bool BlockMouseClickThisFrame { get; private set; }
        
        public static GeneralWorkersControl Instance { get; private set;}
    
        [Header("Texts in building hint")]
        [TextArea] public string HintNotNeededWorkerType; 
        [TextArea] public string HintNoBeAbleToBuildWorker;
        [TextArea] public string FullWorkerInThisBuilding;
        [TextArea] public string LimitRiskBeforeBuildingHint;

        [Header("Texts calling worker")]
        [TextArea] public string LimitUnitsErrorCallingWorkerText;
        [TextArea] public string LimitWorkersErrorCallingWorkerText;
        [TextArea] public string LimitFoodErrorCallingWorkerText;
 
        [Header("Control workers & players")]
        public int CurrentValueOfUnits; // Общее текущее количество рабочих
        public int MaxValueOfUnits; // Максимальное количество рабочих при параметрах потребления еды
        public int NumberOfFreeUnits; // количество рабочих, участвующий на данный момент в постройке здания или на работе в пасеке
        public int MaxValueOfWorkers;
        public int CurrentValueOfWorkers;
    
        [Header("Selected entity")]
        public static IUnitMovement SelectedUnit;
        private IUnitMovement SelectingUnit;
    
        [Header("Flags")]
        private bool IsWorkersHere;
        private bool firstMouseEnterOutlineIndicator; // Если нажали на рабочего/игрока для снятия с него выделения, то выделение при наведении будет работать только при повторном выделении
        public static bool possiilityControlEntities;
    
        [Header("Control building")]
        public List<ThisBuildingWorkersControl> listOfActiveBuildingWithWorkers;
    
        [Header("Camera")]
        public Camera mainCamera;
        public static Camera MainCamera;
    
        [Header("Layer masks")]
        [SerializeField] private LayerMask workerLayerMask;

        [Header("Drone")] 
        [SerializeField] private List<UnitType> DroneTypes;

        [Header("Food")] public static int CurrentFoodConsumptionByWorkers = 20;


        #region Инициализация

        private void Awake()
        {
            Instance = this;
            possiilityControlEntities = true;
            MainCamera = mainCamera;
            firstMouseEnterOutlineIndicator = true;
            SelectingUnit = null;
        }

        #endregion

        #region Методы общего контроля движения юнитов

        private void Update()
        {
            BlockMouseClickThisFrame = false;
            
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
                    BlockMouseClickThisFrame = true;
                    IUnitMovement selectedUnit = hit.collider.GetComponent<IUnitMovement>();
                    if (selectedUnit == SelectedUnit)
                    {
                        if (DroneTypes.Contains(SelectedUnit.ThisUnitType))
                        {
                            DroneMovementController droneMovementController = SelectedUnit as DroneMovementController;
                            if (!droneMovementController.IsDronesFullStopOperation)
                            {
                                if (droneMovementController != null && !droneMovementController.CheckForNonGroundObjects())
                                {
                                    droneMovementController.StartLanding(); // Начало посадки дрона
                                    ResetSelectedUnit();
                                    return;
                                }
                                
                                if(droneMovementController != null && droneMovementController.CheckForNonGroundObjects())
                                {
                                    return;
                                }
                            }
                            
                        }
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

                    DialogueManager.OnUnitMoved?.Invoke(ActionTypeMoveUnit.SelectUnit);
                    
                    selectedUnit.OnUnitSelected?.Invoke();
                    
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
                //Debug.Log($"<color=yellow> Снято выделение с выбранного юнита </color>");
                DialogueManager.OnUnitMoved?.Invoke(ActionTypeMoveUnit.DeselectUnit);
                SelectedUnit.isSelected = false;
                SelectedUnit.isSelecting = false;
                SelectedUnit.OutlineRotate.SetActive(false);
                SelectedUnit = null;
                UIManager.CancelLastOpenPanelEvent -= ResetSelectedUnit;
                
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
                // MaxValueOfUnits += newBuilding.MaxValueOfWorkersInThisBuilding;
                CurrentValueOfUnits += newBuilding.CurrentNumberWorkersInThisBuilding;
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
                MaxValueOfUnits -= newBuilding.MaxValueOfWorkersInThisBuilding;
                CurrentValueOfUnits -= newBuilding.CurrentNumberWorkersInThisBuilding;
            }
            else
            {
                listOfActiveBuildingWithWorkers.Remove(newBuilding);
            }
        }

        /// <summary>
        /// Проверяет, можно ли добавить указанное количество новых рабочих на базу
        /// </summary>
        /// <param name="numberOfNewWorkers"></param>
        /// <returns></returns>
        public async Task<string> CheckValidNumberOfWorkers(int numberOfNewWorkers)
        {
            EntityID entityID = CurrentPlayersDataControl.WhichPlayerCreate;
            PlayerResources playerResources = await APIManager.Instance.GetPlayerResources(entityID);
            
            if ((CurrentValueOfUnits + numberOfNewWorkers) <= MaxValueOfUnits)
            {
                if (CurrentValueOfWorkers + numberOfNewWorkers <= MaxValueOfWorkers)
                {
                    if (playerResources.Food - (numberOfNewWorkers * CurrentFoodConsumptionByWorkers) >= 0)
                    {
                        return String.Empty;
                    }
                    else
                    {
                        return LimitFoodErrorCallingWorkerText;
                    }
                }
                else
                {
                    return LimitWorkersErrorCallingWorkerText;
                }
            }
            else
            {
                return LimitUnitsErrorCallingWorkerText;
            }
        }

        #endregion

        #region Методы учета потребления еды

        /// <summary>
        /// Устанавливает потребление рабочих
        /// Вызывается если количество еды гарантированно не опустится ниже 0
        /// </summary>
        /// <param name="numberOfWorkers"> Количество рабочих </param>
        public async Task IncreasedFoodIntake(int numberOfWorkers)
        {
            EntityID entityID = CurrentPlayersDataControl.WhichPlayerCreate;
            PlayerResources playerResources = await APIManager.Instance.GetPlayerResources(entityID);
            int CurrentConsumptionFood = numberOfWorkers * CurrentFoodConsumptionByWorkers;
            playerResources.Food -= CurrentConsumptionFood;
            await APIManager.Instance.PutPlayerResources(entityID, playerResources.Iron, playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
        }

        #endregion
    }
}
