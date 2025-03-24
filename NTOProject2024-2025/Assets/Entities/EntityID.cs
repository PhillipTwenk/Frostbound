using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Serialization;

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
        return JsonUtility.ToJson(this, true);
    }

    public void DeserializeFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
    
    
    
    
    [FormerlySerializedAs("Name")]
    [Header("Info")]
    [TextArea] public string entityName;
    public string DefaultName;
    public int thisPlayerID;
    
    [Header("Stats")]
    public float speed;
    public float sprintSpeed;
    public float normalSpeed;
    public float speedTurn;

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

