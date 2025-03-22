using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;
public class ExtremeConditionsControl : MonoBehaviour{
    public GameEvent StartExtremeConditionsEvent;
    public GameEvent EndExtremeConditionsEvent;
    public GameEvent SafeZoneConditions;
    private bool IsSafe = true;
    private float DeathTimer;
    private NavMeshAgent agent;
    private UnitType TypeOfUnit;
    void Start(){
        TypeOfUnit = gameObject.GetComponent<WorkerData>().unitType;
        agent = GetComponent<NavMeshAgent>();
    }
    void Update(){
        if (!IsSafe){ // если вне зоны
            DeathTimer += Time.deltaTime;
            GetComponent<NavMeshAgent>().speed = 20;
            if(DeathTimer >= 12f){
                EndExtremeConditionsEvent.TriggerEvent();
                Destroy(gameObject.transform.parent.gameObject); // здесь чел сдыхает ухихихи
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
        if(other.tag == "BaseHemisphere"){
            Debug.Log("yeah");
            IsSafe = true;
            SafeZoneConditions.TriggerEvent();
            DeathTimer = 0f;
            GetComponent<NavMeshAgent>().speed = 90;
        }
    }
}

    