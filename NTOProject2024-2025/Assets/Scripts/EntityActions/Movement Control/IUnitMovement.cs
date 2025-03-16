using UnityEngine;

namespace EntityActions.Movement_Control
{
    /// <summary>
    /// Интерфейс с основными методами управления юнитом в игре
    /// </summary>
    public interface IUnitMovement
    {
        /// <summary>
        /// Получение информации о нажатой на карте точке
        /// Учитывается только нажатие по указанным в placementLayerMask слоям
        /// </summary>
        /// <returns> Позиция нажатой точки </returns>
        public Vector3 GetSelectedMapPosition();

    
        
        /// <summary>
        /// Задать направление пути юниту
        /// </summary>
        /// <param name="point"> Точка назначения</param>
        /// <param name="isAutomatic"> Автоматическое ли движение </param>
        public void SetUnitDestination(Transform point, bool isAutomatic);

    
        
        /// <summary>
        /// Управление движением юнита
        /// </summary>
        public void MovementHandler();
        
        
        /// <summary>
        /// Свойства контроля выделения
        /// </summary>
        bool isSelected { get; set; }
        bool isSelecting { get; set; }
        GameObject OutlineRotate { get; }
        UnitType ThisUnitType { get; }
        GameObject SelectedBuilding { get; set; }
        bool PossibilityClickOnUnit { get; set; }
        GameObject OutlinePOD { get; }
        Transform UnitPointOfDestination { get; set; }
        Camera MainCamera { get; set; }
    }
}
