using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Интерфейс, реализующий методы и поля для логистики
/// </summary>
public interface IUnitLogistics
{
    BuildingData buildingDataLogistics { get; set; }
    bool IsLogisticsCycleActive { get; set; }
    void LogisticsCycleToggle(bool ToTheBase, BuildingData buildingTransform);
    int LogisticsStorage { get; set; }
    List<int> MaximumLogisticsStorage { get; }
}
