using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public enum BuildingsTypes
{
    FoodModule = 0,
    Miner = 1,
    HoneyGun = 2,
    MobileBase = 3,
    HydroelectricModule = 4,
    Home = 5,
    EngineeringModule = 6,
    None = 7
}
public class BuildingData : MonoBehaviour
{
    [Header("Building type source")] [Tooltip("Ссылка на SO здания")]
    public Building buildingTypeSO;

    [Header("Properties")]
    [Tooltip("Название")] public string Title;
    [Tooltip("Текущий уровень данного здания")] public int Level;
    [Tooltip("Текущее количество прочности здания")] public int Durability;
    [Tooltip("Текущее количество хранимых ресурсов")] public List<int> Storage;
    [Tooltip("Текущее количество производимых ресурсов")] public List<int> Production;
    [Tooltip("Текущее количество потребления энергии")] public int HoneyConsumption;
    [Tooltip("Индекс при сохранении здания")] public int SaveListIndex;
    [Tooltip("Построено ли данное здание")] public bool IsThisBuilt;

    [Header("UnityEvents")] 
    [Tooltip("Вызывается при улучшении здания")] public UnityEvent OnUpgradeEvent;
    [Tooltip("Вызывается при старте игры")] public UnityEvent OnStartEvent;
    
    [Header("Components")]
    public TextMeshPro AwaitBuildingThisTMPro;

    [Header("Single VE Control")] 
    [Tooltip("Какое время длятся одиночные эффект здания")] public int lifeTime;

    #region Методы

    private void Awake()
    {
        OnStartEvent?.Invoke();
    }

    /// <summary>
    /// Запускает ожидание окончания одиночного визуального эффекта здания
    /// </summary>
    /// <param name="visualEffect"></param>
    public async void AwaitEndSingleVE(VisualEffect visualEffect)
    {
        await Task.Delay(lifeTime*1000);
        
        visualEffect.Stop();
    }

    #endregion
}
