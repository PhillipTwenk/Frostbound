using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ServerEvent
{
    public string name;
    public string description;
    public int intervalInHours;
    public int durationInMinutes;
    public string startDateTime;
}

[System.Serializable]
public class ServerEventList
{
    public List<ServerEvent> events;
}
