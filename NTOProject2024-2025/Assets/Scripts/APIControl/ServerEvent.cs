using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ServerEvent
{
    public string name;
    public string text;
    public int once_in_hours;
    public int duration_in_minutes;
    public string start_date_time;
}

[System.Serializable]
public class ServerEventList
{
    public List<ServerEvent> events;
}
