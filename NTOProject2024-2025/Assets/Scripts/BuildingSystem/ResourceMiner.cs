using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceMiner : MonoBehaviour
{
    [SerializeField] private int ThisSOIDOB; //6
    public string MinerType;
    [SerializeField] private string IronMinerType;
    [SerializeField] private string CCMinerType;

    [Header("GameEvents")]
    [SerializeField] private GameEvent ResourceIronLimitEvent;
    [SerializeField] private GameEvent ResourceCCLimitEvent;
    [SerializeField] private GameEvent UpdateResourcesEvent;
    [SerializeField] private GameEvent EnergySubZero;

    private bool IsWorkStop;
    private bool OneCycle;
    private bool CanSendMessageToHint;

    [SerializeField] private int TimeProduction;

    private Animator _animator;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.SetBool("StopMining",true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("IronMinerPlace"))
        {
            Debug.Log("Добытчик добывает металл");
            MinerType = IronMinerType;
            IsWorkStop = false;
            BuildingData buildingData = GetComponent<BuildingData>();
            if (buildingData.IsThisBuilt)
            {
                OnStartMining();
            }
        }else if (other.gameObject.CompareTag("CCminerPlace"))
        {
            Debug.Log("Добытчик добывает кристалл");
            MinerType = CCMinerType;
            IsWorkStop = false;
            BuildingData buildingData = GetComponent<BuildingData>();
            if (buildingData.IsThisBuilt)
            {
                Debug.Log(0912);
                OnStartMining();
            }
        }
        
    }

    public async void OnStartMining()
    {
        if (!IsWorkStop && !OneCycle)
        {
            OneCycle = true;

            BuildingData buildingData = GetComponent<BuildingData>();
        
            PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();

            if (MinerType == IronMinerType)
            {
                await MinerIronAsync(CurrentPlayersDataControl.WhichPlayerCreate, playerSaveData, buildingData);
            } else if (MinerType == CCMinerType)
            {
                 await MinerCCAsync(CurrentPlayersDataControl.WhichPlayerCreate, playerSaveData, buildingData);
            }
        }
    }

    /// <summary>
    /// Корутина, запускающая процесс добычи железа, пока не превысит лимит по ресурсам
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="playerResources"></param>
    /// <param name="IronLimit"></param>
    /// <param name="buildingData"></param>
    /// <returns></returns>
    private async Task MinerIronAsync(EntityID playerID, PlayerSaveData playerSaveData, BuildingData buildingData)
    {
        bool isRunning = true;
        while (gameObject.activeSelf && isRunning)
        {
            BuildingSaveData MobileBaseBD = playerSaveData.BuildingDatas[0];

            int StorageAdd = 0;
            foreach (var building in playerSaveData.playerBuildings)
            {
                if (building.transform.GetChild(0).GetComponent<BuildingData>().buildingTypeSO.IDoB == ThisSOIDOB)
                {
                    StorageAdd += building.transform.GetChild(0).GetComponent<BuildingData>().Storage[0];
                }
            }
            int IronLimit = MobileBaseBD.Storage[0] + StorageAdd;

            Debug.Log($"Лимит по металлу: {IronLimit}");

            PlayerResources playerResources = await GetResources(playerID);
            if ((playerResources.Iron + buildingData.Production[0]) <= IronLimit && playerResources.Energy >= 0)
            {
                _animator.SetBool("StopMining",false);
                CanSendMessageToHint = true;
                Debug.Log($"Старое количество металла: {playerResources.Iron}");
                int OldIronValue = playerResources.Iron;
                playerResources.Iron += buildingData.Production[0];
                Debug.Log($"Новое количество металла: {playerResources.Iron}");
                LogSender(playerID.Name, OldIronValue, playerResources.Iron, IronMinerType);
                await UpdateResources(playerResources, playerID);
                await Task.Delay(TimeProduction);
            } else if ((playerResources.Iron + buildingData.Production[0]) > IronLimit)
            {
                // Добрать недостающее количество металла до лимита
                CanSendMessageToHint = true;
                Debug.Log($"Старое количество металла: {playerResources.Iron}");
                int OldIronValue = playerResources.Iron;
                int DifferenceIron = (playerResources.Iron + buildingData.Production[0]) - IronLimit; // Разница между лимитом металла и значением текущего металла + производство
                playerResources.Iron += buildingData.Production[0] - DifferenceIron;
                Debug.Log($"Новое количество металла: {playerResources.Iron}");
                LogSender(playerID.Name, OldIronValue, playerResources.Iron, IronMinerType);
                await UpdateResources(playerResources, playerID);
                await Task.Delay(TimeProduction);
                
                // Остановка работы
                IsWorkStop = true;
                OneCycle = false;
                _animator.SetBool("StopMining",true);
                if (CanSendMessageToHint)
                {
                    ResourceIronLimitEvent.TriggerEvent();
                    CanSendMessageToHint = false;
                }
                isRunning = false;
            }else if (playerResources.Energy < 0)
            {
                IsWorkStop = true;
                OneCycle = false;
                _animator.SetBool("StopMining",true);
                if (CanSendMessageToHint)
                {
                    EnergySubZero.TriggerEvent();
                    CanSendMessageToHint = false;
                }
                isRunning = false;
            }
        }
        
        JSONSerializeManager.Instance.JSONSave();
    }

    
    /// <summary>
    /// Корутина, запускающая процесс добычи КриоКристаллов, пока не превысит лимит по ресурсам
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="playerResources"></param>
    /// <param name="CCLimit"></param>
    /// <param name="buildingData"></param>
    /// <returns></returns>
    private async Task MinerCCAsync(EntityID playerID, PlayerSaveData playerSaveData, BuildingData buildingData)
    {
        bool isRunning = true;
        while (gameObject.activeSelf && isRunning)
        {
            BuildingSaveData MobileBaseBD = playerSaveData.BuildingDatas[0];

            int StorageAdd = 0;
            foreach (var building in playerSaveData.playerBuildings)
            {
                if (building.transform.GetChild(0).GetComponent<BuildingData>().buildingTypeSO.IDoB == ThisSOIDOB)
                {
                    StorageAdd += building.transform.GetChild(0).GetComponent<BuildingData>().Storage[1];
                }
            }
            int CCLimit = MobileBaseBD.Storage[1] + StorageAdd;

            Debug.Log($"Лимит по КриоКристаллам: {CCLimit}");

            PlayerResources playerResources = await GetResources(playerID);
            if ((playerResources.CryoCrystal + buildingData.Production[1]) <= CCLimit && playerResources.Energy >= 0)
            {
                _animator.SetBool("StopMining",false);
                Debug.Log($"Старое количество КриоКристаллов: {playerResources.CryoCrystal}");
                int OldCCValue = playerResources.CryoCrystal;
                playerResources.CryoCrystal += buildingData.Production[1];
                Debug.Log($"Новое количество КриоКристаллов: {playerResources.CryoCrystal}");
                LogSender(playerID.Name, OldCCValue, playerResources.CryoCrystal, CCMinerType);
                await UpdateResources(playerResources, playerID);
                await Task.Delay(TimeProduction);
            } else if ((playerResources.CryoCrystal + buildingData.Production[1]) > CCLimit)
            {
                // Добрать значение криоКристалла до лимита
                Debug.Log($"Старое количество КриоКристаллов: {playerResources.CryoCrystal}");
                int OldCCValue = playerResources.CryoCrystal;
                int DifferenceCC = (playerResources.CryoCrystal + buildingData.Production[1]) - CCLimit;
                playerResources.CryoCrystal += buildingData.Production[1] - DifferenceCC;
                Debug.Log($"Новое количество КриоКристаллов: {playerResources.CryoCrystal}");
                LogSender(playerID.Name, OldCCValue, playerResources.CryoCrystal, CCMinerType);
                await UpdateResources(playerResources, playerID);
                await Task.Delay(TimeProduction);
                
                // Остановка работы
                IsWorkStop = true;
                OneCycle = false;
                _animator.SetBool("StopMining",true);
                if (CanSendMessageToHint)
                {
                    ResourceCCLimitEvent.TriggerEvent();
                    CanSendMessageToHint = false;
                }
                isRunning = false;
            }else if (playerResources.Energy <= 0)
            {
                IsWorkStop = true;
                OneCycle = false;
                _animator.SetBool("StopMining",true);
                if (CanSendMessageToHint)
                {
                    EnergySubZero.TriggerEvent();
                    CanSendMessageToHint = false;
                }
                isRunning = false;
            }
        }
        
        JSONSerializeManager.Instance.JSONSave();
    }

    private async Task UpdateResources(PlayerResources playerResources, EntityID playerID)
    {
        await SyncManager.Enqueue(async () =>
        {
            await APIManager.Instance.PutPlayerResources(playerID, playerResources.Iron, playerResources.Energy,
                playerResources.Food, playerResources.CryoCrystal);
            UpdateResourcesEvent.TriggerEvent();
        });
    }

    private async Task<PlayerResources> GetResources(EntityID playerID)
    {
        PlayerResources playerResources = null;
        await SyncManager.Enqueue(async () =>
        {
           playerResources = await APIManager.Instance.GetPlayerResources(playerID);
           Debug.Log(010101010);
        });
        Debug.Log(2020202020);
        return playerResources;
    }

    private void LogSender(string playerName, int OldValue, int NewValue, string Type)
    {
        Dictionary<string,string> playerDictionary = new Dictionary<string, string>();
        if (Type == IronMinerType)
        {
            playerDictionary.Add("IronValueUpdate", $"{NewValue - OldValue}");
            APIManager.Instance.CreatePlayerLog("Добытчик добыл новую партию железа, ресурсы персонажа обновлены",playerName, playerDictionary);
        }else if (Type == CCMinerType)
        {
            playerDictionary.Add("CryoCrystalValueUpdate", $"{NewValue - OldValue}");
            APIManager.Instance.CreatePlayerLog("Добытчик добыл новую партию Криокристаллов, ресурсы персонажа обновлены",playerName, playerDictionary);
        }
    }
    public void WorkNotStop() => IsWorkStop = false; 
}
