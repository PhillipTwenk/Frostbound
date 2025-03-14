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
    }
}
