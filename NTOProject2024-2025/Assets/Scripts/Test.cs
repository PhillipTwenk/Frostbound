using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
   public GameEvent UpdateResources;
   public async void OnMouseDown()
   {
      PlayerResources resources = await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
      
      await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, resources.Iron + 1, resources.Energy + 1, resources.Food + 1, resources.CryoCrystal + 1);
      
      UpdateResources.TriggerEvent();

      // await APIManager.Instance.PutShopResources(UIManagerLocation.WhichPlayerCreate.Name,
      //    $"{UIManagerLocation.WhichPlayerCreate.Name}'sShop", 0, 0, 0, 0, 0, 0, 0);
      // ShopResources resShop = await APIManager.Instance.GetShopResources(UIManagerLocation.WhichPlayerCreate.Name,
      //    $"{UIManagerLocation.WhichPlayerCreate.Name}'sShop");
      //
      // Debug.Log(resShop);
      //
      // Dictionary<string, ShopData> dicShops = await APIManager.Instance.GetShopsList(UIManagerLocation.WhichPlayerCreate.Name);
      //
      // Debug.Log(dicShops[$"{UIManagerLocation.WhichPlayerCreate.Name}'sShop"]);
   }
}
