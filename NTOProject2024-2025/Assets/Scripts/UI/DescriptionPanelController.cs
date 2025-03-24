using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityActions.WorkersScripts;
using TMPro;
using Unitilities;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class DescriptionPanelController : MonoBehaviour
    {

        [Header("Events")]
        public static Action<int> OnMobileBaseUpgrade;
    
    
    
    
    
        [SerializeField] private TextMeshProUGUI Title;
        [SerializeField] private TextMeshProUGUI Level;
        [SerializeField] private TextMeshProUGUI Durability;
        [SerializeField] private TextMeshProUGUI Production;
        [SerializeField] private TextMeshProUGUI HoneyConsumption;
        [SerializeField] private TextMeshProUGUI Storage;

        [SerializeField] private TextMeshProUGUI HintPanel;
        [SerializeField] private TextMeshProUGUI ButtonUpgradeTextPanel;
        [SerializeField] private TextMeshProUGUI ButtonFunctionTextPanel;
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
        [SerializeField] private GameObject ButtonFunctionThisBuilding;

        public static BuildingData buildingData;
        public static Transform buildingTransform;
        public static Building buildingSO;
        private InteractionBuildingController currentIBC;

        [SerializeField] private Transform pointInPanelAngle1;
        [SerializeField] private Transform pointInPanelAngle2;
        [SerializeField] private Transform pointInPanelAngle3;
        [SerializeField] private Transform pointInPanelAngle4;

        [SerializeField] private GameEvent UpdateResourcesEvent;
        private bool IsPanelActive;

        private void Start()
        {
            currentIBC = null;
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
        /// Выполняется 1 раз перед открытием панели подробного описания 
        /// </summary>
        public void EventODP()
        {
            if (buildingData.IsThisBuilt && Mathf.Approximately(Time.timeScale, 1f) && !TutorialManager.IsTutorialActive &&
                GeneralWorkersControl.SelectedUnit == null)
            {
                UIManager.CancelLastOpenPanelEvent += HideDescriptionPanel;
                currentIBC = buildingData.gameObject.GetComponent<InteractionBuildingController>();
                Debug.Log("Открыта панель подробного описания информации о здании ");
            }
        }
    
        /// <summary>
        /// Нажали на кнопку функции здания в меню подробного просмотра информации о здании 
        /// </summary>
        public void ClickOnFunctionButtonInDescriptionPanel()
        {
            currentIBC?.InteractionEvent?.Invoke();
        }
    
        /// <summary>
        /// Нажали на здание, открытие панели подробной информации
        /// </summary>
        public void ShowDescriptionPanel()
        {
            if (buildingData.IsThisBuilt && Mathf.Approximately(Time.timeScale, 1f) && !TutorialManager.IsTutorialActive &&
                GeneralWorkersControl.SelectedUnit == null)
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
            
                if (currentIBC.PossiblityPutEInThisBuilding)
                {
                    ButtonFunctionThisBuilding.SetActive(true);
                    ButtonFunctionTextPanel.text = currentIBC.nameOfFunction;
                }
                else
                {
                    ButtonFunctionThisBuilding.SetActive(false);
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
                Durability.text = $"Состояние: {Convert.ToString(buildingData.Durability)} / {buildingSO.Durability(buildingData.Level)}";

                // Формирование строки о производстве здания 
                if (buildingData.Production.Count > 0)
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
                            productionTextOutput += $"+ {resource} <sprite={listIndexSAProduction[iP]}>";
                        }
                        else
                        {
                            productionTextOutput += $" {resource} <sprite={listIndexSAProduction[iP]}>";
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

                if (buildingData.Storage.Count > 0)
                {
                    //Формирование строки после "Количество ресурсов:" на панели
                    string storageTextOutput = "Локальное хранилище:";
                    int iS = 0;
                    List<int> listIndexSAStorage = buildingSO.StorageLimit(buildingData.Level).SpriteAssetsUsingIndex;
                    List<int> resourcesValuesStorage = buildingData.Storage;
                    List<int> limitsStorage = buildingSO.StorageLimit(buildingData.Level).resources;
                    foreach (var resource in resourcesValuesStorage)
                    {
                        if (iS >= 1)
                        {
                            storageTextOutput += $" + {resource}/{limitsStorage[iS]} <sprite={listIndexSAStorage[iS]}>";
                        }
                        else
                        {
                            storageTextOutput += $" {resource}/{limitsStorage[iS]} <sprite={listIndexSAStorage[iS]}>"; 
                        }
                        iS++;
                    }
                    Storage.text = storageTextOutput;
                }
                else
                {
                    Storage.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Сокрытие панели подробного описания здания
        /// </summary>
        public void HideDescriptionPanel()
        {
            Debug.Log($"<color=yellow> Закрыто окно подробного просмотра </color>");
            IsPanelActive = false;
            point.SetActive(false);
            panel.SetActive(false);
            UIManager.CancelLastOpenPanelEvent -= HideDescriptionPanel;
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
        
            PlayerResources playerResources = await GetResourcesPLayer(CurrentPlayersDataControl.WhichPlayerCreate);
        
            GameObject building = buildingTransform.gameObject;
            BuildingData buildingData = building.GetComponent<BuildingData>();
            Building buildingSO = buildingData.buildingTypeSO;

            int NewIron = buildingSO.priceBuilding / 2;
            playerResources.Energy += buildingData.HoneyConsumption;

            CompletionOfConstructionController completionOfConstructionController =
                buildingData.gameObject.GetComponent<CompletionOfConstructionController>();
            UnityEvent OnDestroyThisBuildingEvent = completionOfConstructionController?.OnDestroyBuilding;
            OnDestroyThisBuildingEvent?.Invoke();
        
        
            await SyncManager.Enqueue(async () =>
            {
                await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron + NewIron, playerResources.Energy, playerResources.Food,
                    playerResources.CryoCrystal);
            });
        
            PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        
            playerSaveData.DeleteBuilding(building);
        
            UpdateResourcesEvent.TriggerEvent();

            await JSONSerializeManager.Instance.JSONSave();
        
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
        }

        /// <summary>
        /// Улучшение здания
        /// </summary>
        public async void UpgradeBuilding()
        {
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);

            string playerName = CurrentPlayersDataControl.WhichPlayerCreate.entityName;
            PlayerResources playerResources = await GetResourcesPLayer(CurrentPlayersDataControl.WhichPlayerCreate);

            int priceUpgrade = buildingSO.priceUpgrade;
            int BaseLevel = BaseUpgradeConditionManager.CurrentBaseLevel;

            if (buildingData.Level >= buildingSO.MaxLevelThisBuilding)
            {
                ButtonUpgrade.SetActive(false); 
            }

            if (buildingData.buildingTypeSO.buildingType != BuildingsTypes.MobileBase)
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
                        //buildingData.Storage = buildingSO.StorageLimit(buildingData.Level).resources;

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
                            await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron - priceUpgrade,
                                playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                        });
                    
                        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
                    
                        BuildingSaveData buildingSaveData = new BuildingSaveData(buildingData);
                        playerSaveData.BuildingDatas[buildingData.SaveListIndex] = buildingSaveData;
                    
                        buildingData.OnUpgradeEvent?.Invoke();
                    
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
                List<string> ImprovementReport = await BaseUpgradeConditionManager.Instance.CanUpgradeMobileBase(playerResources);
                if (ImprovementReport[0] == BaseUpgradeConditionManager.Instance.SuccesUpgradeText || ImprovementReport[0] == BaseUpgradeConditionManager.Instance.ENDGAME)
                {
                    Dictionary<string, string> playerDictionary = new Dictionary<string, string>();
                    playerDictionary.Add("IronValueUpdate", $"{(playerResources.Iron - priceUpgrade) - playerResources.Iron}");
                    APIManager.Instance.CreatePlayerLog($"Улучшение здания {buildingData.Title}", playerName, playerDictionary);
                
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron - priceUpgrade,
                            playerResources.Energy, playerResources.Food, playerResources.CryoCrystal);
                    });
               
                    buildingData.Level += 1;
                    buildingData.Durability = buildingSO.Durability(BaseLevel);
                    buildingData.HoneyConsumption = buildingSO.EnergyHoneyConsumpiton(BaseLevel);

                    BaseUpgradeConditionManager.CurrentBaseLevel = buildingData.Level;
                
                    BaseUpgradeConditionManager.Instance.Initialization();

                    PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
                    
                    BuildingSaveData buildingSaveData = new BuildingSaveData(buildingData);
                    playerSaveData.BuildingDatas[buildingData.SaveListIndex] = buildingSaveData;
                
                    buildingData.OnUpgradeEvent?.Invoke();
                    OnMobileBaseUpgrade?.Invoke(buildingData.Level);
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
        
            await JSONSerializeManager.Instance.JSONSave();
        
            UpdateResourcesEvent?.TriggerEvent();
        
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
}
