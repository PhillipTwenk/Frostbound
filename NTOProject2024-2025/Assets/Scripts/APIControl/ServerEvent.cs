using System;
using System.Collections.Generic;

[Serializable]
public class ServerEvent
{
    public string name;
    public string text;
    public int once_in_hours;
    public int duration_in_minutes;
    public string start_date_time;
    
    [NonSerialized] 
    public Action OnEventStart;
    [NonSerialized]
    public Action OnEventEnd;
    
    
}

[Serializable]
public class ServerEventList
{
    public List<ServerEvent> events;
}
