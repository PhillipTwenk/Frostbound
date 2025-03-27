using System;
using UnityEngine;
using UnityEngine.Events;

namespace APIControl.Global_Server_Event.Secondary_Scripts
{
    public class SnowBlizzardGEControl : MonoBehaviour
    {
        public string neededEventName;
        public static Action ChangeParametersSnowBlizzardGEEvent;
        public static Action RevertParametersSnowBlizzardGEEvent;
        private QuestOwner neededQuestOwner;
        public Quest neededQuest;
        public GameObject Flag;
        public GameObject Triggers;
        public UnityEvent activateVisual; 
        public UnityEvent deactivateVisual;

        private void OnEnable()
        {
            GlobalServerEventsManager.OnEventAdded += InitializeThisGlobalEvent;
            neededQuestOwner = GetComponent<QuestOwner>();
        }

        public void InitializeThisGlobalEvent(ServerEvent serverEvent)
        {
            if (serverEvent.name == neededEventName)
            {
                serverEvent.OnEventStart += ResourcesUpdateStartGE;
                serverEvent.OnEventStart += StartQuestFlyingAroundPointsStartGE;
                serverEvent.OnEventStart += () =>
                {
                    activateVisual?.Invoke();
                };
                
                serverEvent.OnEventEnd += RevertParametersSnowBlizzardGE;
                serverEvent.OnEventEnd += ForceEndQuest;
                serverEvent.OnEventEnd += () =>
                {
                    deactivateVisual?.Invoke();
                };
            }
            else
            {
                Debug.Log("Имя не подходит");
            }
        }

        public void ResourcesUpdateStartGE()
        {
            Debug.Log("Снижение параметров при срате глобального ивента");
            ChangeParametersSnowBlizzardGEEvent?.Invoke();
        }

        public void StartQuestFlyingAroundPointsStartGE()
        {
            neededQuestOwner.GiveQuest(CurrentPlayersDataControl.CurrentQuestController);
            Triggers.SetActive(true);
        }

        public void RevertParametersSnowBlizzardGE()
        {
            RevertParametersSnowBlizzardGEEvent?.Invoke();
        }

        public void ForceEndQuest()
        {
            Triggers.SetActive(false);
            
            if (neededQuest.completed)
            {
                Flag.SetActive(true);
            }
            else
            {
                foreach (var obj in neededQuest.objectives)
                {
                    obj.CompleteObjective();
                }
            }
        }
        
    }
}
