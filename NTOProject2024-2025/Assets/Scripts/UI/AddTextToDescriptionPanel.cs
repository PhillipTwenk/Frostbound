using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AddTextToDescriptionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private TextMeshProUGUI Level;
    [SerializeField] private TextMeshProUGUI Durability;
    [SerializeField] private TextMeshProUGUI Production;
    [SerializeField] private TextMeshProUGUI HoneyConsumption;
    [SerializeField] private TextMeshProUGUI Storage;

    [SerializeField] private TextMeshProUGUI HintPanel;
    [SerializeField] private TextMeshProUGUI ButtonUpgradeTextPanel;
    [TextArea] [SerializeField] private string TextNotEnoughtResources;
    [TextArea] [SerializeField] private string TextNotEnoughtBaseLevel;
    [TextArea] [SerializeField] private string UpgradeLevelBuildingInformation;
    [TextArea] [SerializeField] private string TextNotCompleteConditionUpgradeMB;
    [TextArea] [SerializeField] private string TextCompleteUpgradeMobileBaseLevel;
    [TextArea] [SerializeField] private string TextLimitResourcesIron;
    [TextArea] [SerializeField] private string TextLimitResourcesCC;


    [SerializeField] private float TimeHint;

    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject point;
    [SerializeField] private GameObject ButtonUpgrade;

    public static BuildingData buildingData;
    public static Transform buildingTransform;
    public static Building buildingSO;

    [SerializeField] private Transform pointInPanelAngle1;
    [SerializeField] private Transform pointInPanelAngle2;
    [SerializeField] private Transform pointInPanelAngle3;
    [SerializeField] private Transform pointInPanelAngle4;

    [SerializeField] private GameEvent UpdateResourcesEvent;
    private bool IsPanelActive;

    private void Start()
    {
        panel.SetActive(false);
        point.SetActive(false);
        ButtonUpgrade.SetActive(false);
        IsPanelActive = false; 
    }

    private void Update()
    {
        if (IsPanelActive)
        {
            ShowDescriptionPanel();
        }
    }

    /// <summary>
    /// Нажали на здание, открытие панели подробной информации
    /// </summary>
    /// <param name="building"></param>
    public void ShowDescriptionPanel()
    {
        if (buildingData.IsThisBuilt && Time.timeScale == 1f && !TutorialManager.IsTutorialActive && WorkersInterBuildingControl.SelectedWorker == null && WorkersInterBuildingControl.SelectedPlayer == null)
        {
            IsPanelActive = true;
        
            point.SetActive(true);
            panel.SetActive(true);

            if (buildingSO.priceUpgrade > 0 && buildingSO.Level(BaseUpgradeConditionManager.CurrentBaseLevel) <= buildingSO.MaxLevelThisBuilding && buildingData.Level < buildingSO.MaxLevelThisBuilding)
            {
                ButtonUpgrade.SetActive(true);
                ButtonUpgradeTextPanel.text = buildingSO.priceUpgrade.ToString();
            }
            else
            {
                ButtonUpgrade.SetActive(false);
            }

            // Центр экрана
            float centerX = Screen.width / 2;
            float centerY = Screen.height / 2;

            // Экранные координаты объекта
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(buildingTransform.position);
                
            float offsetX = Screen.width * 0.15f; // 10% от ширины экрана
            float offsetY = Screen.height * 0.1f; // 5% от высоты экрана
            
            point.transform.position = screenPosition;
            
            // Определяем четверть
            if (screenPosition.x > centerX && screenPosition.y > centerY)
            {
                //Debug.Log("Объект находится в первой четверти.");
                Vector3 panelPosition = screenPosition + new Vector3(-offsetX, -offsetY, 0);
                
                panel.transform.position = panelPosition;
            }
            else if (screenPosition.x < centerX && screenPosition.y > centerY)
            {
                //Debug.Log("Объект находится во второй четверти.");
                Vector3 panelPosition = screenPosition + new Vector3(offsetX, -offsetY, 0);
                panel.transform.position = panelPosition;
            }
            else if (screenPosition.x < centerX && screenPosition.y < centerY)
            {
                //Debug.Log("Объект находится в третьей четверти.");
                Vector3 panelPosition = screenPosition + new Vector3(offsetX, offsetY, 0);
                panel.transform.position = panelPosition;
                
            }
            else if (screenPosition.x > centerX && screenPosition.y < centerY)
            {
                //Debug.Log("Объект находится в четвёртой четверти.");
                Vector3 panelPosition = screenPosition + new Vector3(-offsetX, offsetY, 0);
                panel.transform.position = panelPosition;
                
            }
            
            Title.text = buildingData.Title;

            //Формирование строки об уровне здания
            Level.text = $"Уровень: {Convert.ToString(buildingData.Level)}";

            //Формирование строки о текущей прочности здания
            Durability.text = $"Прочность: {Convert.ToString(buildingData.Durability)} / {buildingSO.Durability(buildingData.Level)}";

            if (buildingSO.Production(buildingData.Level).resources.Count != 0)
            {
                //Формирование строки после "Производит:" на панели
                string productionTextOutput = "Производит:";
                int iP = 0;
                List<int> listIndexSAProduction = buildingSO.Production(buildingData.Level).SpriteAssetsUsingIndex;
                List<int> resourcesValuesProduction = buildingSO.Production(buildingData.Level).resources;
                foreach (var resource in resourcesValuesProduction)
                {
                    if (iP >= 1)
                    {
                        productionTextOutput += $"+ {resource}   <sprite={listIndexSAProduction[iP]}>";
                    }
                    else
                    {
                        productionTextOutput += $" {resource}   <sprite={listIndexSAProduction[iP]}>";
                    }
                    iP++;
                }
                Production.text = productionTextOutput;
            }
            else
            {
                Production.gameObject.SetActive(false);
            }
            
            //Формирование строки о трате энергомеда
            HoneyConsumption.text = $"Тратит: {Convert.ToString(buildingSO.EnergyHoneyConsumpiton(buildingData.Level))} <sprite=0>";

            if (buildingSO.StorageLimit(buildingData.Level).resources.Count != 0)
            {
                //Формирование строки после "Количество ресурсов:" на панели
                string storageTextOutput = "Лимиты ресурсов:";
                int iS = 0;
                List<int> listIndexSAStorage = buildingSO.StorageLimit(buildingData.Level).SpriteAssetsUsingIndex;
                List<int> resourcesValuesStorage = buildingSO.StorageLimit(buildingData.Level).resources;
                foreach (var resource in resourcesValuesStorage)
                {
                    if (iS >= 1)
                    {
                        storageTextOutput += $" + {resource}   <sprite={listIndexSAStorage[iS]}>";
                    }
                    else
                    {
                        storageTextOutput += $" {resource}   <sprite={listIndexSAStorage[iS]}>"; 
                    }
                    iS++;
                }
                Storage.text = storageTextOutput;
            }
            else
            {
                Storage.gameObject.SetActive(false);
            }

            //DescriptionCanvas.renderMode = RenderMode.WorldSpace;
            //DescriptionCanvas.worldCamera = mainCamera;
        }
        
    }

    /// <summary>
    /// Сокрытие панели подробного описания здания
    /// </summary>
    public void HideDescriptionPanel()
    {
        IsPanelActive = false;
        point.SetActive(false);
        panel.SetActive(false);
        //DescriptionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //DescriptionCanvas.worldCamera = null;
    }

    /// <summary>
    /// Разрушение здания
    /// </summary>
    public async void DestroyBuilding()
    {
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);

        HideDescriptionPanel();
        
        string playerName = UIManagerLocation.WhichPlayerCreate.Name;
        PlayerResources playerResources = await GetResourcesPLayer(UIManagerLocation.WhichPlayerCreate);
        
        GameObject building = buildingTransform.gameObject;
        BuildingData buildingData = building.GetComponent<BuildingData>();
        Building buildingSO = buildingData.buildingTypeSO;

        int NewIron = buildingSO.priceBuilding / 2;
        int OldEnergyValue = playerResources.Energy;
        int OldFoodValue = playerResources.Food;
        playerResources.Energy += buildingData.HoneyConsumption;
        if (building.GetComponent<ThisBuildingWorkersControl>())
        {
            playerResources.Food += building.GetComponent<ThisBuildingWorkersControl>()
                .CurrentNumberWorkersInThisBuilding * 20;
        }

        if (building.GetComponent<EnergyProduction>())
        {
            playerResources = building.GetComponent<EnergyProduction>().OnDestroyThis(playerResources);
        }
        Dictionary<string,string> buildingDictionary = new Dictionary<string, string>();
        buildingDictionary.Add("EnergyValueUpdate", $"{playerResources.Energy - OldEnergyValue}");
        buildingDictionary.Add("FoodValueUpdate", $"{playerResources.Food - OldFoodValue}");
        buildingDictionary.Add("IronValueUpdate", $"{(playerResources.Iron + NewIron) - playerResources.Iron}");
        APIManager.Instance.CreatePlayerLog("Здание продано, игрок получает энергию, металл и еду ( если здание обладает рабочими) ", playerName, buildingDictionary);

        await SyncManager.Enqueue(async () =>
        {
            await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerResources.Iron + NewIron, playerResources.Energy, playerResources.Food,
                playerResources.CryoCrystal);
        });
       
        
        PlayerSaveData playerSaveData = UIManagerLocation.Instance.WhichPlayerDataUse();
        
        playerSaveData.DeleteBuilding(building);
        
        UpdateResourcesEvent.TriggerEvent();

        JSONSerializeManager.Instance.JSONSave();
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }

    /// <summary>
    /// Улучшение здания
    /// </summary>
    public async void UpgradeBuilding()
    {
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);

        string playerName = UIManagerLocation.WhichPlayerCreate.Name;
        PlayerResources playerResources = await GetResourcesPLayer(UIManagerLocation.WhichPlayerCreate);

        int priceUpgrade = buildingSO.priceUpgrade;
        int BaseLevel = BaseUpgradeConditionManager.CurrentBaseLevel;

        if (buildingData.Level >= buildingSO.MaxLevelThisBuilding)
        {
            ButtonUpgrade.SetActive(false); 
        }

        if (buildingData.Title != "Мобильная база")
        {
            if (playerResources.Iron >= priceUpgrade)
            {
                if (BaseLevel >= buildingSO.MBLevelForUpgradethisIron)
                {
                    Dictionary<string, string> playerDictionary = new Dictionary<string, string>();
                    playerDictionary.Add("IronValueUpdate", $"{(playerResources.Iron - priceUpgrade) - playerResources.Iron}");
                    APIManager.Instance.CreatePlayerLog($"Улучшение здания {buildingData.Title}", playerName, playerDictionary);


                    buildingData.Level += 1;
                    buildingData.Durability = buildingSO.Durability(buildingData.Level);
                    buildingData.HoneyConsumption = buildingSO.EnergyHoneyConsumpiton(buildingData.Level);
                    buildingData.Production = buildingSO.Production(buildingData.Level).resources;
                    buildingData.Storage = buildingSO.StorageLimit(buildingData.Level).resources;

                    if (buildingData.Production.Count > 0)
                    {
                        buildingData.Production = buildingSO.Production(buildingData.Level).resources;
                    }

                    await SyncManager.Enqueue(async () =>
                    {
                        if (buildingData.gameObject.GetComponent<EnergyProduction>())
                        {
                            playerResources.Energy += (buildingData.Production[0] -
                                                       (buildingSO.Production(buildingData.Level - 1).resources[0]));
                            playerResources.Food += (buildingData.Production[1] -
                                                     (buildingSO.Production(buildingData.Level - 1).resources[1]));
                        }
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerResources.Iron - priceUpgrade,
                            playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                    });
                    
                    PlayerSaveData playerSaveData = UIManagerLocation.Instance.WhichPlayerDataUse();
                    
                    BuildingSaveData buildingSaveData = new BuildingSaveData(buildingData);
                    playerSaveData.BuildingDatas[buildingData.SaveListIndex] = buildingSaveData;
                    
                    OnHintPanel(UpgradeLevelBuildingInformation);
                }
                else
                {
                    OnHintPanel(TextNotEnoughtBaseLevel);
                }
            }
            else
            {
                OnHintPanel(TextNotEnoughtResources);
            }
        }
        else
        {
            List<string> ImprovementReport = BaseUpgradeConditionManager.Instance.CanUpgradeMobileBase(playerResources);
            if (ImprovementReport[0] == BaseUpgradeConditionManager.Instance.SuccesUpgradeText || ImprovementReport[0] == BaseUpgradeConditionManager.Instance.ENDGAME)
            {
                Dictionary<string, string> playerDictionary = new Dictionary<string, string>();
                playerDictionary.Add("IronValueUpdate", $"{(playerResources.Iron - priceUpgrade) - playerResources.Iron}");
                APIManager.Instance.CreatePlayerLog($"Улучшение здания {buildingData.Title}", playerName, playerDictionary);
                
                await SyncManager.Enqueue(async () =>
                {
                    await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerResources.Iron - priceUpgrade,
                        playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                });
               
                buildingData.Level += 1;
                buildingData.Durability = buildingSO.Durability(BaseLevel);
                buildingData.HoneyConsumption = buildingSO.EnergyHoneyConsumpiton(BaseLevel);

                BaseUpgradeConditionManager.CurrentBaseLevel = buildingData.Level;

                PlayerSaveData playerSaveData = UIManagerLocation.Instance.WhichPlayerDataUse();
                    
                BuildingSaveData buildingSaveData = new BuildingSaveData(buildingData);
                playerSaveData.BuildingDatas[buildingData.SaveListIndex] = buildingSaveData;


                OnHintPanel(ImprovementReport[0]);
            }
            else
            {
                string UnsuccessfullReportText = $"";
                foreach (var report in ImprovementReport)
                {
                    UnsuccessfullReportText += $"\n- {report} ";
                }
                
                OnHintPanel(TextNotCompleteConditionUpgradeMB + UnsuccessfullReportText);
            }
        }
        
        JSONSerializeManager.Instance.JSONSave();
        
        UpdateResourcesEvent.TriggerEvent();
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }

    private void OnHintPanel(string Text)
    {
        HintPanel.transform.parent.gameObject.SetActive(true);
        Utility.Invoke(this, () => HintPanel.transform.parent.gameObject.SetActive(false), TimeHint);
        HintPanel.text = Text;
    }

    public void ResourceIronLimit() => OnHintPanel(TextLimitResourcesIron);

    public void ResourceCCLimit() => OnHintPanel(TextLimitResourcesCC);
    
    private async Task<PlayerResources> GetResourcesPLayer(EntityID playerID)
    {
        PlayerResources playerResources = null;
        await SyncManager.Enqueue(async () =>
        {
            playerResources = await APIManager.Instance.GetPlayerResources(playerID);
        });
        return playerResources;
    }
}
