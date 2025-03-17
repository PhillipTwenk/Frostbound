using System;
using UnityEngine;

public class DronePlaceEMTriggerCheck : MonoBehaviour
{
    public EngineeringModule engineeringModule;

    private void Start()
    {
        engineeringModule.isSpawnPointFree = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ClickOnWorker") || other.CompareTag("Worker"))
        {
            engineeringModule.isSpawnPointFree = false;
            Debug.Log($"Триггер активирован - engineeringModule.isSpawnPointFree = {engineeringModule.isSpawnPointFree}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ClickOnWorker") || other.CompareTag("Worker"))
        {
            engineeringModule.isSpawnPointFree = true;
            Debug.Log($"Площадка расчищена - engineeringModule.isSpawnPointFree = {engineeringModule.isSpawnPointFree}");
        }
    }
}
