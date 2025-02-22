using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.VFX;

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
    
    [Header("Components")]
    public TextMeshPro AwaitBuildingThisTMPro;
    public VisualEffect BuildingVE;

    [Header("Function of this building")] [Tooltip("Функционал взаимодействия данного здания")]
    public UnityEvent StartBuildingFunctionEvent;

    private void Start()
    {
        if (IsThisBuilt)
        {
            BuildingVE.Stop();
        }
    }

    /// <summary>
    /// Контроль текста над зданием
    /// </summary>
    /// <param name="IsOpen"> Появление/сокрытие текста </param>
    /// <param name="WhichAction"> Какой текст</param>
    public void TextPanelBuildingControl(bool IsOpen, string WhichText)
    {
        if (IsOpen)
        {
            AwaitBuildingThisTMPro.gameObject.SetActive(IsOpen);

            AwaitBuildingThisTMPro.text = WhichText;
        }
        else
        {
            AwaitBuildingThisTMPro.gameObject.SetActive(IsOpen);
        }
    }
}
