using System;
using UnityEngine;

public class DroneTriggerCompleteObjective : MonoBehaviour
{
   public Objective neededObjective;

   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.CompareTag("ClickOnWorker"))
      {
         neededObjective.CompleteObjective();
      }
   }
}
