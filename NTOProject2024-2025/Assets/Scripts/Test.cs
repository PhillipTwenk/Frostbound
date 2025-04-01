using System;
using System.Collections.Generic;
using APIControl.Global_Server_Event;
using Dialogues;
using UnityEngine;

public class Test : MonoBehaviour
{
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PlayerPrefs.SetInt("TutorialCompleted", 0);
        }
    }
}
