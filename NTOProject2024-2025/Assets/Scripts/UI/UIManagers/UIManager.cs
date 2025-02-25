using System;
using System.Collections.Generic;
using RTS_Cam;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective PlansPanelOpenTutorial;
    [SerializeField] private TutorialObjective ApiaryStartBuildingTutorial;
    [SerializeField] private TutorialObjective HomeStartBuildingTutorial;

    public GameEvent OpenBuildingPanelEvent;
    public GameEvent CloseBuildingPanelEvent;
    public GameEvent StartPlacingBuildEvent;
    public GameEvent EndPlacingBuildEvent;
    public GameEvent OpenBarterMenuEvent;
    public GameEvent CloseBarterMenuEvent;
    public GameEvent OpenHiringWorkersPanelEvent;
    public GameEvent CloseHiringWorkersPanelEvent;
    public GameEvent CloseTabletMenuEvent;
    public static TabletSO currentTablet; 

    [SerializeField] private Transform NewPlanPosition;
    [SerializeField] private Transform ContentPanel;
    [SerializeField] private GameObject TabletPanel;
    [SerializeField] private TMP_Text TitleTabletPanel;
    [SerializeField] private TMP_Text DescriptionTabletPanel;
    [SerializeField] private GameObject ImageTabletPanel;
    [SerializeField] private GameObject Resources_Icons;
    [SerializeField] private GameObject ExtremeCondImage;
    
    [SerializeField] private TMP_Text failedRequestLimitExceededUITMP_Text;
    [TextArea] [SerializeField] private string failedRequestLimitExceededUIText;
    

    [SerializeField] private List<Plan> plansArray;
    
    [SerializeField] private VolumeSlider _volumeSliderMusic;
    [SerializeField] private VolumeSlider _volumeSliderEffects;
    
    [SerializeField] private ScreenResolutionControl _screenResolutionControl;

    private bool IsOpenBuildingPanel;
    private float timer;
    private Color tempColor;
    public bool IsExtremeActivated;
    
    public static UIManager Instance { get; set; }

    private void InitializeData()
    {
        _volumeSliderMusic.Initialization();
        _volumeSliderEffects.Initialization();
        
        _screenResolutionControl.Initialization();
    }

    private void OnEnable()
    {
        HTTPRequests.FailedRequestLimitExceededEvent += FailedRequestLimitExceededUI;
    }

    private void OnDisable()
    {
        HTTPRequests.FailedRequestLimitExceededEvent -= FailedRequestLimitExceededUI;
    }

    public void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        IsOpenBuildingPanel = true;
        InitializeData();
    }

    public void FailedRequestLimitExceededUI()
    {
        failedRequestLimitExceededUITMP_Text.transform.parent.gameObject.SetActive(true);
        failedRequestLimitExceededUITMP_Text.text = failedRequestLimitExceededUIText;

        Utility.Invoke(this, () => failedRequestLimitExceededUITMP_Text.transform.parent.gameObject.SetActive(false),
            8f);
    }
    
    public void CloseAndOpenBuildingPanel(bool IsOpenBuildingPanel)
    {
        if (IsOpenBuildingPanel)
        {
            Debug.Log("Открыта панель строительства");
            RTS_Camera.possibilityZoomCamera = false;
            PlansPanelOpenTutorial.CheckAndUpdateTutorialState();
            OpenBuildingPanelEvent.TriggerEvent();
            this.IsOpenBuildingPanel = false;
        }
        else
        {
            Debug.Log("Закрыта панель строительства");
            RTS_Camera.possibilityZoomCamera = true;
            EndPlacingBuildEvent.TriggerEvent();
            CloseBuildingPanelEvent.TriggerEvent();
            Destroy(BuildingManager.Instance.MouseIndicator);
            this.IsOpenBuildingPanel = true;
        }
    }
    private void Update()
    {
        if (Input.GetButtonDown("OpenBuildingPanel") && IsOpenBuildingPanel)
        {
            CloseAndOpenBuildingPanel(IsOpenBuildingPanel);
            return;
        }
        if (Input.GetButtonDown("OpenBuildingPanel") && !IsOpenBuildingPanel)
        {
            CloseAndOpenBuildingPanel(IsOpenBuildingPanel);
            return;
        }
        if (IsExtremeActivated) 
        {
            Debug.Log("SHEEEEESH");
            timer += Time.deltaTime;
            if (timer >= 120f){
                timer = 120f;
            } else {
                tempColor = ExtremeCondImage.GetComponent<Image>().color;
                tempColor.a = timer/120f;
                ExtremeCondImage.GetComponent<Image>().color = new Color(tempColor.r, tempColor.g, tempColor.b, tempColor.a);
            }
        } else {
            timer = 0f;
            tempColor = ExtremeCondImage.GetComponent<Image>().color;
            tempColor.a = 0f;
            ExtremeCondImage.GetComponent<Image>().color = tempColor;
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
        string playerName = UIManagerLocation.WhichPlayerCreate.Name;
        string shopName = $"{playerName}'sShop";
        ShopResources shopResources = await APIManager.Instance.GetShopResources(UIManagerLocation.WhichPlayerCreate, shopName);

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
    }
    
    /// <summary>
    /// Октрытие меню торговли медведями
    /// </summary>
    public void OpenHiringWorkersPanel()
    {
        OpenHiringWorkersPanelEvent.TriggerEvent();
    }
    
    /// <summary>
    /// Закрытие меню торговли медведями
    /// </summary>
    public void CloseHiringWorkersPanel()
    {
        CloseHiringWorkersPanelEvent.TriggerEvent();
    }
    
    /// <summary>
    /// ОТКРЫЛ ОКОШКО ДУШНО СТАЛО ЫЫЫЫ юный колонизатор 
    /// </summary>
    public void FunctionOpenTabletMenu()
    {
        Resources_Icons.SetActive(false);
        TitleTabletPanel.text = "Юный колонизатор\n" + $"#{currentTablet.tablet_id} - " + currentTablet.title;
        DescriptionTabletPanel.text = currentTablet.description;
        ImageTabletPanel.GetComponent<Image>().sprite = currentTablet.picture;

        TabletPanel.SetActive(true);       
    }

    /// <summary>
    /// закрываааааааает окошко юный колонизатор
    /// </summary>
    public void FunctionCloseTabletMenu()
    {
        Resources_Icons.SetActive(true);
        TabletPanel.SetActive(false);
    }

    public void FunctionStartExtremeConditions(){
        IsExtremeActivated = true;
    }

    public void FunctionEndExtremeConditions(){
        IsExtremeActivated = false;
    }
}
