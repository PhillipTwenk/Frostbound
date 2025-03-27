using System.Collections.Generic;
using UnityEngine;

namespace APIControl.Global_Server_Event.Local_Save
{
    [CreateAssetMenu(fileName = "LocalEventSaveData", menuName = "GlobalServerEvents/LocalEventSaveData")]
    public class LocalEventSaveData : ScriptableObject, ISerializableSO
    {
        #region Реализация ISerializableSO

        public string SerializeToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
    
        public void DeserializeFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        #endregion
    
        public List<ServerEvent> activeServerEvents;
        public ServerEvent currentServerEvent;
    
    }
}
