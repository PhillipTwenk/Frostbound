using System.Threading.Tasks;
using EntityActions.Movement_Control;

namespace EntityActions.WorkersScripts
{
    /// <summary>
    /// Интерфейс со специализированными методами и полями для рабочих
    /// </summary>
    public interface IWorkerUnit: IUnitMovement
    {
        bool ReadyForWork { get; set; }
        bool ArriveForBuildBuidling {get; set;}
        
        /// <summary>
        /// Проверяет перед тем как побежать к зданию, не будет ли после ее постройки нарушено потребление энергии
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckEnergyConsumptionBeforeBuilding(BuildingData buildingData);
    }
}
