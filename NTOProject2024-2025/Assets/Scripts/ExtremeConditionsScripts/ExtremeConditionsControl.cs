using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityActions.WorkersScripts;

public class ExtremeConditionsControl : MonoBehaviour{
    public GameEvent StartExtremeConditionsEvent;
    public GameEvent EndExtremeConditionsEvent;
    public GameEvent SafeZoneConditions;
    private bool IsSafe = true;
    private float DeathTimer;
    private NavMeshAgent agent;
    private UnitType TypeOfUnit;
    void Start(){
        if (gameObject.GetComponent<WorkerData>())
        {
            TypeOfUnit = gameObject.GetComponent<WorkerData>().unitType;
        }
        else if (gameObject.GetComponent<PlayerSaveDataController>())
        {
            TypeOfUnit = UnitType.Player;
        }
        
        agent = GetComponent<NavMeshAgent>();
    }
    void Update(){
        if (!IsSafe){ // если вне зоны
            DeathTimer += Time.deltaTime;
            GetComponent<NavMeshAgent>().speed = 20;
            if(DeathTimer >= 12f){
                EndExtremeConditionsEvent.TriggerEvent();
                if (gameObject.GetComponent<WorkerData>())
                {
                    DeleteWorker();  // здесь чел сдыхает ухихихи
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "BaseHemisphere"){
            Debug.Log("yeah");
            if (TypeOfUnit != UnitType.MainDrone){ // если это не дрон 
                StartExtremeConditionsEvent.TriggerEvent();
                IsSafe = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "BaseHemisphere" && !IsSafe){
            Debug.Log("yeah");
            IsSafe = true;
            SafeZoneConditions.TriggerEvent();
            DeathTimer = 0f;
            GetComponent<NavMeshAgent>().speed = 90;
        }
    }

    /// <summary>
    /// Метод удаления рабочего и информации о нем
    /// </summary>
    private async Task DeleteWorker()
    {
        WorkerData workerData = gameObject.GetComponent<WorkerData>();
        int saveListIndex = workerData.SaveListIndex;
        PlayerSaveData playerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        playerSaveData.workers.RemoveAt(saveListIndex);
        playerSaveData.workersTransform.RemoveAt(saveListIndex);
        playerSaveData.workerDatas.RemoveAt(saveListIndex);

        GeneralWorkersControl.Instance.CurrentValueOfWorkers--;
        GeneralWorkersControl.Instance.CurrentValueOfUnits--;

        await JSONSerializeManager.Instance.JSONSave();
        
        Destroy(gameObject.transform.parent.gameObject); 
    }
}

    