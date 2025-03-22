using System;
using EntityActions.WorkersScripts;
using UnityEngine;

public class BuildingUpdateLimitService : MonoBehaviour
{
    public int LimitUpUnit;
    public int LimitUpWorkers;
    
    /// <summary>
    /// Увеличивает максимальное допустимое количество рабочих при установке здания
    /// </summary>
    public void PlacementBuilding()
    {
        GeneralWorkersControl.Instance.MaxValueOfUnits += LimitUpUnit;
        GeneralWorkersControl.Instance.MaxValueOfWorkers += LimitUpWorkers;
        BaseUpgradeConditionManager.CurrentNumberOfHome --;
    }

    public void OnDestroyThis()
    {
        GeneralWorkersControl.Instance.MaxValueOfUnits -= LimitUpUnit;
        GeneralWorkersControl.Instance.MaxValueOfWorkers -= LimitUpWorkers;
        BaseUpgradeConditionManager.CurrentNumberOfHome --;
    }
}
