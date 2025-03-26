using System.Collections.Generic;
using Dialogues;
using UnityEngine;

public class Test : MonoBehaviour
{
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ServerEvent serverEvent = new ServerEvent();
            serverEvent.name = "Test";
            serverEvent.description = "This is a test.";
            serverEvent.intervalInHours = 3;
            serverEvent.durationInMinutes = 1488;
            serverEvent.startDateTime = "2025-02-10T00:00:00";
            
            await APIManager.Instance.PostCreateServerEvent(serverEvent);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            List<ServerEvent> serverEvents = await APIManager.Instance.GetServerEventList();
            foreach (var s in serverEvents)
            {
                Debug.Log(s.name);
            }
        }
    }
}
