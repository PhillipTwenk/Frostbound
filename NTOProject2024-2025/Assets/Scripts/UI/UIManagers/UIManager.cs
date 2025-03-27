using System;
using System.Collections.Generic;
using System.Linq;
using APIControl.Global_Server_Event;
using Dialogues;
using EntityActions.WorkersScripts;
using RTS_Cam;
using TMPro;
using Unitilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.UIManagers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        #region Делегаты 

        /// <summary>
        /// Если какая-либо панель открывается, она подписывается на этот делегат при нажатии кнопки esc ( Добавление метода, реализующего закрытие себя же )
        /// Если закрывается - отписывается
        /// </summary>
        public static event Action CancelLastOpenPanelEvent;

        /// <summary>
        /// Вызывается при подтвержднном вызове нового рабочего из космопорта
        /// </summary>
        public static event Action<int> CallingNewWorkerEvent;

        /// <summary>
        /// При появлении уведомления 
        /// </summary>
        public static Action NotificationServerEvent;

        #endregion
    
        #region Переменные / Свойства

        [Header("UI Control Events")]
        public GameEvent OpenBuildingPanelEvent;
        public GameEvent CloseBuildingPanelEvent;
        public GameEvent StartPlacingBuildEvent;
        public GameEvent EndPlacingBuildEvent;
        public GameEvent OpenBarterMenuEvent;
        public GameEvent CloseBarterMenuEvent;
        public GameEvent OpenQuestPanelEvent;
        public GameEvent CloseQuestPanelEvent;
        public GameEvent OpenCallingWorkerPanelEvent;
        public GameEvent CloseCallingWorkerPanelEvent;
        public GameEvent UIAudioEffectEvent;
        public GameEvent OpenGlobalServerEventsPanelEvent;
        public GameEvent CloseGlobalServerEventsPanelEvent;
        public GameEvent ShowNotificationsPanelEvent;
        public GameEvent HideNotificationsPanelEvent;
    
    
    
        [Header("UI Objects")]
        [SerializeField] private GameObject Resources_Icons;

        [Header("Quests")] 
        [SerializeField] private Transform uiListForQuestTransform;
        private List<GameObject> _currentsUIQuestPanels;
        [SerializeField] private List<GameObject> AllUIQuestPanels;
        [SerializeField] private TextMeshProUGUI NameOfObjective;
        [SerializeField] private TextMeshProUGUI DescriptionOfObjective;
        [SerializeField] private GameObject IsCompletedObjective;
        private GameObject SelectedItemObjectiveIdicator;
        private Objective selectedObjective;
    
        [FormerlySerializedAs("ExtremeCondImage")]
        [Header("Extreme")]
        [SerializeField] private GameObject extremeCondImage;
        [SerializeField] private GameObject extremeFadeImage;
        public bool isExtremeActivated;
        public bool InSafeZone;
        private float timer;
        private Color condColor;
        private Color fadeColor;
    
        [Header("Request Error UI")]
        [SerializeField] private TMP_Text failedRequestLimitExceededUITMP_Text;
        [TextArea] [SerializeField] private string failedRequestLimitExceededUIText;
    
        [Header("Plans")]
        [SerializeField] private List<Plan> plansArray;
        [SerializeField] private Transform NewPlanPosition;
        [SerializeField] private Transform ContentPanel;
    
        [Header("Settings initialization")]
        [SerializeField] private VolumeSlider _volumeSliderMusic;
        [SerializeField] private VolumeSlider _volumeSliderEffects;
        [SerializeField] private ScreenResolutionControl _screenResolutionControl;

        [Header("Flags")]
        private bool IsOpenBuildingPanel;

        [Header("Calling Worker Panel")] 
        public int CurrentConstNumberOfNewWorkersAfterCalling;
        public TextMeshProUGUI CallingWorkerText;

        [Header("Server Global Events")] 
        public int timeNotificationServerEventPanel;
        public Image panelGlobalEventImage;
        public TextMeshProUGUI panelGlobalEventName;
        public TextMeshProUGUI panelGlobalEventDescription;
        public List<string> allEventNames = new List<string>();
        public List<Sprite> allEventImages = new List<Sprite>();


        public bool PossibilityZoomCamera
        {
            set
            {
                RTS_Camera.possibilityZoomCamera = value;
            }
            get
            {
                return RTS_Camera.possibilityZoomCamera;
            }
        }

        #endregion

        #region Инициализация / Окончание

        private void InitializeData()
        {
            _volumeSliderMusic.Initialization();
            _volumeSliderEffects.Initialization();
        
            _screenResolutionControl.Initialization();

            QuestController.OnInitializationQuests += InitializationQuestPanel;
        }
    
        private void OnEnable()
        {
            HTTPRequests.FailedRequestLimitExceededEvent += FailedRequestLimitExceededUI;
            QuestController.OnStartNewQuest += AddNewQuestItemInQuestPanel;
            NotificationServerEvent += ShowNotificationPanel;
            GlobalServerEventsManager.OnPanelGlobalServerEventsOpened += CurrentGlobalEventPanelInitialize;
            GlobalServerEventsManager.ClearNotificationServerEvent += CurrentGlobalEventPanelClear;
        }

        private void OnDisable()
        {
            HTTPRequests.FailedRequestLimitExceededEvent -= FailedRequestLimitExceededUI;
            QuestController.OnStartNewQuest -= AddNewQuestItemInQuestPanel;
            QuestController.OnInitializationQuests -= InitializationQuestPanel;
            NotificationServerEvent -= ShowNotificationPanel;
            GlobalServerEventsManager.OnPanelGlobalServerEventsOpened -= CurrentGlobalEventPanelInitialize;
            GlobalServerEventsManager.ClearNotificationServerEvent -= CurrentGlobalEventPanelClear;
            UnsubscribeAllCancelLastOpenPanelEvent();
        }

        public void Awake()
        {
            Instance = this;
            _currentsUIQuestPanels = new List<GameObject>();
        }
        private void Start()
        {
            SelectedItemObjectiveIdicator = null;
            IsOpenBuildingPanel = true;
            InitializeData();
        }

        #endregion

        #region Контроль нажатых клавиш

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                ESCCloseLastOpenUIPanel();
            }
        
            if (Input.GetButtonDown("OpenBuildingPanel") && Mathf.Approximately(Time.timeScale, 1f))
            {
                if (IsOpenBuildingPanel)
                {
                    OpenBuildingPanel();
                }
                else
                {
                    CloseBuildingPanel();
                }
                return;
            }
            if (isExtremeActivated) 
            {
                Debug.Log("SHEEEEESH");
                timer += Time.deltaTime;
                if (timer >= 12f){
                    timer = 0f;
                } else {
                    condColor = extremeCondImage.GetComponent<Image>().color;
                    condColor.a = timer/12f;
                    fadeColor = extremeFadeImage.GetComponent<Image>().color;
                    fadeColor.a = timer/24f;
                    extremeCondImage.GetComponent<Image>().color = new Color(condColor.r, condColor.g, condColor.b, condColor.a);
                    extremeFadeImage.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeColor.a);
                }
            } else if (!isExtremeActivated) {
                timer -= Time.deltaTime;
                if (timer <= 0f){
                    condColor = extremeCondImage.GetComponent<Image>().color;
                    fadeColor = extremeFadeImage.GetComponent<Image>().color;
                    condColor.a = 0f;
                    fadeColor.a = 0f;
                    extremeCondImage.GetComponent<Image>().color = condColor;
                    extremeFadeImage.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeColor.a);
                } else {
                    condColor = extremeCondImage.GetComponent<Image>().color;
                    fadeColor = extremeFadeImage.GetComponent<Image>().color;
                    condColor.a = timer/1.5f;
                    fadeColor.a = timer/3f;
                    extremeCondImage.GetComponent<Image>().color = new Color(condColor.r, condColor.g, condColor.b, condColor.a);
                    extremeFadeImage.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeColor.a);
                }
            }
            if (InSafeZone){
                timer -= Time.deltaTime;
                if (timer <= 0f){
                    condColor = extremeCondImage.GetComponent<Image>().color;
                    fadeColor = extremeFadeImage.GetComponent<Image>().color;
                    condColor.a = 0f;
                    fadeColor.a = 0f;
                    extremeCondImage.GetComponent<Image>().color = condColor;
                    extremeFadeImage.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeColor.a);
                } else {
                    condColor = extremeCondImage.GetComponent<Image>().color;
                    fadeColor = extremeFadeImage.GetComponent<Image>().color;
                    condColor.a = timer/1.5f;
                    fadeColor.a = timer/3f;
                    extremeCondImage.GetComponent<Image>().color = new Color(condColor.r, condColor.g, condColor.b, condColor.a);
                    extremeFadeImage.GetComponent<Image>().color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeColor.a);
                }
            }
        }

        #endregion
    
        #region Контроль отмены последнего действия

        /// <summary>
        /// Отписка всех методов от делегата CancelLastOpenPanelEvent при окончании игры 
        /// </summary>
        private static void UnsubscribeAllCancelLastOpenPanelEvent()
        {
            if (CancelLastOpenPanelEvent == null) return;
        
            for (int i = 0; i < CancelLastOpenPanelEvent?.GetInvocationList().Length; i++)
            {
                Debug.Log($"Отменено действие под номером {i}");
                CancelLastOpenPanelEvent -= (Action)CancelLastOpenPanelEvent?.GetInvocationList()[i];
            }
        }
    
        /// <summary>
        /// Получает информацию о том, какую панель закрыть при нажатии ESC
        /// </summary>
        private void ESCCloseLastOpenUIPanel()
        {
            var lastPanel = CancelLastOpenPanelEvent?.GetInvocationList().Last() as Action;
            lastPanel?.Invoke();
            UIAudioEffectEvent.TriggerEvent();
            //CancelLastOpenPanelEvent -= lastPanel;
        }
        #endregion

        #region Панель с квестами

        /// <summary>
        /// При старте нового квеста он отображается на панели квестов
        /// </summary>
        /// <param name="quest"> Ссылка на SO квеста </param>
        public void AddNewQuestItemInQuestPanel(Quest quest)
        {
            foreach (var uiQuestPanel in AllUIQuestPanels)
            {
                if (uiQuestPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == quest.Name)
                {
                    Debug.Log($" Добавлен квест в панель: {uiQuestPanel.name}");
                    _currentsUIQuestPanels.Add(uiQuestPanel);
                    uiQuestPanel.SetActive(true);
                    return;
                }
            }
        }
    
        /// <summary>
        /// Загрузка данных о цели в панель подроного описания 
        /// </summary>
        /// <param name="objective"> SO цели </param>
        public void LoadDataInDescriptionUIPanel(Objective objective)
        {
            NameOfObjective.text = objective.name;
            DescriptionOfObjective.text = objective.description;
            IsCompletedObjective.SetActive(objective.completed);
        
            selectedObjective = null;
            selectedObjective = objective;
        }

        /// <summary>
        /// Обновить активных индикатор выделенной цели
        /// </summary>
        /// <param name="newIdicator"> Ссылка на gameObject индикатора </param>
        public void NewIndicator(GameObject newIdicator)
        {
            SelectedItemObjectiveIdicator?.SetActive(false);
            SelectedItemObjectiveIdicator = newIdicator;
            SelectedItemObjectiveIdicator?.SetActive(true);
        }

        /// <summary>
        /// Убрать завершенный квест из панели UI
        /// </summary>
        /// <param name="quest"></param>
        public void RemoveQuestItemInQuestPanel(Quest quest)
        {
            foreach (var uiQuestPanel in _currentsUIQuestPanels)
            {
                if (uiQuestPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == quest.Name)
                {
                    Debug.Log($" Завершен квест {uiQuestPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text}, убираем его из панели квестов ");
                    _currentsUIQuestPanels.Remove(uiQuestPanel);
                    uiQuestPanel.SetActive(false);
                    if (selectedObjective.parentQuest.Name == quest.Name)
                    {
                        DescriptionOfObjective.transform.parent.gameObject.SetActive(false);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Инициализация панели квестов
        /// </summary>
        /// <param name="quests"> Активные квесты </param>
        public void InitializationQuestPanel(List<Quest> quests)
        {
            Debug.Log("Начата инициализация панели квестов");
            List<string> listOfActiveQuests = new List<string>();
            for (int i = 0; i < quests.Count; i++)
            {
                listOfActiveQuests.Add(quests[i].Name);
            }
        
            for (int i = 0; i < AllUIQuestPanels.Count; i++)
            {
                string questName = AllUIQuestPanels[i].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
                if (listOfActiveQuests.Contains(questName))
                {
                    Debug.Log($"Инициализирован квест: {AllUIQuestPanels[i].name}");
                    _currentsUIQuestPanels.Add(AllUIQuestPanels[i]);
                    AllUIQuestPanels[i].SetActive(true);
                }
            }
        }
    
        /// <summary>
        /// Добавление / удаления метода по закрытию панели квестов в делегат
        /// </summary>
        public void OpenQuestPanel()
        {
            OpenQuestPanelEvent.TriggerEvent();
            if (selectedObjective != null)
            {
                IsCompletedObjective?.SetActive(selectedObjective.completed);
            }
            CancelLastOpenPanelEvent += CloseQuestPanel;
        }
        public void CloseQuestPanel()
        {
            Debug.Log($"<color=yellow> Закрыта панель квестов </color>");
            CloseQuestPanelEvent.TriggerEvent();
            CancelLastOpenPanelEvent -= CloseQuestPanel;
        }
        #endregion

        #region Панель строительства

        /// <summary>
        /// Контроль панели строительства
        /// </summary>
        /// <param name="IsOpenBuildingPanel"></param>
        public void OpenBuildingPanel()
        {
            Debug.Log("Открыта панель строительства");
            RTS_Camera.possibilityZoomCamera = false;
            //PlansPanelOpenTutorial.CheckAndUpdateTutorialState();
            OpenBuildingPanelEvent.TriggerEvent();
            IsOpenBuildingPanel = false;
            DialogueManager.OnPanelOpened?.Invoke(ActionTypeUIPanel.OpenBuildingPanel);
            CancelLastOpenPanelEvent += CloseBuildingPanel;
        }
        public void CloseBuildingPanel()
        {
            Debug.Log($"<color=yellow> Закрыта панель строительства </color>");
            RTS_Camera.possibilityZoomCamera = true;
            EndPlacingBuildEvent.TriggerEvent();
            CloseBuildingPanelEvent.TriggerEvent();
            Destroy(BuildingManager.Instance.MouseIndicator);
            IsOpenBuildingPanel = true;
            CancelLastOpenPanelEvent -= CloseBuildingPanel;
        }
    
        /// <summary>
        /// Инициализация панели строительства
        /// </summary>
        public async void InitializeBuildingPanel()
        {
            string playerName = CurrentPlayersDataControl.WhichPlayerCreate.entityName;
            string shopName = $"{playerName}'sShop";
            ShopResources shopResources = await APIManager.Instance.GetShopResources(CurrentPlayersDataControl.WhichPlayerCreate, shopName);

            if (shopResources.Apiary.IsPurchased)
                AddNewPlanInPanel(plansArray[0]);
            if (shopResources.MobileBase.IsPurchased)
                AddNewPlanInPanel(plansArray[2]);
            if (shopResources.Storage.IsPurchased)
                AddNewPlanInPanel(plansArray[3]);
            if (shopResources.ResidentialModule.IsPurchased)
                AddNewPlanInPanel(plansArray[4]);
            if (shopResources.Minner.IsPurchased)
                AddNewPlanInPanel(plansArray[5]);
            if (shopResources.Pier.IsPurchased)
                AddNewPlanInPanel(plansArray[6]);

            LoadingCanvasController.Instance.LoadingCanvasNotTransparent.SetActive(false);
        
        }

        /// <summary>
        /// Нажатие на кнопку старта строительства
        /// Начинаем размещать строение на земле
        /// </summary>
        public void StartPlacingNewBuilding(Plan plan)
        {
            if (BuildingManager.Instance.MouseIndicator != null)
            {
                Destroy(BuildingManager.Instance.MouseIndicator);
            }
            GameObject PlaceNewBuildingTrigger = Instantiate(plan.buildingSO.PrefabBeforeBuilding);
            BuildingManager.Instance.MouseIndicator = PlaceNewBuildingTrigger;
            BuildingManager.Instance.CurrentBuilding = plan.buildingSO.PrefabBuilding;
            DialogueManager.OnBuildingPlaced?.Invoke(plan.buildingSO, ActionTypeInteractWithObject.SelectPlan);
            StartPlacingBuildEvent.TriggerEvent();
        }
        #endregion

        #region Бартер

        /// <summary>
        /// Добавляет возможность строить новое здание после покупки нового чертежа
        /// </summary>
        public void AddNewPlanInPanel(Plan plan)
        {
            GameObject newPlanGameObject = Instantiate(plan.PlanPrefab, NewPlanPosition);

            newPlanGameObject.transform.SetParent(ContentPanel);
        
            TextMeshProUGUI titleTMPro = newPlanGameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descriptionTMPro = newPlanGameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            Image sprite = newPlanGameObject.transform.GetChild(2).GetComponent<Image>();
            TextMeshProUGUI durabilityTMPro = newPlanGameObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI energyHoneyConsumptionTMPro = newPlanGameObject.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI resourceProductionTMPro = newPlanGameObject.transform.GetChild(5).GetComponent<TextMeshProUGUI>();

            titleTMPro.text = plan.Title;
            descriptionTMPro.text = plan.Description;
            sprite.sprite = plan.planSprite;
            durabilityTMPro.text = $"- Прочность: {plan.durability}";
            energyHoneyConsumptionTMPro.text = $"- Потребляет: {plan.energyHoneyConsumption}";
            resourceProductionTMPro.text = $"- Производит: {plan.resourceProduction}";

            Button ButtonComponent = newPlanGameObject.GetComponent<Button>();
            ButtonComponent.onClick.AddListener(() => StartPlacingNewBuilding(plan));
        }
    
        /// <summary>
        /// Закрытие меню бартера
        /// </summary>
        public void CloseBarterMenu()
        {
            Debug.Log($"<color=yellow> Закрыта панель магазина </color>");
            CloseBarterMenuEvent.TriggerEvent();
            RTS_Camera.possibilityZoomCamera = true;
            DialogueManager.OnPanelOpened?.Invoke(ActionTypeUIPanel.CloseBarter);
            CancelLastOpenPanelEvent -= CloseBarterMenu;
        }

        #endregion

        #region Панель космопорта

        /// <summary>
        /// Открытие панели космопорта
        /// </summary>
        public void OpenCallingWorkersPanel()
        {
            OpenCallingWorkerPanelEvent.TriggerEvent();
            RTS_Camera.possibilityZoomCamera = false;
            DialogueManager.OnPanelOpened?.Invoke(ActionTypeUIPanel.OpenCallingWorkersPanel);
            CancelLastOpenPanelEvent += CloseCallingWorkersPanel;
        }

        /// <summary>
        /// Закрытие панели коспоморта
        /// </summary>
        public void CloseCallingWorkersPanel()
        {
            CloseCallingWorkerPanelEvent.TriggerEvent();
            RTS_Camera.possibilityZoomCamera = true;
            CancelLastOpenPanelEvent -= CloseCallingWorkersPanel;
        }

        /// <summary>
        /// Нажали на кнопку вызова рабочего
        /// </summary>
        public async void ClickButtonCallingWorker(int workerType)
        {
            string newText = await GeneralWorkersControl.Instance.CheckValidNumberOfWorkers(CurrentConstNumberOfNewWorkersAfterCalling);
            UpdateTextNearCallingWorkerPanel(newText);
        
            if (newText == String.Empty)
            {
                CallingNewWorkerEvent?.Invoke(workerType);
                if (workerType == 1)
                {
                    DialogueManager.OnWorkerCalled?.Invoke(ActionTypeCallWorker.CallBeekeeper);
                }else if (workerType == 2)
                {
                    DialogueManager.OnWorkerCalled?.Invoke(ActionTypeCallWorker.CallConstructor);
                }
            }
            else
            {
                if (workerType == 1)
                {
                    DialogueManager.OnWorkerCalled?.Invoke(ActionTypeCallWorker.UnsuccesefullCallBeekeeper);
                }
            }
        
        }

        /// <summary>
        /// Обновляет текст около панели вызова рабочих, если какое либо из условий не выполнилось
        /// </summary>
        /// <param name="text"></param>
        public void UpdateTextNearCallingWorkerPanel(string text) => CallingWorkerText.text = text;
    
    
        #endregion
    
        #region Экстремальные условия

        public void FunctionStartExtremeConditions(){
            if(!isExtremeActivated){
                InSafeZone = false;
                isExtremeActivated = true;
                timer = 0f;
            }
        }

        public void FunctionEndExtremeConditions(){
            isExtremeActivated = false;
            timer = 1.5f;
        }

        public void FunctionSafeZoneConditions(){
            isExtremeActivated = false;
            timer = 1.5f;
            InSafeZone = true;
        }

        #endregion

        #region Панель глобальных серверных ивентов

        /// <summary>
        /// Открытие панели уведомлений о серверных ивентах
        /// </summary>
        public void OpenGlobalServerEventsPanel()
        {
            GlobalServerEventsManager.OnPanelGlobalServerEventsOpened?.Invoke(GlobalServerEventsManager.currentServerEvent);
            OpenGlobalServerEventsPanelEvent.TriggerEvent();
            RTS_Camera.possibilityZoomCamera = false;
            CancelLastOpenPanelEvent += CloseGlobalServerEventsPanel;
        }

        /// <summary>
        /// Закрытие панели уведолений о серверных ивентах 
        /// </summary>
        public void CloseGlobalServerEventsPanel()
        {
            CloseGlobalServerEventsPanelEvent.TriggerEvent();
            RTS_Camera.possibilityZoomCamera = true;
            CancelLastOpenPanelEvent -= CloseGlobalServerEventsPanel;
        }
    
        /// <summary>
        /// Показывает панель уведомления на определенное время 
        /// </summary>
        public void ShowNotificationPanel()
        {
            ShowNotificationsPanelEvent.TriggerEvent();

            Utility.Invoke(this, () =>
            {
                HideNotificationsPanelEvent.TriggerEvent();
            }, timeNotificationServerEventPanel);

        }

        /// <summary>
        /// Инициаизация панели 
        /// </summary>
        public void CurrentGlobalEventPanelInitialize(ServerEvent serverEvent)
        {
            int indexImage = allEventNames.IndexOf(serverEvent.name);
            Sprite currenImage = allEventImages[indexImage];
            panelGlobalEventImage.sprite = currenImage;
            panelGlobalEventName.text = serverEvent.name;
            panelGlobalEventDescription.text = serverEvent.text;
        }

        /// <summary>
        /// Очищение панели 
        /// </summary>
        public void CurrentGlobalEventPanelClear()
        {
            panelGlobalEventImage.sprite = null;
            panelGlobalEventName.text = String.Empty;
            panelGlobalEventDescription.text = String.Empty;
        }

        #endregion
    
        #region Панель ошибок

        /// <summary>
        /// Вызов панели ошибки запросов и перевода в оффлайн режим
        /// </summary>
        public void FailedRequestLimitExceededUI()
        {
            failedRequestLimitExceededUITMP_Text.transform.parent.gameObject.SetActive(true);
            failedRequestLimitExceededUITMP_Text.text = failedRequestLimitExceededUIText;

            Utility.Invoke(this, () => failedRequestLimitExceededUITMP_Text.transform.parent.gameObject.SetActive(false),
                8f);
        }

        #endregion
    
    
    

   

   
    
    
    
    
    

    
    

    
    
   
    }
}
