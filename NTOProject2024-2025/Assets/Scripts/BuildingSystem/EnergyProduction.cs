using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnergyProduction : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective WorkerStartWorkingOnApiaryTutorial;
    
    private BuildingData _buildingData;
    
    [Header("Events")]
    [SerializeField] private GameEvent ResourceUpdateEvent;
    
    public async void OnAddEnergy()
    {
        _buildingData = GetComponent<BuildingData>();
        if (GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= 1)
        {

            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);
        
            int honeyProduction = _buildingData.Production[0];
            int foodProduction = _buildingData.Production[1];
        
            Debug.Log($"Производство меда: {honeyProduction}");

            string playerName = UIManagerLocation.WhichPlayerCreate.Name;
            PlayerResources playerResources = null;
            await SyncManager.Enqueue(async () =>
            {
                playerResources =
                    await APIManager.Instance.GetPlayerResources(UIManagerLocation.WhichPlayerCreate);
            });
            int OldEnergyValue = playerResources.Energy;
            int OldFoodValue = playerResources.Food;
            playerResources.Energy += honeyProduction;
            playerResources.Food += foodProduction;
            LogSender(playerName, "Пасека начала производство энергии и мёда", playerResources.Energy - OldEnergyValue, playerResources.Food - OldFoodValue);

            await SyncManager.Enqueue(async () =>
            {
                await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerResources.Iron, playerResources.Energy,
                    playerResources.Food, playerResources.CryoCrystal);
                ResourceUpdateEvent.TriggerEvent();
                WorkerStartWorkingOnApiaryTutorial.CheckAndUpdateTutorialState();
            });
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
        }
    }

    public PlayerResources OnDestroyThis(PlayerResources playerResources)
    {
        if (GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= 1)
        {
            _buildingData = GetComponent<BuildingData>();
        
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);    
        
            int honeyProduction = _buildingData.Production[0];
            int foodProduction = _buildingData.Production[1];

            string playerName = UIManagerLocation.WhichPlayerCreate.Name;
            int OldEnergyValue = playerResources.Energy;
            int OldFoodValue = playerResources.Food;
            playerResources.Energy -= honeyProduction;
            playerResources.Food -= foodProduction;
            LogSender(playerName, "Пасека прекратила производство энергии и мёда", playerResources.Energy - OldEnergyValue, playerResources.Food - OldFoodValue );
            // await SyncManager.Enqueue(async () =>
            // {
            //     await APIManager.Instance.PutPlayerResources(playerName, playerResources.Iron, playerResources.Energy,
            //         playerResources.Food, playerResources.CryoCrystal);
            // });
            ResourceUpdateEvent.TriggerEvent();
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
            return playerResources;
        }
        else
        {
            return playerResources;
        }
    }

    public async void OnWorkerLeave(TextMeshPro text)
    {
        if (GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding >= 1)
        {
            // GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding -= 1;
            // _buildingData = GetComponent<BuildingData>();
        
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(true);    
        
            int honeyProduction = _buildingData.Production[0];
            int foodProduction = _buildingData.Production[1];

            string playerName = UIManagerLocation.WhichPlayerCreate.Name;
            PlayerResources playerResources =
                await APIManager.Instance.GetPlayerResources(UIManagerLocation.WhichPlayerCreate);
            int OldEnergyValue = playerResources.Energy;
            int OldFoodValue = playerResources.Food;
            playerResources.Energy -= honeyProduction;
            playerResources.Food -= foodProduction;
            LogSender(playerName, $"{_buildingData.Title} прекратила производство энергии и мёда", playerResources.Energy - OldEnergyValue, playerResources.Food - OldFoodValue );
            await SyncManager.Enqueue(async () =>
            {
                await APIManager.Instance.PutPlayerResources(UIManagerLocation.WhichPlayerCreate, playerResources.Iron, playerResources.Energy,
                    playerResources.Food, playerResources.CryoCrystal);
            });
            ResourceUpdateEvent.TriggerEvent();
            // GameObject newWorker = Instantiate(GetComponent<ThisBuildingWorkersControl>().WorkerPrefab, null);
            // newWorker.transform.position = GetComponent<ThisBuildingWorkersControl>().buildingSpawnWorkerPointTransform.position;
            // newWorker.transform.rotation = GetComponent<ThisBuildingWorkersControl>().buildingSpawnWorkerPointTransform.rotation;
            // newWorker.transform.GetChild(0).GetComponent<WorkerMovementController>().MainCamera = WorkersInterBuildingControl.MainCamera;

            TextChangerEnergy(text);
            LoadingCanvasController.Instance.LoadingCanvasTransparent.SetActive(false);
        }
        
    }

    public void TextChangerEnergy(TextMeshPro text)
    {
        _buildingData = GetComponent<BuildingData>();
        if (GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding == 0)
        {
            text.text = $"{_buildingData.Title} прекратила работу ({GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding}/1)";
        }
        else
        {
            text.text =  $"{_buildingData.Title} работает ({GetComponent<ThisBuildingWorkersControl>().CurrentNumberWorkersInThisBuilding}/1) \n Нажмите E чтобы выгрузить рабочего";
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
