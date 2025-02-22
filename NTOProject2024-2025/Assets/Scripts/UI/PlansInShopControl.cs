using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlansInShopControl : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective OpenShopTutorial;
    [SerializeField] private TutorialObjective BuyAllPlansTutorial;
    [SerializeField] private TutorialObjective CloseShopTutorialTutorial;
    private int plansBuyCounter;
    
    [SerializeField] private GameObject PanelHoneyGun;
    [SerializeField] private GameObject PanelStorage;
    [SerializeField] private GameObject PanelPier;

    [SerializeField] private GameObject PanelRH;
    [SerializeField] private GameObject PanelM;
    [SerializeField] private GameObject PanelA;
    [SerializeField] private GameObject PanelRHBought;
    [SerializeField] private GameObject PanelMBought;
    [SerializeField] private GameObject PanelABought;
    
    [SerializeField] private GameObject PanelHoneyGunBought;
    [SerializeField] private GameObject PanelStorageBought;
    [SerializeField] private GameObject PanelPierBought;
    [SerializeField] private GameObject NotEnoughtResourcesTextPanel;

    [SerializeField] private GameEvent UpdateResourcesEvent;

    [SerializeField] private Button _buttonHG;
    [SerializeField] private Button _buttonS;
    [SerializeField] private Button _buttonP;
    [SerializeField] private Button _buttonApiary;
    [SerializeField] private Button _buttonMiner;
    [SerializeField] private Button _buttonHome;
    
    [SerializeField] private string HoneyGunName;
    [SerializeField] private string StorageName;
    [SerializeField] private string PierName;
    [SerializeField] private string ApiaryName;
    [SerializeField] private string MinerName;
    [SerializeField] private string HomeName;

    [SerializeField] private Plan HGPlan;
    [SerializeField] private Plan SPlan;
    [SerializeField] private Plan PPlan;
    [SerializeField] private Plan APlan;
    [SerializeField] private Plan HPlan;
    [SerializeField] private Plan MPlan;

    /// <summary>
    /// При включении панели бартера в зависимости от уровня базы дает разные предложения
    /// </summary>
    private async void OnEnable()
    {
        OpenShopTutorial.CheckAndUpdateTutorialState();
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        NotEnoughtResourcesTextPanel.SetActive(false);
        PanelHoneyGunBought.SetActive(false);
        PanelStorageBought.SetActive(false);
        PanelPierBought.SetActive(false);
        string playerName = UIManagerLocation.WhichPlayerCreate.Name;
        string shopName = $"{playerName}'sShop";
        ShopResources shopResources = await GetResourcesShop(playerName, shopName);
        if (TutorialManager.IsTutorialActive)
        {
            PanelRH.SetActive(true);
            if (shopResources.ResidentialModule.IsPurchased)
            {
                PanelRHBought.SetActive(true);
                _buttonHome.enabled = false;
            }
            PanelA.SetActive(true);
            if (shopResources.Apiary.IsPurchased)
            {
                PanelABought.SetActive(true);
                _buttonApiary.enabled = false;
            }
            PanelM.SetActive(true);
            if (shopResources.Minner.IsPurchased)
            {
                PanelMBought.SetActive(true);
                _buttonMiner.enabled = false;
            }
        }
        else
        {
            PanelRH.SetActive(false);
            PanelA.SetActive(false);
            PanelM.SetActive(false);
            switch (BaseUpgradeConditionManager.CurrentBaseLevel)
            {
                case 1:
                    PanelHoneyGun.SetActive(true);
                    if (shopResources.HoneyGun.IsPurchased)
                    {
                        PanelHoneyGunBought.SetActive(true);
                        _buttonHG.enabled = false;
                    }
                    break;
                case 2:
                    PanelStorage.SetActive(true);
                    if (shopResources.Storage.IsPurchased)
                    {
                        PanelStorageBought.SetActive(true);
                        _buttonS.enabled = false;
                    }
                    break;
                case 3:
                    PanelPier.SetActive(true);
                    if (shopResources.Pier.IsPurchased)
                    {
                        PanelPierBought.SetActive(true);
                        _buttonP.enabled = false;
                    }
                    break;
            }
        }
        
        WorkersInterBuildingControl.possiilityControlEntities = false;
        
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }

    private void OnDisable()
    {
        WorkersInterBuildingControl.possiilityControlEntities = true;
        
        CloseShopTutorialTutorial.CheckAndUpdateTutorialState();
    }

    /// <summary>
    /// Нажатие на кнопку покупки чертежа
    /// </summary>
    public async void ClickBuyPlanButton(string TypeBuyButton)
    {
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        string playerName = UIManagerLocation.WhichPlayerCreate.Name;
        PlayerResources playerResources = await GetResourcesPLayer(playerName);

        string shopName = $"{playerName}'sShop";
        ShopResources shopResources = await GetResourcesShop(playerName, shopName);

        int playerIron = playerResources.Iron;
        int playerCryoCrystal = playerResources.CryoCrystal;
        int playerEnergy = playerResources.Energy;
        int playerFood = playerResources.Food;

        NotEnoughtResourcesTextPanel.SetActive(false);

        Dictionary<string,string> shopDictionary = new Dictionary<string, string>();
        Dictionary<string,string> playerResourcesDictionary = new Dictionary<string, string>();
        
        if (TypeBuyButton == ApiaryName)
        {
            if (!shopResources.Apiary.IsPurchased && TutorialManager.IsTutorialActive)
            {
                if (playerIron >= shopResources.Apiary.IronPrice &&
                    playerCryoCrystal >= shopResources.Apiary.CryoCrystalPrice)
                {
                    shopResources.Apiary.IsPurchased = true;
                    
                    shopDictionary.Add("ApiaryShopValueUpdate", "true");
                    APIManager.Instance.CreateShopLog("Куплен чертеж пасеки (товары в единичном экземпляре)", playerName, shopName, shopDictionary);
                    
                    playerResourcesDictionary.Add("IronValueUpdate", $"{(playerIron - shopResources.Apiary.IronPrice) - playerIron}");
                    playerResourcesDictionary.Add("CrytoCrystalValueUpdate", $"{(playerCryoCrystal - shopResources.Apiary.CryoCrystalPrice) - playerCryoCrystal}");
                    APIManager.Instance.CreatePlayerLog("Куплен чертеж пасеки в магазине, потрачены металл и кристаллы", playerName, playerResourcesDictionary);
                    
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate, shopName, shopResources.Apiary,
                            shopResources.HoneyGun, shopResources.MobileBase, shopResources.Storage,
                            shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerIron - shopResources.Apiary.IronPrice, playerEnergy, playerFood, playerCryoCrystal - shopResources.Apiary.CryoCrystalPrice);
                        plansBuyCounter++;
                        if (plansBuyCounter == 3)
                        {
                            BuyAllPlansTutorial.CheckAndUpdateTutorialState();
                        }
                    });
                    PanelABought.SetActive(true);
                    _buttonApiary.enabled = false;
                    UIManager.Instance.AddNewPlanInPanel(APlan);
                }
                else
                {
                    NotEnoughtResourcesTextPanel.SetActive(true);
                }
            }
            else
            {
                PanelABought.SetActive(true);
                _buttonApiary.enabled = false;
            }
        }
        
        if (TypeBuyButton == HomeName)
        {
            if (!shopResources.ResidentialModule.IsPurchased && TutorialManager.IsTutorialActive)
            {
                if (playerIron >= shopResources.ResidentialModule.IronPrice &&
                    playerCryoCrystal >= shopResources.ResidentialModule.CryoCrystalPrice)
                {
                    shopResources.ResidentialModule.IsPurchased = true;
                    
                    shopDictionary.Add("ResidentialModuleShopValueUpdate", "true");
                    APIManager.Instance.CreateShopLog("Куплен чертеж жилого модуля (товары в единичном экземпляре)", playerName, shopName, shopDictionary);
                    
                    playerResourcesDictionary.Add("IronValueUpdate", $"{(playerIron - shopResources.ResidentialModule.IronPrice) - playerIron}");
                    playerResourcesDictionary.Add("CrytoCrystalValueUpdate", $"{(playerCryoCrystal - shopResources.ResidentialModule.CryoCrystalPrice) - playerCryoCrystal}");
                    APIManager.Instance.CreatePlayerLog("Куплен чертеж жилого модуля в магазине, потрачены металл и кристаллы", playerName, playerResourcesDictionary);
                    
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate, shopName, shopResources.Apiary,
                            shopResources.HoneyGun, shopResources.MobileBase, shopResources.Storage,
                            shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerIron - shopResources.ResidentialModule.IronPrice, playerEnergy, playerFood, playerCryoCrystal - shopResources.ResidentialModule.CryoCrystalPrice);
                        plansBuyCounter++;
                        if (plansBuyCounter == 3)
                        {
                            BuyAllPlansTutorial.CheckAndUpdateTutorialState();
                        }
                    });
                    PanelRHBought.SetActive(true);
                    _buttonHome.enabled = false;
                    UIManager.Instance.AddNewPlanInPanel(HPlan);
                }
                else
                {
                    NotEnoughtResourcesTextPanel.SetActive(true);
                }
            }
            else
            {
                PanelRHBought.SetActive(true);
                _buttonHome.enabled = false;
            }
        }
        
        if (TypeBuyButton == MinerName)
        {
            if (!shopResources.Minner.IsPurchased && TutorialManager.IsTutorialActive)
            {
                if (playerIron >= shopResources.Minner.IronPrice &&
                    playerCryoCrystal >= shopResources.Minner.CryoCrystalPrice)
                {
                    shopResources.Minner.IsPurchased = true;
                    
                    shopDictionary.Add("MinnerShopValueUpdate", "true");
                    APIManager.Instance.CreateShopLog("Куплен чертеж добывающего здания (товары в единичном экземпляре)", playerName, shopName, shopDictionary);
                    
                    playerResourcesDictionary.Add("IronValueUpdate", $"{(playerIron - shopResources.Minner.IronPrice) - playerIron}");
                    playerResourcesDictionary.Add("CryoCrystalValueUpdate", $"{(playerCryoCrystal - shopResources.Minner.CryoCrystalPrice) - playerCryoCrystal}");
                    APIManager.Instance.CreatePlayerLog("Куплен чертеж добывающего здания в магазине, потрачены металл и кристаллы", playerName, playerResourcesDictionary);
                    
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate, shopName, shopResources.Apiary,
                            shopResources.HoneyGun, shopResources.MobileBase, shopResources.Storage,
                            shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerIron - shopResources.Minner.IronPrice, playerEnergy, playerFood, playerCryoCrystal - shopResources.Minner.CryoCrystalPrice);
                        plansBuyCounter++;
                        if (plansBuyCounter == 3)
                        {
                            BuyAllPlansTutorial.CheckAndUpdateTutorialState();
                        }
                    });
                    PanelMBought.SetActive(true);
                    _buttonMiner.enabled = false;
                    UIManager.Instance.AddNewPlanInPanel(MPlan);
                }
                else
                {
                    NotEnoughtResourcesTextPanel.SetActive(true);
                }
            }
            else
            {
                PanelMBought.SetActive(true);
                _buttonMiner.enabled = false;
            }
        }
        
        if (TypeBuyButton == HoneyGunName)
        {
            if (!shopResources.HoneyGun.IsPurchased)
            {
                if (playerIron >= shopResources.HoneyGun.IronPrice &&
                    playerCryoCrystal >= shopResources.HoneyGun.CryoCrystalPrice)
                {
                    shopResources.HoneyGun.IsPurchased = true;
                    
                    shopDictionary.Add("HoneyGunShopValueUpdate", "true");
                    APIManager.Instance.CreateShopLog("Куплен чертеж медовой пушки (товары в единичном экземпляре)", playerName, shopName, shopDictionary);
                    
                    playerResourcesDictionary.Add("IronValueUpdate", $"{(playerIron - shopResources.HoneyGun.IronPrice) - playerIron}");
                    playerResourcesDictionary.Add("CrytoCrystalValueUpdate", $"{(playerCryoCrystal - shopResources.HoneyGun.CryoCrystalPrice) - playerCryoCrystal}");
                    APIManager.Instance.CreatePlayerLog("Куплен чертеж медовой пушки в магазине, потрачены металл и кристаллы", playerName, playerResourcesDictionary);
                    
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate, shopName, shopResources.Apiary,
                            shopResources.HoneyGun, shopResources.MobileBase, shopResources.Storage,
                            shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerIron - shopResources.HoneyGun.IronPrice, playerEnergy, playerFood, playerCryoCrystal - shopResources.HoneyGun.CryoCrystalPrice);
                    });
                    PanelHoneyGunBought.SetActive(true);
                    _buttonHG.enabled = false;
                    UIManager.Instance.AddNewPlanInPanel(HGPlan);
                }
                else
                {
                    NotEnoughtResourcesTextPanel.SetActive(true);
                }
            }
            else
            {
                PanelHoneyGunBought.SetActive(true);
                _buttonHG.enabled = false;
            }
        }

        if (TypeBuyButton == StorageName)
        {
            if (!shopResources.Storage.IsPurchased)
            {
                if (playerIron >= shopResources.Storage.IronPrice &&
                    playerCryoCrystal >= shopResources.Storage.CryoCrystalPrice)
                {
                    shopResources.Storage.IsPurchased = true;
                    
                    shopDictionary.Add("StorageShopValueUpdate", "true");
                    APIManager.Instance.CreateShopLog("Куплен чертеж хранилища ресурсов (товары в единичном экземпляре)", playerName, shopName, shopDictionary);
                    
                    playerResourcesDictionary.Add("IronValueUpdate", $"{(playerIron - shopResources.Storage.IronPrice) - playerIron}");
                    playerResourcesDictionary.Add("CrytoCrystalValueUpdate", $"{(playerCryoCrystal - shopResources.Storage.CryoCrystalPrice) - playerCryoCrystal}");
                    APIManager.Instance.CreatePlayerLog("Куплен чертеж хранилища в магазине, потрачены металл и кристаллы", playerName, playerResourcesDictionary);
                    
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate, shopName, shopResources.Apiary,
                            shopResources.HoneyGun, shopResources.MobileBase, shopResources.Storage,
                            shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerIron - shopResources.Storage.IronPrice, playerEnergy, playerFood, playerCryoCrystal - shopResources.Storage.CryoCrystalPrice);
                    });
                   
                    PanelStorageBought.SetActive(true);
                    _buttonS.enabled = false;
                    UIManager.Instance.AddNewPlanInPanel(SPlan);
                }
                else
                {
                    NotEnoughtResourcesTextPanel.SetActive(true);
                }
            }
            else
            {
                PanelHoneyGunBought.SetActive(true);
                _buttonHG.enabled = false;
            }
        }

        if (TypeBuyButton == PierName)
        {
            if (!shopResources.Pier.IsPurchased)
            {
                if (playerIron >= shopResources.Pier.IronPrice &&
                    playerCryoCrystal >= shopResources.Pier.CryoCrystalPrice)
                {
                    shopResources.Pier.IsPurchased = true;
                    
                    shopDictionary.Add("PierShopValueUpdate", "true");
                    APIManager.Instance.CreateShopLog("Куплен чертеж пристани (товары в единичном экземпляре)", playerName, shopName, shopDictionary);
                    
                    playerResourcesDictionary.Add("IronValueUpdate", $"{(playerIron - shopResources.Pier.IronPrice) - playerIron}");
                    playerResourcesDictionary.Add("CrytoCrystalValueUpdate", $"{(playerCryoCrystal - shopResources.Pier.CryoCrystalPrice) - playerCryoCrystal}");
                    APIManager.Instance.CreatePlayerLog("Куплен чертеж пристани в магазине, потрачены металл и кристаллы", playerName, playerResourcesDictionary);
                    
                    await SyncManager.Enqueue(async () =>
                    {
                        await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate, shopName, shopResources.Apiary,
                            shopResources.HoneyGun, shopResources.MobileBase, shopResources.Storage,
                            shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                        await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerIron - shopResources.Pier.IronPrice, playerEnergy, playerFood, playerCryoCrystal - shopResources.Pier.CryoCrystalPrice);
                    });
                    
                    PanelPierBought.SetActive(true);
                    _buttonP.enabled = false;
                    UIManager.Instance.AddNewPlanInPanel(PPlan);
                }
                else
                {
                    NotEnoughtResourcesTextPanel.SetActive(true);
                }
            }
            else
            {
                PanelHoneyGunBought.SetActive(true);
                _buttonHG.enabled = false;
            }
        }

        UpdateResourcesEvent.TriggerEvent();

        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }
    
    private async Task<PlayerResources> GetResourcesPLayer(string playerName)
    {
        PlayerResources playerResources = null;
        await SyncManager.Enqueue(async () =>
        {
            playerResources = await APIManager.Instance.GetPlayerResources(UIManagerLocation.WhichPlayerCreate);
        });
        return playerResources;
    }
    
    private async Task<ShopResources> GetResourcesShop(string playerName, string shopName)
    {
        ShopResources shopResources = null;
        await SyncManager.Enqueue(async () =>
        {
            shopResources = await APIManager.Instance.GetShopResources(UIManagerLocation.WhichPlayerCreate, shopName);
        });
        return shopResources;
    }
}

