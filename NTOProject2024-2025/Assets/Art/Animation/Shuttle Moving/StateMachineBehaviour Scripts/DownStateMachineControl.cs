using UnityEngine;

public class DownStateMachineControl: StateMachineBehaviour
{
    [Header("GameEvents")] 
    public GameEvent EndRocketFlyEvent;
    
    
    /// <summary>
    /// Вызывается при выходе из состояния анимации
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EndRocketFlyEvent.TriggerEvent();
    }
}
