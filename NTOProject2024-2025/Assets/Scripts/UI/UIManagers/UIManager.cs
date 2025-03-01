using System;
using System.Collections.Generic;
using System.Linq;
using RTS_Cam;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective PlansPanelOpenTutorial;
    [SerializeField] private TutorialObjective ApiaryStartBuildingTutorial;
    [SerializeField] private TutorialObjective HomeStartBuildingTutorial;

    [Header("UI Control Events")]
    public GameEvent OpenBuildingPanelEvent;
    public GameEvent CloseBuildingPanelEvent;
    public GameEvent StartPlacingBuildEvent;
    public GameEvent EndPlacingBuildEvent;
    public GameEvent OpenBarterMenuEvent;
    public GameEvent CloseBarterMenuEvent;
    // public GameEvent ClosePauseMenu;
    // public GameEvent CloseSettingsMenu;
    public GameEvent OpenQuestPanelEvent;
    public GameEvent CloseQuestPanelEvent;
    
    /// <summary>
    /// Если какая-либо панель открывается, она подписывается на этот делегат при нажатии кнопки esc ( Добавление метода, реализующего закрытие себя же )
    /// Если закрывается - отписывается
    /// </summary>
    public static event Action CancelLastOpenPanelEvent; 
    
    
    [Header("UI Objects")]
    [SerializeField] private GameObject Resources_Icons;

    [Header("Quests")] 
    [SerializeField] private Transform uiListForQuestTransform;
    private List<GameObject> _currentsUIQuestPanels;
    
    [FormerlySerializedAs("ExtremeCondImage")]
    [Header("Extreme")]
    [SerializeField] private GameObject extremeCondImage;
    public bool isExtremeActivated;
    private float timer;
    private Color tempColor;
    
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
    
    public static UIManager Instance { get; set; }

    private void InitializeData()
    {
        _volumeSliderMusic.Initialization();
        _volumeSliderEffects.Initialization();
        
        _screenResolutionControl.Initialization();
    }

    /// <summary>
    /// При старте нового квеста он отображается на панели квестов
    /// </summary>
    /// <param name="quest"> Ссылка на SO квеста </param>
    public void AddNewQuestItemInQuestPanel(Quest quest)
    {
        GameObject newQuestItemGameObject = Instantiate(quest.UIItemOnQuestPanel, uiListForQuestTransform);
        _currentsUIQuestPanels.Add(newQuestItemGameObject);
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
                Debug.Log($"{uiQuestPanel.name}");
                _currentsUIQuestPanels.Remove(uiQuestPanel);
                Destroy(uiQuestPanel);
                return;
            }
        }
    }
    private void OnEnable()
    {
        HTTPRequests.FailedRequestLimitExceededEvent += FailedRequestLimitExceededUI;
        QuestController.OnStartNewQuest += AddNewQuestItemInQuestPanel;
    }

    private void OnDisable()
    {
        HTTPRequests.FailedRequestLimitExceededEvent -= FailedRequestLimitExceededUI;
        QuestController.OnStartNewQuest -= AddNewQuestItemInQuestPanel;
        UnsubscribeAllCancelLastOpenPanelEvent();
    }

    public void Awake()
    {
        Instance = this;
        _currentsUIQuestPanels = new List<GameObject>();
    }
    private void Start()
    {
        IsOpenBuildingPanel = true;
        InitializeData();
    }

    /// <summary>
    /// Отписка всех методов от делегата CancelLastOpenPanelEvent при окончании игры 
    /// </summary>
    private static void UnsubscribeAllCancelLastOpenPanelEvent()
    {
        if (CancelLastOpenPanelEvent == null) return;

        foreach (Delegate d in CancelLastOpenPanelEvent.GetInvocationList())
        {
            CancelLastOpenPanelEvent -= (Action)d; 
        }
    }

    /// <summary>
    /// Добавление / удаления метода по закрытию панели квестов в делегат
    /// </summary>
    public void OpenQuestPanel()
    {
        OpenQuestPanelEvent.TriggerEvent();
        CancelLastOpenPanelEvent += CloseQuestPanel;
    }
    public void CloseQuestPanel()
    {
        CloseQuestPanelEvent.TriggerEvent();
        CancelLastOpenPanelEvent -= CloseQuestPanel;
    }
    
    
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
    
    /// <summary>
    /// Контроль панели строительства
    /// </summary>
    /// <param name="IsOpenBuildingPanel"></param>
    public void OpenBuildingPanel()
    {
        Debug.Log("Открыта панель строительства");
        RTS_Camera.possibilityZoomCamera = false;
        PlansPanelOpenTutorial.CheckAndUpdateTutorialState();
        OpenBuildingPanelEvent.TriggerEvent();
        IsOpenBuildingPanel = false;
        CancelLastOpenPanelEvent += CloseBuildingPanel;
    }
    public void CloseBuildingPanel()
    {
        Debug.Log("Закрыта панель строительства");
        RTS_Camera.possibilityZoomCamera = true;
        EndPlacingBuildEvent.TriggerEvent();
        CloseBuildingPanelEvent.TriggerEvent();
        Destroy(BuildingManager.Instance.MouseIndicator);
        IsOpenBuildingPanel = true;
        CancelLastOpenPanelEvent -= CloseBuildingPanel;
    }

    /// <summary>
    /// Получает информацию о том, какую панель закрыть при нажатии ESC
    /// </summary>
    private void ESCCloseLastOpenUIPanel()
    {
        var lastPanel = CancelLastOpenPanelEvent?.GetInvocationList().Last() as Action;
        lastPanel?.Invoke();
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            ESCCloseLastOpenUIPanel();
        }
        
        if (Input.GetButtonDown("OpenBuildingPanel") && Time.timeScale == 1f)
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
            if (timer >= 120f){
                timer = 120f;
            } else {
                tempColor = extremeCondImage.GetComponent<Image>().color;
                tempColor.a = timer/120f;
                extremeCondImage.GetComponent<Image>().color = new Color(tempColor.r, tempColor.g, tempColor.b, tempColor.a);
            }
        } else {
            timer = 0f;
            tempColor = extremeCondImage.GetComponent<Image>().color;
            tempColor.a = 0f;
            extremeCondImage.GetComponent<Image>().color = tempColor;
        }
    }

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
    /// Инициализация панели строительства
    /// </summary>
    public async void InitializeBuildingPanel()
    {
        string playerName = CurrentPlayersDataControl.WhichPlayerCreate.Name;
        string shopName = $"{playerName}'sShop";
        ShopResources shopResources = await APIManager.Instance.GetShopResources(CurrentPlayersDataControl.WhichPlayerCreate, shopName);

        if (shopResources.Apiary.IsPurchased)
            AddNewPlanInPanel(plansArray[0]);
        if (shopResources.HoneyGun.IsPurchased)
            AddNewPlanInPanel(plansArray[1]);
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
        StartPlacingBuildEvent.TriggerEvent();
        if (plan.buildingSO.PrefabBuilding.transform.GetChild(0).GetComponent<BuildingData>().Title == "Пасека")
        {
            ApiaryStartBuildingTutorial.CheckAndUpdateTutorialState();
        }
        if (plan.buildingSO.PrefabBuilding.transform.GetChild(0).GetComponent<BuildingData>().Title == "Жилой модуль")
        {
            HomeStartBuildingTutorial.CheckAndUpdateTutorialState();
        }
    }

    /// <summary>
    /// Октрытие меню бартера
    /// </summary>
    public void OpenBarterMenu()
    {
        OpenBarterMenuEvent.TriggerEvent();
    }
    
    /// <summary>
    /// Закрытие меню бартера
    /// </summary>
    public void CloseBarterMenu()
    {
        CloseBarterMenuEvent.TriggerEvent();
        RTS_Camera.possibilityZoomCamera = true;
        CancelLastOpenPanelEvent -= CloseBarterMenu;
    }
    
    public void FunctionStartExtremeConditions(){
        isExtremeActivated = true;
    }

    public void FunctionEndExtremeConditions(){
        isExtremeActivated = false;
    }
}
