using UnityEngine;

public class UpStateMachineControl : StateMachineBehaviour
{
    [Header("GameEvents")] 
    public GameEvent SpacewalkRocketAwaitStartEvent;
    
    
    /// <summary>
    /// Вызывается при выходе из состояния анимации
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SpacewalkRocketAwaitStartEvent.TriggerEvent();
    }
}
