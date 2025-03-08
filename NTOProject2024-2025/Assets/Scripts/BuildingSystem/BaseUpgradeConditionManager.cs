using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BaseUpgradeConditionManager : MonoBehaviour
{
    public static BaseUpgradeConditionManager Instance {get; set;}

    public static int CurrentBaseLevel;
    public static BuildingData buildingDataMB;

    public List<bool> FindNote;
    
    public List<int> NumberOfWorkersForDifferentLevels;
    
    [TextArea] public string NotEnoughtResourcesTextError;  
    [TextArea] public string NotEnoughtWorkers;
    [TextArea] public string NotEnoughtLevelSomeBuildings;
    [TextArea] public string SuccesUpgradeText;
    [TextArea] public string NoStorageBuidlingText;
    [TextArea] public string NoApiaryBuidlingText;
    [TextArea] public string NoMinerBuidlingText;
    [TextArea] public string NoPierBuidlingText;
    [TextArea] public string NoHomeBuidlingText;
    
    [TextArea] public string ENDGAME;

    [SerializeField] private GameEvent ResourceMinerRestored;

    [Header("Shield")]
    [SerializeField] private Material ShieldColor;
    [SerializeField] private MeshRenderer ShieldRenderer;
    
    private void Awake()
    {
        Instance = this;
    }

    public void Initialization()
    {
        WorkersInterBuildingControl.Instance.MaxValueOfWorkers =
            NumberOfWorkersForDifferentLevels[CurrentBaseLevel - 1];
    }

    private void Update()
    {
        //чит
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     CurrentBaseLevel += 1; 
        //     ResourceMinerRestored.TriggerEvent();
        //     Debug.Log(CurrentBaseLevel);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     Dictionary<string, string> testDictionary = new Dictionary<string, string>();
        //     testDictionary.Add("Шкебедедопдодп", "+1488 ");
        //     testDictionary.Add("ДАбулум нипнип", "- 997 deadinside");
        //     APIManager.Instance.CreatePlayerLog("Тестовые логи шкебеде допдоп", UIManagerLocation.WhichPlayerCreate.Name, testDictionary);
        // }
    }

    public List<string> CanUpgradeMobileBase(PlayerResources playerResources)
    {
        int WorkersCount = WorkersInterBuildingControl.Instance.MaxValueOfWorkers;
        string playerName = CurrentPlayersDataControl.WhichPlayerCreate.entityName;
        int IronCountPlayer = playerResources.Iron;
        List<GameObject> CurrentBuidlings = CurrentPlayersDataControl.WhichPlayerCreate._playerSaveData.playerBuildings;
        List<BuildingSaveData> buildingSDs = CurrentPlayersDataControl.WhichPlayerCreate._playerSaveData.BuildingDatas;

        List<string> resultReport = new List<string>();
        bool IsThisReportUnsuccess = false;

        

        switch (CurrentBaseLevel)
        {
            case 1:
                //Перепроверка условий
                if (playerResources.Iron < buildingDataMB.buildingTypeSO.priceUpgrade)
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtResourcesTextError}: {playerResources.Iron} / {buildingDataMB.buildingTypeSO.priceUpgrade}";
                    resultReport.Add(report);
                }
                if (WorkersCount < NumberOfWorkersForDifferentLevels[0])
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtWorkers}: {WorkersCount} / {NumberOfWorkersForDifferentLevels[0]}";
                    resultReport.Add(report);
                }
                int currentNumberNeededBuildingLevel1;
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 0, 1))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoApiaryBuidlingText}: {currentNumberNeededBuildingLevel1} / {1}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 5, 1))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoHomeBuidlingText}: {currentNumberNeededBuildingLevel1} / {1}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 4, 1))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoPierBuidlingText}: {currentNumberNeededBuildingLevel1} / {1}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 1, 1))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoMinerBuidlingText}: {currentNumberNeededBuildingLevel1} / {1}";
                    resultReport.Add(report);
                }
                int currentNumberNeededBuildingLevelMBL1;
                if (!BuildingNeededLevelCheck(out currentNumberNeededBuildingLevelMBL1, buildingSDs, 1, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtLevelSomeBuildings} 1 здание {2} уровня";
                    resultReport.Add(report);
                }
                



                //Отравка ответа
                if (IsThisReportUnsuccess)
                {
                    return resultReport;
                }
                else
                {
                    resultReport.Clear();
                    resultReport.Add(SuccesUpgradeText);
                    ResourceMinerRestored.TriggerEvent();
                    return resultReport;
                }

                break;
            case 2:
                //Перепроверка условий
                if (playerResources.Iron < buildingDataMB.buildingTypeSO.priceUpgrade)
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtResourcesTextError}: {playerResources.Iron} / {buildingDataMB.buildingTypeSO.priceUpgrade}";
                    resultReport.Add(report);
                }
                if (WorkersCount < NumberOfWorkersForDifferentLevels[1])
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtWorkers}: {WorkersCount} / {NumberOfWorkersForDifferentLevels[1]}";
                    resultReport.Add(report);
                }

                int currentNumberNeededBuildingLevel2;
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel2, CurrentBuidlings, 6, 1))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoStorageBuidlingText}: {currentNumberNeededBuildingLevel2} / {1}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 0, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoApiaryBuidlingText}: {currentNumberNeededBuildingLevel1} / {2}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 5, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoHomeBuidlingText}: {currentNumberNeededBuildingLevel1} / {2}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 1, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoMinerBuidlingText}: {currentNumberNeededBuildingLevel1} / {2}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 4, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoPierBuidlingText}: {currentNumberNeededBuildingLevel1} / {2}";
                    resultReport.Add(report);
                }
                int currentNumberNeededBuildingLevelMBL2;
                if (!BuildingNeededLevelCheck(out currentNumberNeededBuildingLevelMBL2, buildingSDs, 2, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtLevelSomeBuildings} 2 здания {2} уровня";
                    resultReport.Add(report);
                }
                
                
                //Отравка ответа
                if (IsThisReportUnsuccess)
                {
                    return resultReport;
                }
                else
                {
                    resultReport.Clear();
                    resultReport.Add(SuccesUpgradeText);
                    ResourceMinerRestored.TriggerEvent();
                    return resultReport;
                }

                break;
            case 3:
                //Перепроверка условий
                if (playerResources.Iron < buildingDataMB.buildingTypeSO.priceUpgrade)
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtResourcesTextError}: {playerResources.Iron} / {buildingDataMB.buildingTypeSO.priceUpgrade}";
                    resultReport.Add(report);
                }
                if (WorkersCount < NumberOfWorkersForDifferentLevels[2])
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtWorkers}: {WorkersCount} / {NumberOfWorkersForDifferentLevels[2]}";
                    resultReport.Add(report);
                }
                int currentNumberNeededBuildingLevel3;
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel3, CurrentBuidlings, 6, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoStorageBuidlingText}: {currentNumberNeededBuildingLevel3} / {2}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 0, 3))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoApiaryBuidlingText}: {currentNumberNeededBuildingLevel1} / {3}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 1, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoMinerBuidlingText}: {currentNumberNeededBuildingLevel1} / {2}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 4, 4))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoPierBuidlingText}: {currentNumberNeededBuildingLevel1} / {4}";
                    resultReport.Add(report);
                }
                if (!BuildingNeededNumberCheck(out currentNumberNeededBuildingLevel1, CurrentBuidlings, 5, 3))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NoHomeBuidlingText}: {currentNumberNeededBuildingLevel1} / {3}";
                    resultReport.Add(report);
                }
                int currentNumberNeededBuildingLevelMBL3;
                if (!BuildingNeededLevelCheck(out currentNumberNeededBuildingLevelMBL3, buildingSDs, 3, 2))
                {
                    IsThisReportUnsuccess = true;
                    string report = $"{NotEnoughtLevelSomeBuildings} 3 здания {2} уровня";
                    resultReport.Add(report);
                }
                


                //Отравка ответа
                if (IsThisReportUnsuccess)
                {
                    return resultReport;
                }
                else
                {
                    resultReport.Clear();
                    resultReport.Add(ENDGAME);
                    ResourceMinerRestored.TriggerEvent();
                    ShieldRenderer.material = ShieldColor;
                    return resultReport;
                }

                break;
        }

        JSONSerializeManager.Instance.JSONSave();
        
        return null;
    }

    public bool BuildingNeededNumberCheck(out int currentNumberNeededBuilding, List<GameObject> currentBuildings, int IDoB, int number)
    {
        currentNumberNeededBuilding = 0;
        foreach (var building in currentBuildings)
        {
            if (building.transform.GetChild(0).GetComponent<BuildingData>().buildingTypeSO.IDoB == IDoB)
            {
                currentNumberNeededBuilding += 1;
            }
        }

        if (currentNumberNeededBuilding >= number)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool BuildingNeededLevelCheck(out int currentNumberNeededBuildingLevel, List<BuildingSaveData> currentBuildingSaveDatas, int number, int NeededLevel)
    {
        currentNumberNeededBuildingLevel = 0;
        foreach (var buildingSD in currentBuildingSaveDatas)
        {
            if (buildingSD.Level == NeededLevel)
            {
                currentNumberNeededBuildingLevel += 1;
            }
        }

        if (currentNumberNeededBuildingLevel >= number)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
