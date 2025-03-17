using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlansInShopControl : MonoBehaviour
{
    // [Header("Tutorial")]
    // [SerializeField] private TutorialObjective OpenShopTutorial;
    // [SerializeField] private TutorialObjective BuyAllPlansTutorial;
    // [SerializeField] private TutorialObjective CloseShopTutorialTutorial;
    // private int plansBuyCounter;
    
    [SerializeField] private GameObject PanelRHBought;
    [SerializeField] private GameObject PanelMBought;
    [SerializeField] private GameObject PanelABought;
    [SerializeField] private GameObject PanelStorageBought;
    [SerializeField] private GameObject PanelPierBought;
    
    [SerializeField] private GameObject NotEnoughtResourcesTextPanel;

    [SerializeField] private GameEvent UpdateResourcesEvent;
    
    [SerializeField] private Button _buttonS;
    [SerializeField] private Button _buttonP;
    [SerializeField] private Button _buttonApiary;
    [SerializeField] private Button _buttonMiner;
    [SerializeField] private Button _buttonHome;
    
    [SerializeField] private string StorageName;
    [SerializeField] private string PierName;
    [SerializeField] private string ApiaryName;
    [SerializeField] private string MinerName;
    [SerializeField] private string HomeName;
    
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
        //OpenShopTutorial.CheckAndUpdateTutorialState();
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        
        NotEnoughtResourcesTextPanel.SetActive(false);
        
        PanelStorageBought.SetActive(false);
        PanelPierBought.SetActive(false);
        PanelABought.SetActive(false);
        PanelRHBought.SetActive(false);
        PanelMBought.SetActive(false);
        
        string playerName = CurrentPlayersDataControl.WhichPlayerCreate.entityName;
        string shopName = $"{playerName}'sShop";
        ShopResources shopResources = await GetResourcesShop(playerName, shopName);
        if (TutorialManager.IsTutorialActive)
        {
            
        }
        else
        {
            // Определяем текущий уровень базы
            int baseLevel = BaseUpgradeConditionManager.CurrentBaseLevel;

            // Словарь: уровень базы → товары, доступные на этом уровне
            Dictionary<int, List<PriceShopProduct>> baseLevelProducts = new Dictionary<int, List<PriceShopProduct>>
            {
                { 1, new List<PriceShopProduct> { shopResources.ResidentialModule, shopResources.Apiary, shopResources.Minner, shopResources.Pier, shopResources.Storage } }
            };

            // Словарь: товар → его купленный UI + кнопка
            Dictionary<PriceShopProduct, (GameObject boughtPanel, GameObject buttonParent)> shopUIElements =
                new Dictionary<PriceShopProduct, (GameObject, GameObject)>
                {
                    { shopResources.ResidentialModule, (PanelRHBought, _buttonHome.transform.parent.gameObject) },
                    { shopResources.Apiary, (PanelABought, _buttonApiary.transform.parent.gameObject) },
                    { shopResources.Minner, (PanelMBought, _buttonMiner.transform.parent.gameObject) },
                    { shopResources.Pier, (PanelPierBought, _buttonP.transform.parent.gameObject) },
                    { shopResources.Storage, (PanelStorageBought, _buttonS.transform.parent.gameObject) }
                };

            // Проверяем, какие товары доступны на этом уровне базы
            if (baseLevelProducts.TryGetValue(baseLevel, out var products))
            {
                foreach (var product in products)
                {
                    if (shopUIElements.TryGetValue(product, out var uiElements))
                    {
                        uiElements.boughtPanel.transform.parent.gameObject.SetActive(true); // Включаем панель чертежа (родитель купленного UI)
                        if (product.IsPurchased)
                        {
                            uiElements.boughtPanel.SetActive(true); // Показываем, что товар куплен
                            uiElements.buttonParent.SetActive(false); // Скрываем кнопку (родителя кнопки)
                        }
                    }
                }
            }

            // Дополнительно: активируем купленные панели вне зависимости от уровня
            foreach (var item in shopUIElements)
            {
                if (item.Key.IsPurchased)
                {
                    item.Value.boughtPanel.SetActive(true);
                    item.Value.buttonParent.SetActive(false);
                }
            }

        }
        
        WorkersInterBuildingControl.possiilityControlEntities = false;
        
        
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }

    private void OnDisable()
    {
        WorkersInterBuildingControl.possiilityControlEntities = true;
        
        //CloseShopTutorialTutorial.CheckAndUpdateTutorialState();
    }

    /// <summary>
    /// Нажатие на кнопку покупки чертежа
    /// </summary>
    public async void ClickBuyPlanButton(string typeBuyButton)
    {
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        
        string playerName = CurrentPlayersDataControl.WhichPlayerCreate.entityName;
        PlayerResources playerResources = await GetResourcesPLayer(playerName);
        string shopName = $"{playerName}'sShop";
        ShopResources shopResources = await GetResourcesShop(playerName, shopName);

        NotEnoughtResourcesTextPanel.SetActive(false);

        // Словарь соответствий: название чертежа → его данные
        Dictionary<string, (PriceShopProduct product, GameObject boughtPanel, GameObject button, Plan planUI)> shopItems =
            new Dictionary<string, (PriceShopProduct, GameObject, GameObject, Plan)>
            {
                { ApiaryName, (shopResources.Apiary, PanelABought, _buttonApiary.gameObject, APlan) },
                { HomeName, (shopResources.ResidentialModule, PanelRHBought, _buttonHome.gameObject, HPlan) },
                { MinerName, (shopResources.Minner, PanelMBought, _buttonMiner.gameObject, MPlan) },
                { StorageName, (shopResources.Storage, PanelStorageBought, _buttonS.gameObject, SPlan) },
                { PierName, (shopResources.Pier, PanelPierBought, _buttonP.gameObject, PPlan) }
            };

        // Проверяем, есть ли товар в словаре
        if (!shopItems.TryGetValue(typeBuyButton, out var shopItem)) return;

        var (product, boughtPanel, button, planUI) = shopItem;

        // Проверяем, куплен ли уже этот чертеж
        if (product.IsPurchased)
        {
            boughtPanel.SetActive(true);
            button.SetActive(false);
            return;
        }

        // Проверяем, хватает ли ресурсов
        if (playerResources.Iron >= product.IronPrice && playerResources.CryoCrystal >= product.CryoCrystalPrice)
        {
            product.IsPurchased = true;

            await SyncManager.Enqueue(async () =>
            {
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, shopName, 
                    shopResources.Apiary, shopResources.MobileBase, shopResources.Storage,
                    shopResources.ResidentialModule, shopResources.Minner, shopResources.Pier);
                
                await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, 
                    playerResources.Iron - product.IronPrice, 
                    playerResources.Energy, playerResources.Food, 
                    playerResources.CryoCrystal - product.CryoCrystalPrice);
            });

            // Обновляем UI
            boughtPanel.SetActive(true);
            button.SetActive(false);
            UIManager.Instance.AddNewPlanInPanel(planUI);
        }
        else
        {
            NotEnoughtResourcesTextPanel.SetActive(true);
        }

        UpdateResourcesEvent.TriggerEvent();
        LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
    }

    
    private async Task<PlayerResources> GetResourcesPLayer(string playerName)
    {
        PlayerResources playerResources = null;
        await SyncManager.Enqueue(async () =>
        {
            playerResources = await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
        });
        return playerResources;
    }
    
    private async Task<ShopResources> GetResourcesShop(string playerName, string shopName)
    {
        ShopResources shopResources = null;
        await SyncManager.Enqueue(async () =>
        {
            shopResources = await APIManager.Instance.GetShopResources(CurrentPlayersDataControl.WhichPlayerCreate, shopName);
        });
        return shopResources;
    }
}

