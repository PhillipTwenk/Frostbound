using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace GlobalEvents.Cataclysm_Services.Secondary
{
    public class DroneChangeVisualListener : MonoBehaviour
    {
        public List<GameObject> materialsGO = new List<GameObject>();
        public Material brokenMaterial;
        public Material defaultMaterial;
        public VisualEffect brokenVisualEffect;

        private void OnEnable()
        {
            DroneCrashGlobalEventService.ChangeDroneVisual += ChangeAllMaterials;
            DroneCrashGlobalEventService.ChangeDroneVisual += ActivateVE;
            
            DroneCrashGlobalEventService.RevertDroneVisual += RevertAllBrokenMaterial;
            DroneCrashGlobalEventService.RevertDroneVisual += DeactivateVE;
        }

        private void OnDisable()
        {
            DroneCrashGlobalEventService.ChangeDroneVisual -= ChangeAllMaterials;
            DroneCrashGlobalEventService.ChangeDroneVisual -= ActivateVE;
            
            DroneCrashGlobalEventService.RevertDroneVisual -= RevertAllBrokenMaterial;
            DroneCrashGlobalEventService.RevertDroneVisual -= DeactivateVE;
        }


        public void ChangeAllMaterials()
        {
            foreach (var obj in materialsGO)
            {
                obj.GetComponent<MeshRenderer>().material = brokenMaterial;
            }
        }

        public void RevertAllBrokenMaterial()
        {
            foreach (var obj in materialsGO)
            {
                obj.GetComponent<MeshRenderer>().material = defaultMaterial;
            }
        }

        public void ActivateVE()
        {
            brokenVisualEffect.gameObject.SetActive(true);
            brokenVisualEffect.Play();
        }

        public void DeactivateVE()
        {
            brokenVisualEffect.Stop();
            brokenVisualEffect.gameObject.SetActive(false);
        }
        
    }
}
