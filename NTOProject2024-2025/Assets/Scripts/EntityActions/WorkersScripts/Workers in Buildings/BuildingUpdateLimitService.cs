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

    private void OnDisable()
    {
        GeneralWorkersControl.Instance.MaxValueOfUnits -= LimitUpUnit;
        GeneralWorkersControl.Instance.MaxValueOfWorkers -= LimitUpWorkers;
        BaseUpgradeConditionManager.CurrentNumberOfHome --;
    }
}
