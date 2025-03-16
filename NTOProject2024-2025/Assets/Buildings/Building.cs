using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building")]
public class Building : ScriptableObject
{
    public int IDoB;

    public BuildingsTypes buildingType;
    
    [SerializeField] private List<int> levelListLevel;

    public int MaxLevelThisBuilding;
    
    [SerializeField] private List<int> durabilityListLevel;
    
    [SerializeField] private List<int> energyHoneyConsumptionListLevel;

    [SerializeField] private List<ResourceData> resourceProductionListLevel;
    
    [SerializeField] private List<ResourceData> storageListLevel;
    
    public int priceBuilding; // Стоимость здания ( металл )
    public int priceUpgrade;//Стоимость улучшения
    public int MBLevelForUpgradethisIron; //Минимальный уровень улучшения данного здания, =0 если улучшаем мобильную базу
    
    public int MBLevelForBuidlingthisIron; // Минимальный уровень мобильной базы для постройки данного здания, = 0 если строим Мобильную базу (Металл)
    
    public float TimeAwaitBuildingThis; //Cколько нужно ждать для завершения строительства данного здания

    [SerializeField] private GameObject prefabBuilding; // Префаб строения
    [SerializeField] private GameObject prefabBeforeBuilding; // Префаб триггера перед поставновкой здания
    
    /// <summary>
    /// Возвращает значение уровня
    /// </summary>
    /// <param name="levelMobileBase"></param>
    /// <returns></returns>
    public int Level(int levelMobileBase)
    {
        foreach (var level in levelListLevel)
        {
            if (level  == levelMobileBase)
            {
                return levelListLevel[level - 1];
            }
        }

        return levelListLevel[0];
    }

    /// <summary>
    /// Возвращает значение лимита локальных ресурсов в зависимости от уровня базы
    /// </summary>
    /// <param name="levelMobileBase"></param>
    /// <returns></returns>
    public ResourceData StorageLimit(int levelMobileBase)
    {
        foreach (var level in levelListLevel)
        {
            if (level  == levelMobileBase)
            {
                return storageListLevel[level - 1];
            }
        }
        return storageListLevel[0];
    }
    
    /// <summary>
    /// Возвращает значение медопотребления в зависимости от уровня базы
    /// </summary>
    /// <param name="levelMobileBase"></param>
    /// <returns></returns>
    public int EnergyHoneyConsumpiton(int levelMobileBase)
    {
        
        foreach (var level in levelListLevel)
        {
            if (level  == levelMobileBase)
            {
                return energyHoneyConsumptionListLevel[level - 1];
            }
        }

        return energyHoneyConsumptionListLevel[0];
    }
    
    /// <summary>
    /// Возвращает значение Прочности в зависимости от уровня базы
    /// </summary>
    /// <param name="levelMobileBase"></param>
    /// <returns></returns>
    public int Durability(int levelMobileBase)
    {
        foreach (var level in levelListLevel)
        {
            if (level  == levelMobileBase)
            {
                return durabilityListLevel[level - 1];
            }
        }

        return durabilityListLevel[0];
    }
    
    /// <summary>
    /// Возвращает значение производства в зависимости от уровня базы
    /// </summary>
    /// <param name="levelMobileBase"></param>
    /// <returns></returns>
    public ResourceData Production(int levelMobileBase)
    {
        foreach (var level in levelListLevel)
        {
            if (level  == levelMobileBase)
            {
                return resourceProductionListLevel[level - 1];
            }
        }

        return resourceProductionListLevel[0];
    }

    public GameObject PrefabBuilding
    {
        get => prefabBuilding;
    }
    
    public GameObject PrefabBeforeBuilding
    {
        get => prefabBeforeBuilding;
    }
}
