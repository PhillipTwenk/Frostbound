using System;
using System.Threading.Tasks;
using GlobalEvents.Cataclysm_Services;
using Unitilities;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Типы добытчиков в зависимости от типа месторождения 
/// </summary>
[Serializable]
public enum MinerType
{
    IronMiner = 0,
    CryoCrystalMiner = 1
}
/// <summary>
/// Отвечает за контроль добычи ресурсов 
/// </summary>
public class ResourceMiner : MonoBehaviour
{
    [Header("Building Info")]
    private BuildingData _buildingData;
    
    [Header("Miner Function")]
    [Tooltip("Интервал добычи")] [SerializeField] private int TimeProduction;
    [NonSerialized]public MinerType _minerType;
    private ResourceData resourcesLimits;
    
    
    [Header("GameEvents")]
    [SerializeField] private GameEvent ResourceIronLimitEvent;
    [SerializeField] private GameEvent ResourceCCLimitEvent;
    [SerializeField] private GameEvent UpdateResourcesEvent;
    [SerializeField] private GameEvent EnergySubZero;

    [Header("Flags")]
    private bool IsWorkStop;
    private bool OneCycle;

    [Header("Animations")]
    [Tooltip("Название параметра для остановки анимации добычи")] [SerializeField] private string stopMineAnimationKey;
    private Animator _animator;

    [Header("Miner Info Texts")]
    [Tooltip("Текст, выводящийся над зданием при остановке работы")] [TextArea] [SerializeField] private string stopTextWorking;
    [Tooltip("Текст, выводящийся над зданием при возобновлении работы")] [TextArea] [SerializeField] private string resumeTextWorking;
    [Tooltip("Время, через которое текст должен удалится")] [SerializeField] private int showTextTime;

    [Header("Unity Events")] 
    [Tooltip("Срабатывает при старте добычи ресурсов")] public UnityEvent OnStartMiningEvent;
    [Tooltip("Срабатывает при окончании добычи ресурсов")] public UnityEvent OnEndMiningEvent;

    [Header("Components")] 
    private InteractionBuildingController interactionBuildingController;


    private void Start()
    {
        _buildingData = GetComponent<BuildingData>();
        _animator = GetComponent<Animator>();
        interactionBuildingController = GetComponent<InteractionBuildingController>();
        _animator.SetBool(stopMineAnimationKey,true);
        resourcesLimits = _buildingData.buildingTypeSO.StorageLimit(BaseUpgradeConditionManager.CurrentBaseLevel);
    }

    private void OnEnable()
    {
        SnowBlizzardGEService.ChangeParametersSnowBlizzardGeEvent += DeclineProduction;
        SnowBlizzardGEService.RevertParametersSnowBlizzardGeEvent += RevertDeclineProduction;
        
        DroneCrashGlobalEventService.RevertDroneBroke += CheckDrone;
    }

    private void OnDisable()
    {
        SnowBlizzardGEService.ChangeParametersSnowBlizzardGeEvent -= DeclineProduction;
        SnowBlizzardGEService.RevertParametersSnowBlizzardGeEvent += RevertDeclineProduction;
        
        DroneCrashGlobalEventService.RevertDroneBroke -= CheckDrone;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Добытчик на месторождении металла
        if (other.gameObject.CompareTag("IronMinerPlace"))
        {
            _minerType = MinerType.IronMiner;
            IsWorkStop = false;
            
            if (_buildingData.IsThisBuilt)
            {
                OnStartMining();
            }
        }
        // Добытчик на месторождении криокристаллов
        else if (other.gameObject.CompareTag("CCminerPlace"))
        {
            _minerType = MinerType.CryoCrystalMiner;
            IsWorkStop = false;
            
            if (_buildingData.IsThisBuilt)
            {
                OnStartMining();
            }
        }
        
    }

    /// <summary>
    /// Запускает корутины для добычи определенного ресурса
    /// </summary>
    public async void OnStartMining()
    {
        if (!IsWorkStop && !OneCycle && _buildingData.IsThisBuilt)
        {
            OneCycle = true;

            if (_minerType == MinerType.IronMiner)
            {
                 MinerIronAsync(_buildingData);
            } 
            else if (_minerType == MinerType.CryoCrystalMiner)
            {
                 MinerCCAsync( _buildingData);
            }
            
            OnStartMiningEvent?.Invoke();
            
            await JSONSerializeManager.Instance.JSONSave();
        }
    }

