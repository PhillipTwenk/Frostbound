using System;
using System.Threading.Tasks;
using APIControl.Semaphore;
using Dialogues;
using EntityActions.WorkersScripts;
using UI;
using Unitilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CompletionOfConstructionController : MonoBehaviour
{
   [Header("Events")]
   [SerializeField] private GameEvent UpdateResourcesEvent;
   
   
   public event Action IsWorkerHereEvent; // Игрок прибыл
   [NonSerialized] public bool IsWorkersHere;
   
   private BuildingData _buildingData;
   private InteractionBuildingController _interactionBuildingController;

   [NonSerialized] public WorkerData currentWorker;
   
   [Header("Texts in building hint")]
   [TextArea] [SerializeField] public string HintAwaitArriveWorker;
   [TextArea] [SerializeField] public string HintAwaitBuilding;
   [TextArea] [SerializeField] public string HintAwaitTimeWorker;
   
   [FormerlySerializedAs("StartBuildingFunctionEvent")]
   [Header("Events of this building")] 
   [Tooltip("Что должно быть сделано при постройке здания")] public UnityEvent OnEndBuilding;
   [Tooltip("Что выполняется при размещении здания")] public UnityEvent OnPlacementBuilding;
   [Tooltip("Что выполняется при старте строительства здания")] public UnityEvent OnStartBuilding;
   [Tooltip("Что выполняется при уничтожении")] public UnityEvent OnDestroyBuilding;
   

   private void Start()
   {
      _buildingData = GetComponent<BuildingData>();
      _interactionBuildingController = GetComponent<InteractionBuildingController>();
   }

   /// <summary>
   /// Запуск окончания постройки
   /// Вызывается после того, как здание было размещено, но пока не построено
   /// </summary>
   public async Task StartCompletionOfConstruction(PlayerResources playerResources)
   {
      OnPlacementBuilding?.Invoke();
      
      await WaitForWorkerArrival();

      await AwaitEndWorking(_buildingData, playerResources);
   }
   
   ///<summary> 
   /// Ожидание прибытия рабочего
   ///</summary>
   private async Task WaitForWorkerArrival()
   {
      TextPanelBuildingControl(HintAwaitArriveWorker);
      _buildingData.IsThisBuilt = false;
      
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
   public void NotifyWorkerArrival(WorkerData workerData)
   {
      currentWorker = workerData;
      IsWorkersHere = true;
      IWorkerUnit movementController = currentWorker.gameObject.GetComponent<IWorkerUnit>();
      movementController.ReadyForWork = false;
      GeneralWorkersControl.Instance.NumberOfFreeUnits -= 1;
      
      OnStartBuilding?.Invoke();
      IsWorkerHereEvent?.Invoke();
   }
   
   ///<summary> 
   /// Ожидание завершения строительства
   ///</summary>
   private async Task AwaitEndWorking(BuildingData buildingData, PlayerResources playerResources)
   {
      var taskCompletionSource = new TaskCompletionSource<bool>();

      // Что будет сделано по завершению строительства
      Utility.Invoke( async () =>
      {
         Debug.Log("Активация прозрачного холста загрузки...");
         LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);

         Debug.Log("Активация рабочего...");
         ActivateWorker(); // Активируем рабочего

         Debug.Log("Начало оплаты стоимости здания металлом...");
         await PaymentForConstruction(playerResources, buildingData); // Оплачиваем стоимость здания металлом
         Debug.Log("Оплата завершена.");

         Debug.Log("Сохранение данных о новом здании...");
         await SaveNewBuildingData(); // Сохраняем данные о новом здании
         Debug.Log("Данные о здании успешно сохранены.");

         Debug.Log("Здание построено.");
         OnEndBuilding?.Invoke();
         Debug.Log("Событие OnEndBuilding вызвано.");

         Debug.Log("Вызов события BuildingPlaced в DialogueManager...");
         DialogueManager.OnBuildingPlaced?.Invoke(buildingData.buildingTypeSO, ActionTypeInteractWithObject.EndWorkingOnBuilding);
         Debug.Log("Событие BuildingPlaced вызвано.");

         Debug.Log("Скрытие текста ожидания постройки...");
         _buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(false);
         Debug.Log("Текст ожидания постройки скрыт.");
         _buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(false);
         foreach (var obj in _interactionBuildingController.objectsInTrigger)
         {
            if (obj.gameObject.CompareTag("Player"))
            {
               _buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(true);
               _interactionBuildingController.TextOnEvent?.Invoke();
            }
         }
         DescriptionPanelController.OnUpdateTextConditionsUpgradeBase?.Invoke();
         
         LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
         taskCompletionSource.SetResult(true);
         
      }, buildingData.buildingTypeSO.TimeAwaitBuildingThis);
        
      for (float i = buildingData.buildingTypeSO.TimeAwaitBuildingThis; i > 0; )
      {
         string newTimeText = $"{HintAwaitBuilding}\n {i} {HintAwaitTimeWorker}";
         i--;
         TextPanelBuildingControl(newTimeText);
         await Task.Delay(1000);
      }

      await taskCompletionSource.Task;
   }
   
   /// <summary>
   /// Активация рабочего около здания
   /// </summary>
   public void ActivateWorker()
   {
      IWorkerUnit movementController = currentWorker.gameObject.GetComponent<IWorkerUnit>();
      Transform spawnWorkerPosition = _interactionBuildingController.spawnWorker;

      if (currentWorker.unitType != UnitType.MainDrone)
      {
         currentWorker.transform.position = spawnWorkerPosition.position;
         movementController.SelectedBuilding = null;
         movementController.isSelected = false;
         movementController.isSelecting = false;
         movementController.OutlineRotate.SetActive(false);
         movementController.OutlinePOD.SetActive(false);
         currentWorker.gameObject.SetActive(true); 
      }
      movementController.ReadyForWork = true;
      movementController.ArriveForBuildBuidling = false;
      movementController.PossibilityClickOnUnit = true;
        
      GeneralWorkersControl.Instance.NumberOfFreeUnits += 1;
      Debug.Log($"<color=green>Свободные рабочие + 1: {GeneralWorkersControl.Instance.NumberOfFreeUnits}</color>");

      currentWorker = null;
   }

   /// <summary>
   /// Снятие ресурсов за стротельство здания
   /// </summary>
   public async Task PaymentForConstruction(PlayerResources playerResources, BuildingData buildingData)
   {
      int priceBuilding = buildingData.buildingTypeSO.priceBuilding;
      int EnergyConsumption = buildingData.HoneyConsumption;
      
      await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron - priceBuilding,
            playerResources.Energy - EnergyConsumption, playerResources.Food, playerResources.CryoCrystal);
      UpdateResourcesEvent.TriggerEvent();
   }
   
   /// <summary>
   /// Сохранение данных о новом здании
   /// </summary>
   public async Task SaveNewBuildingData()
   {
      GameObject newBuildingObject = _buildingData.transform.parent.gameObject;
      
      
      //Сохранение данных здания в SO сохранения
       PlayerSaveData pLayerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
       pLayerSaveData.playerBuildings.Add(_buildingData.buildingTypeSO.PrefabBuilding);

       TransformData transformData = new TransformData(newBuildingObject.transform);
       pLayerSaveData.buildingsTransform.Add(transformData);
       
       BuildingSaveData buildingSaveData = new BuildingSaveData(_buildingData);
       pLayerSaveData.BuildingDatas.Add(buildingSaveData);
       _buildingData.SaveListIndex = pLayerSaveData.BuildingDatas.IndexOf(buildingSaveData);
       pLayerSaveData.BuildingDatas[_buildingData.SaveListIndex].SaveListIndex = _buildingData.SaveListIndex;

       if (_buildingData.gameObject.GetComponent<ThisBuildingWorkersControl>())
       {
           ThisBuildingWorkersControl thisBuildingWorkersControl = _buildingData.gameObject.GetComponent<ThisBuildingWorkersControl>();
           WorkersControlSaveData worlersSaveData = new WorkersControlSaveData(thisBuildingWorkersControl);
           pLayerSaveData.BuildingWorkersInformationList.Add(worlersSaveData);

           GeneralWorkersControl.Instance.AddNewBuilding(thisBuildingWorkersControl);
       }
       else
       {
           ThisBuildingWorkersControl thisBuildingWorkersControl = null;
           
           pLayerSaveData.BuildingWorkersInformationList.Add(null);
           GeneralWorkersControl.Instance.AddNewBuilding(thisBuildingWorkersControl); 
       }
       
       _buildingData.IsThisBuilt = true;
       
       
       CurrentPlayersDataControl.currentBuildingsDatas.Add(_buildingData);
       
       await JSONSerializeManager.Instance.JSONSave();
   }
   
   
   /// <summary>
   /// Контроль текста над зданием
   /// </summary>
   /// <param name="IsOpen"> Появление/сокрытие текста </param>
   /// <param name="WhichAction"> Какой текст</param>
   public void TextPanelBuildingControl(string WhichText)
   {
         _buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(true);

         _buildingData.AwaitBuildingThisTMPro.text = WhichText;
   }
}
