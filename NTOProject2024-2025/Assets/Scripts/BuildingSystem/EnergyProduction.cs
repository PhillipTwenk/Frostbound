using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnergyProduction : MonoBehaviour
{
    
    private BuildingData _buildingData;
    private ThisBuildingWorkersControl _thisBuildingWorkersControl;
    
    [Header("Events")]
    [SerializeField] private GameEvent ResourceUpdateEvent;

    private void Start()
    {
        _thisBuildingWorkersControl = GetComponent<ThisBuildingWorkersControl>();
        _buildingData = GetComponent<BuildingData>();
    }

    public async void OnAddEnergy()
    {
        if (_thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding >= 1)
        {

            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        
            int honeyProduction = _buildingData.Production[0];
            int foodProduction = _buildingData.Production[1];
        
            Debug.Log($"Производство меда: {honeyProduction}");
            
            PlayerResources playerResources = null;
            await SyncManager.Enqueue(async () =>
            {
                playerResources =
                    await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
            });
            playerResources.Energy += honeyProduction;
            playerResources.Food += foodProduction;

            await SyncManager.Enqueue(async () =>
            {
                await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron, playerResources.Energy,
                    playerResources.Food, playerResources.CryoCrystal);
                ResourceUpdateEvent.TriggerEvent();
            });
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
        }
    }

    /// <summary>
    /// Добавляется в ивент, который вызывается при уничтожении здания
    /// </summary>
    public async void OnDestroyThis()
    {
        
        PlayerResources playerResources = await GetResourcesPLayer(CurrentPlayersDataControl.WhichPlayerCreate);
        
        if (_thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding >= 1)
        {
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);    
        
            int honeyProduction = _buildingData.Production[0];
            int foodProduction = _buildingData.Production[1];
            
            playerResources.Energy -= honeyProduction;
            playerResources.Food -= foodProduction;
            ResourceUpdateEvent.TriggerEvent();
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
        }
        
        await SyncManager.Enqueue(async () =>
        {
            await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron, playerResources.Energy, playerResources.Food,
                playerResources.CryoCrystal);
        });
    }
    
    private async Task<PlayerResources> GetResourcesPLayer(EntityID playerID)
    {
        PlayerResources playerResources = null;
        await SyncManager.Enqueue(async () =>
        {
            playerResources = await APIManager.Instance.GetPlayerResources(playerID);
        });
        return playerResources;
    }

    public async void OnWorkerLeave(TextMeshPro text)
    {
        if (_thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding >= 1)
        {
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);    
        
            int honeyProduction = _buildingData.Production[0];
            int foodProduction = _buildingData.Production[1];
            
            PlayerResources playerResources =
                await APIManager.Instance.GetPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate);
            playerResources.Energy -= honeyProduction;
            playerResources.Food -= foodProduction;
            await SyncManager.Enqueue(async () =>
            {
                await APIManager.Instance.PutPlayerResources(CurrentPlayersDataControl.WhichPlayerCreate, playerResources.Iron, playerResources.Energy,
                    playerResources.Food, playerResources.CryoCrystal);
            });
            ResourceUpdateEvent.TriggerEvent();

            TextChangerEnergy(text);
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
        }
        
    }

    public void TextChangerEnergy(TextMeshPro text)
    {
        if (_thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding == 0)
        {
            text.text = $"{_buildingData.Title} прекратила работу ({_thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding}/{_thisBuildingWorkersControl.MaxValueOfWorkersInThisBuilding})";
        }
        else if (_thisBuildingWorkersControl.CurrentNumberWorkersInThisBuilding == _thisBuildingWorkersControl.MaxValueOfWorkersInThisBuilding)
        {
            text.text =  $"{_buildingData.Title} работает ({GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding}/{GetComponent<ThisBuildingWorkersControl>().MaxValueOfWorkersInThisBuilding}) \n Нажмите E чтобы выгрузить рабочего";
        }
    }

    private void LogSender(string playerName, string comment, int ChangeEnergy, int ChangeFood)
    {
        Dictionary<string,string> playerDictionary = new Dictionary<string, string>();
        playerDictionary.Add("EnergyValueUpdate", $"{ChangeEnergy}");
        playerDictionary.Add("FoodValueUpdate", $"{ChangeFood}");
        APIManager.Instance.CreatePlayerLog(comment, playerName, playerDictionary);
    }
}
