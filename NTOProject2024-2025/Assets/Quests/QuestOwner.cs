using UnityEngine;

public class QuestOwner : MonoBehaviour
{
    public Quest myQuest;

    public void GiveQuest(QuestController player)
    {
        player.ReceiveNewQuest(myQuest);
        //myQuest.startQuestEvent.TriggerEvent();
        //myQuest.startNewObjection.TriggerEvent();
    }
}