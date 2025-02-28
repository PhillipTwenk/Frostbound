using UnityEngine;
using System;
public class ExtremeConditionsManager : MonoBehaviour
{
    public bool IsSafe;
    private float timer;
    public GameEvent StartExtremeConditionsEvent;
    public GameEvent EndExtremeConditionsEvent;
    void Start(){
        
    }

    void Update(){
        if(!IsSafe){
            timer += Time.deltaTime;
            CurrentPlayersDataControl.WhichPlayerCreate.speed = 40f - timer/2;
            if(timer >= 120f){
                timer = 0;
            }
        } else {
            timer = 0;
        }
    }

    private void OnTriggerStay(Collider other) {
        if(other.tag == "BaseHemisphere"){
            IsSafe = true;
            EndExtremeConditionsEvent.TriggerEvent();
        }
    }
    private void OnTriggerExit(Collider other) {
        if(other.tag == "BaseHemisphere" && IsSafe){
            IsSafe = false;
            StartExtremeConditionsEvent.TriggerEvent();
        }
    }
}
