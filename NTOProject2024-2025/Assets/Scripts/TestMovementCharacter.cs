using UnityEngine;

public class TestMovementCharacter : MonoBehaviour
{
    public QuestOwner thisQuestOwner;
    private void Start()
    {
        thisQuestOwner = GetComponent<QuestOwner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log(CurrentPlayersDataControl.CurrentQuestController.name);
            thisQuestOwner.GiveQuest(CurrentPlayersDataControl.CurrentQuestController);
        }
    }
}
