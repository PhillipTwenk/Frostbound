using UnityEngine;

[CreateAssetMenu(menuName = "QuestScriptableObjects/TutorialObjective")]
public class TutorialObjective : ScriptableObject
{
    [Header("Objective Texts")]
    [TextArea] public string Title;
    [TextArea] public string TextOnThisObjective;
    [TextArea] public string TechnicalTExtOnThisObjective;
    
    //public Objective tutorialObjective;
    public bool IsActive;

    public bool IsTimeStopOnThisStep;
    public int MainUIPositionTypeOnThisStep;
    public int TechUIPositionTypeOnThisStep;

    public bool IsTechPanelActiveOnThisStep;
    public bool IsAutomaticalyEnterOnThisStep;

    public bool IsClosePlanMenuOnThisStep;

    // public void CheckAndUpdateTutorialState()
    // {
    //     if (IsActive && TutorialManager.IsTutorialActive)
    //     {
    //         TutorialManager.UpdateTutorialStateEvent?.Invoke();
    //     }
    // }
}
