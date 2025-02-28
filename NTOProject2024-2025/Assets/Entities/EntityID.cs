using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Общее описание свойств сущности
/// </summary>
[CreateAssetMenu(menuName = "ForEntities/Entity")]
public class EntityID : ScriptableObject, ISerializableSO
{
    // Реализация ISerializableSO
    public string SerializeToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void DeserializeFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
    
    
    
    
    [Header("Info")]
    [TextArea] public string Name;
    public string DefaultName;
    public int thisPlayerID;
    
    [Header("Stats")]
    public float speed;
    public float sprintSpeed;
    public float normalSpeed;
    public float speedTurn;
    //public int jumpForce;

    [Header("Quests")]
    public Quest currentQuest;
    public List<Quest> openQuests;

    [Header("OfflineData")] 
    public PlayerResources playerResources;
    public ShopResources shopResources;
    public PlayerResources DefaultPlayerResources;
    public ShopResources DefaultShopResources;

    [Header("Data")] 
    public PlayerSaveData _playerSaveData;
    public PlayerSaveData DefaultPlayerSaveData;

    public async Task DefaultRevert()
    {
        if (Name != DefaultName)
        {
            string shopName = $"{Name}'sShop";
            await APIManager.Instance.DeleteShop(this, shopName);
            
            await APIManager.Instance.DeletePlayer(this);
        }
        
        Name = DefaultName;

        playerResources = DefaultPlayerResources;
        shopResources = DefaultShopResources;
        
        _playerSaveData.playerBuildings = DefaultPlayerSaveData.playerBuildings;
        _playerSaveData.buildingsTransform = DefaultPlayerSaveData.buildingsTransform;
        _playerSaveData.BuildingDatas = DefaultPlayerSaveData.BuildingDatas;
        _playerSaveData.BuildingWorkersInformationList =
            DefaultPlayerSaveData.BuildingWorkersInformationList;
    }
}

