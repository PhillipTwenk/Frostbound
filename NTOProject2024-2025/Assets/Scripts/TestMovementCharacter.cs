using UnityEngine;

public class TestMovementCharacter : MonoBehaviour
{
    private QuestOwner thisQuestOwner;
    private void Start()
    {
        thisQuestOwner = GetComponent<QuestOwner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            thisQuestOwner.GiveQuest(CurrentPlayersDataControl.CurrentQuestController);
        }
    }
}
