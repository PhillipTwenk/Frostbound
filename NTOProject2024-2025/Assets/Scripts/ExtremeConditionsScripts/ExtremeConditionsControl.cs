using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class ExtremeConditionsControl : MonoBehaviour
{
    public bool IsSafe;
    public List<GameObject> UnitsInDanger = new List<GameObject>();
    public List<float> UnitsInDangerValues = new List<float>();
    public GameEvent EndExtremeConditionsEvent;
    public GameEvent StartExtremeConditionsEvent;
    public GameObject DeathAudioSource;
    public AudioClip DeathMusic;
    public AudioClip DeathSound;
    void Start(){
        
    }

    public void Update(){
        if(!IsSafe && UnitsInDanger.Count > 0){
            Debug.Log("Dangerrrr!!!");
            float period = Time.deltaTime;
            for (int i = 0; i < UnitsInDangerValues.Count; i++){
                UnitsInDangerValues[i] = UnitsInDangerValues[i] + period;
            }
            foreach (var unit in UnitsInDanger){
                if(unit != null){
                    if(unit.tag == "ClickOnWorker"){
                        unit.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 20f;
                    } else {
                        unit.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 20f;
                    }

                    if(!DeathAudioSource.transform.GetChild(0).gameObject.GetComponent<AudioSource>().isPlaying && UnitsInDangerValues[UnitsInDanger.IndexOf(unit)] < 12f){
                        DeathAudioSource.transform.GetChild(0).gameObject.GetComponent<AudioSource>().Play();
                    } else if (UnitsInDangerValues[UnitsInDanger.IndexOf(unit)] >= 12f){
                        DeathAudioSource.transform.GetChild(0).gameObject.GetComponent<AudioSource>().Stop();
                        DeathAudioSource.transform.GetChild(1).gameObject.GetComponent<AudioSource>().Play();
                        EndExtremeConditionsEvent.TriggerEvent();
                        UnitsInDangerValues = new List<float>();
                        UnitsInDanger = new List<GameObject>();
                        GameObject DeadUnit = unit;
                        Destroy(DeadUnit);
                        IsSafe = true;
                    }
                }
            }
            // CurrentPlayersDataControl.WhichPlayerCreate.speed = 40f - timer/2;
        }
    }

    private void OnTriggerStay(Collider other) {
        if((other.tag == "Player" || other.tag == "ClickOnWorker") && UnitsInDanger == null){
            IsSafe = true;
            other.gameObject.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 90f;
            DeathAudioSource.transform.GetChild(0).gameObject.GetComponent<AudioSource>().Stop();
            DeathAudioSource.transform.GetChild(1).gameObject.GetComponent<AudioSource>().Stop();
            EndExtremeConditionsEvent.TriggerEvent();
        }
    }
    private void OnTriggerEnter(Collider other) {
        if((other.tag == "Player" || other.tag == "ClickOnWorker")){
            IsSafe = true;
            other.gameObject.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 90f;
            DeathAudioSource.transform.GetChild(0).gameObject.GetComponent<AudioSource>().Stop();
            DeathAudioSource.transform.GetChild(1).gameObject.GetComponent<AudioSource>().Stop();
            EndExtremeConditionsEvent.TriggerEvent();
            UnitsInDangerValues.Remove(UnitsInDanger.IndexOf(other.gameObject));
            UnitsInDanger.Remove(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other) {
        if(other.tag == "Player" || other.tag == "ClickOnWorker"){
            IsSafe = false;
            UnitsInDanger.Add(other.gameObject);
            UnitsInDangerValues.Add(0f);
            Debug.Log(UnitsInDanger[0]);
            Debug.Log(UnitsInDangerValues[0]);
            StartExtremeConditionsEvent.TriggerEvent();
        }
    }
}
