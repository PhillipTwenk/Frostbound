using UnityEngine;

[CreateAssetMenu(menuName = "Plan")]
public class Plan : ScriptableObject
{
    public string Title;

    public BuildingsTypes buildingType;
    
    [TextArea] public string Description;

    public string durability;
    public string energyHoneyConsumption;
    public string resourceProduction;

    public GameObject PlanPrefab;

    public Building buildingSO;

    public Sprite planSprite;
}

// public class NewPlan()
// {
//     public string Title;
//     public string Description;
//     public string Durability;
//     public string EnergyHoneyConsumption;
//     public string ResourceProduction;
//     public Sprite PlanSprite;
// }
