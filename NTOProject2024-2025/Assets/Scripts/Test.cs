using System;
using System.Collections.Generic;
using APIControl.Global_Server_Event;
using Dialogues;
using UnityEngine;

public class Test : MonoBehaviour
{
    public string neededEventName;

    private void OnEnable()
    {
        GlobalServerEventsManager.OnEventAdded += InitializeThisGlobalEvent;
    }

    public void InitializeThisGlobalEvent(ServerEvent serverEvent)
    {
        if (serverEvent.name == neededEventName)
        {
            serverEvent.OnEventStart += DebugTest;
        }
    }

    public void DebugTest()
    {
        Debug.Log("GLOBAL EVENT TETTTTTTT");
    }
    

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ServerEvent serverEvent = new ServerEvent();
            serverEvent.name = "Test";
            serverEvent.text = "This is a test.";
            serverEvent.once_in_hours = 3;
            serverEvent.duration_in_minutes = 1488;
            serverEvent.start_date_time = "2025-02-10T00:00:00";
            
            await APIManager.Instance.PostCreateServerEvent(serverEvent);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            ServerEvent serverEvent = new ServerEvent();
            serverEvent.name = "sss";
            serverEvent.text = "ssssssssssss";
            serverEvent.once_in_hours = 3;
            serverEvent.duration_in_minutes = 1488;
            serverEvent.start_date_time = "2025-02-10T00:00:00";
            
            await APIManager.Instance.PostCreateServerEvent(serverEvent);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            List<ServerEvent> serverEvents = await APIManager.Instance.GetServerEventList();
            foreach (var s in serverEvents)
            {
                Debug.Log(s.name);
            }
        }
    }
}
