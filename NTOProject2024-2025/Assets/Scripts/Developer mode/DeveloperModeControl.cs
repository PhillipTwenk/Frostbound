using System;
using UnityEngine;

public class DeveloperModeControl : MonoBehaviour
{
   public bool isDeveloperMode;
   public static bool IsDeveloperMode;

   private void Awake()
   {
      IsDeveloperMode = isDeveloperMode;
   }
}
