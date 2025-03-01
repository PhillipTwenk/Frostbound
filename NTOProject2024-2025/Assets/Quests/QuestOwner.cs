using UnityEngine;

public class QuestOwner : MonoBehaviour
{
    public Quest myQuest;

    public void GiveQuest(QuestController player)
    {
        player.ReceiveNewQuest(myQuest);
        Debug.Log($"<color=green> Квест {myQuest.Name} выдан игроку</color>");
    }
}