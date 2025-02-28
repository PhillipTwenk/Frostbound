using UnityEngine;

public class TestBuy : MonoBehaviour
{
    [SerializeField] private GameEvent ApiaryShopEvent;
    [SerializeField] private GameEvent HoneyGunShopEvent;
    [SerializeField] private GameEvent MobileBaseShopEvent;
    [SerializeField] private GameEvent StorageShopEvent;
    [SerializeField] private GameEvent ResidentialModuleShopEvent;
    [SerializeField] private GameEvent BreadwinnerShopEvent;
    [SerializeField] private GameEvent PierShopEvent;
    
    public async void OnMouseDown()
    {
        ShopResources res = await APIManager.Instance.GetShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop");
        
        switch (gameObject.name)
        {
            case "ApiaryShop":
                PriceShopProduct ApiaryPrice = new PriceShopProduct()
                {
                    IronPrice = res.Apiary.IronPrice,
                    CryoCrystalPrice = res.Apiary.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", ApiaryPrice , res.HoneyGun,res.MobileBase,res.Storage,res.ResidentialModule,res.Minner,res.Pier);
                ApiaryShopEvent.TriggerEvent();
                break;
            case "HoneyGunShop":
                PriceShopProduct HoneyGunPrice = new PriceShopProduct()
                {
                    IronPrice = res.HoneyGun.IronPrice,
                    CryoCrystalPrice = res.HoneyGun.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", res.Apiary, HoneyGunPrice,res.MobileBase,res.Storage,res.ResidentialModule,res.Minner,res.Pier);
                HoneyGunShopEvent.TriggerEvent();
                break;
            case "MobileBaseShop":
                PriceShopProduct MobileBasePrice = new PriceShopProduct()
                {
                    IronPrice = res.MobileBase.IronPrice,
                    CryoCrystalPrice = res.MobileBase.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", res.Apiary, res.HoneyGun,MobileBasePrice,res.Storage,res.ResidentialModule,res.Minner,res.Pier);
                MobileBaseShopEvent.TriggerEvent();
                break;
            case "StorageShop":
                PriceShopProduct StoragePrice = new PriceShopProduct()
                {
                    IronPrice = res.Storage.IronPrice,
                    CryoCrystalPrice = res.Storage.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", res.Apiary, res.HoneyGun,res.MobileBase,StoragePrice,res.ResidentialModule,res.Minner,res.Pier);
                StorageShopEvent.TriggerEvent();
                break;
            case "ResidentialModuleShop":
                PriceShopProduct RM = new PriceShopProduct()
                {
                    IronPrice = res.ResidentialModule.IronPrice,
                    CryoCrystalPrice = res.ResidentialModule.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", res.Apiary, res.HoneyGun,res.MobileBase,res.Storage,RM,res.Minner,res.Pier);
                ResidentialModuleShopEvent.TriggerEvent();
                break;
            case "BreadwinnerShop":
                PriceShopProduct Br = new PriceShopProduct()
                {
                    IronPrice = res.Minner.IronPrice,
                    CryoCrystalPrice = res.Minner.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", res.Apiary, res.HoneyGun,res.MobileBase,res.Storage,res.ResidentialModule,Br,res.Pier);
                BreadwinnerShopEvent.TriggerEvent();
                break;
            case "PierShop":
                PriceShopProduct pier = new PriceShopProduct()
                {
                    IronPrice = res.Pier.IronPrice,
                    CryoCrystalPrice = res.Pier.CryoCrystalPrice,
                    IsPurchased = true
                };
                await APIManager.Instance.PutShopResources(CurrentPlayersDataControl.WhichPlayerCreate, $"{CurrentPlayersDataControl.WhichPlayerCreate.Name}'sShop", res.Apiary, res.HoneyGun,res.MobileBase,res.Storage,res.ResidentialModule,res.Minner,pier);
                PierShopEvent.TriggerEvent();
                break;
        }
    }
}
