using System;
using UnityEngine;

public class YouCantBuildHereTrigger : MonoBehaviour
{
    public Material material;

    public LayerMask greenTriggers;
    public LayerMask redTriggers;

    private void Start()
    {
        material.color = Color.green;
        BuildingManager.Instance.CanBuilding = true;
    }

    private void OnTriggerStay(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        if (redTriggers == (redTriggers | (1 << otherLayer)))
        {
            material.color = Color.red;
            BuildingManager.Instance.CanBuilding = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        if (redTriggers == (redTriggers | (1 << otherLayer)))
        {
            material.color = Color.green;
            BuildingManager.Instance.CanBuilding = true;
        }
    }
}
