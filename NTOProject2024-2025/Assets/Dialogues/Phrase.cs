using UnityEngine;

namespace Dialogues
{
    [System.Serializable]
    public enum DialogueSide
    {
        Left,
        Right
    }
    
    [System.Serializable]
    public enum ActionCategory
    {
        None,
        BuildBuilding,
        UIPanel,
        CallWorker,
        MoveUnit,
        InteractWithObject
    }
    
    [System.Serializable]
    public enum ActionTypeUIPanel
    {
        None,
        BuyFirstThreePlans,
        CloseBarter,
        OpenBuildingPanel,
        OpenCallingWorkersPanel,
        BuyFoodModulePlan,
        BuyHomePlan,
        BuyAllPlans,
    }
    
    [System.Serializable]
    public enum ActionTypeCallWorker
    {
        None,
        CallConstructor,
        CallBeekeeper,
        EndAwaitRocket,
        CreateDrone,
        UnsuccesefullCallBeekeeper,
    }
    
    [System.Serializable]
    public enum ActionTypeMoveUnit
    {
        None,
        SelectUnit,
        DeselectUnit,
        SelectConstructor,
        SelectBeekeeper,
        SelectDrone,
        TakeOffDrone,
        SetFreeDestination,
    }
    
    [System.Serializable]
    public enum ActionTypeInteractWithObject
    {
        None,
        SelectPlan,
        PlacementBuilding,
        EndWorkingOnBuilding,
        WorkerDestinationOnBuilding,
        WorkerCameToBuilding,
        ClickOnMobileBase,
        OpenBarter,
        DroneGetResources,
        DroneDestinationOnBuilding,
        DroneCameToBuilding
    }
    
    [CreateAssetMenu(fileName = "Phrase", menuName = "Dialogues/Phrase")]
    public class Phrase : ScriptableObject
    {
        [Header("Info")]
        [Tooltip("Текст фразы")] [TextArea] public string text;
        [Tooltip("Изображение говорящего персонажа")] public Sprite characterImage;
        [Tooltip("Имя говорящего персонажа")] public string characterName;  
        
        [Header("State")]
        [Tooltip("В какой стороне будет находиться окно фразы")] public DialogueSide side;
        [Tooltip("Будет ли остановлено время и показан UI затеменения на данной фразе")] public bool isFade;
        [Tooltip("Нужно ли сделать какое-либо действие для продолжения диалога")] public bool isActionAwait;
        public ActionCategory actionCategory;
        public BuildingsTypes actionParameterBuildBuilding;
        public ActionTypeUIPanel actionParameterUIPanel;
        public ActionTypeCallWorker actionParameterCallWorker;
        public ActionTypeMoveUnit actionParameterMoveUnit;
        public ActionTypeInteractWithObject actionParameterInteractWithObject;

    }
}