    /// <summary>
    /// Корутина, запускающая процесс добычи железа, пока не превысит лимит по ресурсам
    /// </summary>
    /// <param name="buildingData"></param>
    private async Task MinerIronAsync(BuildingData buildingData)
    {
        // Инициализация текущего лимита по ресурсу
        int resourceLimit = resourcesLimits.resources[0];
        bool isRunning = true;
        
        while (gameObject.activeSelf && isRunning)
        {
            // Если после добычи ресурса он не привысит лимит по ресурсу
            if ((buildingData.Storage[0] + buildingData.Production[0]) <= resourceLimit)
            {
                _animator.SetBool(stopMineAnimationKey, false);
                
                await Task.Delay(TimeProduction);
                
                buildingData.Storage[0] += buildingData.Production[0];
                
            } 
            
            // Если превысит, добираем недостающее количество ресурса до лимита
            else if ((buildingData.Storage[0] + buildingData.Production[0]) > resourceLimit)
            {
                
                int differenceIron = (buildingData.Storage[0] + buildingData.Production[0]) - resourceLimit; // Разница между лимитом ресурса и значением текущего металла + производство
                buildingData.Storage[0] += buildingData.Production[0] - differenceIron;
                
                // Остановка работы
                IsWorkStop = true;
                OneCycle = false;
                _animator.SetBool(stopMineAnimationKey, true);
                OnEndMiningEvent?.Invoke();

                ShowTextMinerStatus(stopTextWorking);
                
                isRunning = false;
            }
            
            await JSONSerializeManager.Instance.JSONSave();
            
            
            CheckDrone();

            if (isRunning)
            {
                TextMinerChanger();
            }
            
        }
    }

    
    /// <summary>
    /// Корутина, запускающая процесс добычи КриоКристаллов, пока не превысит лимит по ресурсам
    /// </summary>
    /// <param name="buildingData"></param>
    private async Task MinerCCAsync(BuildingData buildingData)
    {
        // Инициализация текущего лимита по ресурсу
        int resourceLimit = resourcesLimits.resources[1];
            
        
        bool isRunning = true;
        
        while (gameObject.activeSelf && isRunning)
        {
            // Если после добычи ресурса он не привысит лимит по ресурсу
            if ((buildingData.Storage[1] + buildingData.Production[1]) <= resourceLimit)
            {
                _animator.SetBool(stopMineAnimationKey, false);
                
                await Task.Delay(TimeProduction);
                
                buildingData.Storage[1] += buildingData.Production[1];
                
                
            } 
            
            // Если превысит, добираем недостающее количество ресурса до лимита
            else if ((buildingData.Storage[1] + buildingData.Production[1]) > resourceLimit)
            {
                
                int differenceIron = (buildingData.Storage[1] + buildingData.Production[1]) - resourceLimit; // Разница между лимитом ресурса и значением текущего металла + производство
                buildingData.Storage[1] += buildingData.Production[1] - differenceIron;
                
                // Остановка работы
                IsWorkStop = true;
                OneCycle = false;
                _animator.SetBool(stopMineAnimationKey, true);

                ShowTextMinerStatus(stopTextWorking);
                
                isRunning = false;
            }
            
            await JSONSerializeManager.Instance.JSONSave();
            

            CheckDrone();
            
            if (isRunning)
            {
                TextMinerChanger();
            }
            
            
        }
    }

    /// <summary>
    /// Проверяет область вокруг здания на наличие дронов
    /// </summary>
    /// <param name="interactionBuildingController"></param>
    public void CheckDrone()
    {
        foreach (var obj in interactionBuildingController.objectsInTrigger)
        {
            if (obj.GetComponent<DroneMovementController>())
            {
                interactionBuildingController.DroneArriveToMiner(obj.GetComponent<DroneMovementController>());
            }
        }
    }
    
    /// <summary>
    /// Показывает нужный текст над зданием на определенное количество времени
    /// </summary>
    private void ShowTextMinerStatus(string text)
    {
        
        bool wasTextActive = _buildingData.AwaitBuildingThisTMPro.gameObject.activeSelf;
        string oldText = _buildingData.AwaitBuildingThisTMPro.text;
        
        _buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(true);

        _buildingData.AwaitBuildingThisTMPro.text = text;
        
        Utility.Invoke(this, () =>
        {
            if (wasTextActive)
            {
                _buildingData.AwaitBuildingThisTMPro.text = oldText;
            }
            else
            {
                _buildingData.AwaitBuildingThisTMPro.gameObject.SetActive(false);
            }
        }, showTextTime);
    }

    /// <summary>
    /// Показывает текущее количество ресурсов в локальном хранилище у добытчика
    /// </summary>
    /// <param name="text"></param>
    public void TextMinerChanger()
    {
        if (_minerType == MinerType.IronMiner)
        {
            _buildingData.AwaitBuildingThisTMPro.text = $"Локальные ресурсы: {_buildingData.Storage[0]}/{resourcesLimits.resources[0]}";
        }
        else if (_minerType == MinerType.CryoCrystalMiner)
        {
            _buildingData.AwaitBuildingThisTMPro.text = $"Локальные ресурсы: {_buildingData.Storage[1]}/{resourcesLimits.resources[1]}";
        }
    }
    public void WorkNotStop() => IsWorkStop = false;

    public void DeclineProduction()
    {
        Debug.Log("Производство майнера снижено");
        _buildingData.Production[(int)_minerType] = 2;
    }

    public void RevertDeclineProduction()
    {
        _buildingData.Production[(int)_minerType] = 10;
    }
    
}
