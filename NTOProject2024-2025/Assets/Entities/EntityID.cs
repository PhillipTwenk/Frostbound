using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Serialization;

[System.Serializable]
public class EntityIDSaveData
{
    public string name;
    public int ID;
    public List<Quest> openQuests;
    public PlayerResources playerResources;
    public ShopResources shopResources;
    public bool isTutorialComplete;
}


/// <summary>
/// Общее описание свойств сущности
/// </summary>
[CreateAssetMenu(menuName = "ForEntities/Entity")]
public class EntityID : ScriptableObject, ISerializableSO 
{
    /// <summary>
    /// Реализация ISerializableSO
    /// </summary>
    /// <returns></returns>
    public string SerializeToJson()
    {
        EntityIDSaveData saveData = new EntityIDSaveData();
        saveData.name = entityName;
        saveData.openQuests = openQuests;
        saveData.playerResources = playerResources;
        saveData.shopResources = shopResources;
        saveData.ID = thisPlayerID;
        saveData.isTutorialComplete = isTutorialComplete;
        return JsonUtility.ToJson(saveData, true);
    }

    public void DeserializeFromJson(string json)
    {
        EntityIDSaveData saveData = new EntityIDSaveData();
        JsonUtility.FromJsonOverwrite(json, saveData);
        entityName = saveData.name;
        openQuests = saveData.openQuests;
        playerResources = saveData.playerResources;
        shopResources = saveData.shopResources;
        isTutorialComplete = saveData.isTutorialComplete;
        thisPlayerID = saveData.ID;
    }
    
    
    
    
    [FormerlySerializedAs("Name")]
    [Header("Info")]
    [TextArea] public string entityName;
    public string DefaultName;
    public int thisPlayerID;
    public bool isTutorialComplete;

    [Header("Quests")]
    public List<Quest> openQuests = new List<Quest>();

    [Header("OfflineData")] 
    public PlayerResources playerResources;
    public ShopResources shopResources;
    public PlayerResources DefaultPlayerResources;
    public ShopResources DefaultShopResources;

    [Header("Data")] 
    public PlayerSaveData _playerSaveData;
    public PlayerSaveData DefaultPlayerSaveData;

    /// <summary>
    /// Обнуление данных этого персонажа
    /// </summary>
    public async Task DefaultRevert()
    {
        if (entityName != DefaultName)
        {
            string shopName = $"{entityName}'sShop";
            await APIManager.Instance.DeleteShop(this, shopName);
            
            await APIManager.Instance.DeletePlayer(this);
        }
        
        entityName = DefaultName;

        playerResources = DefaultPlayerResources;
        shopResources = DefaultShopResources;
        
        _playerSaveData.playerBuildings = DefaultPlayerSaveData.playerBuildings;
        _playerSaveData.buildingsTransform = DefaultPlayerSaveData.buildingsTransform;
        _playerSaveData.BuildingDatas = DefaultPlayerSaveData.BuildingDatas;
        _playerSaveData.BuildingWorkersInformationList =
            DefaultPlayerSaveData.BuildingWorkersInformationList;
    }
}

