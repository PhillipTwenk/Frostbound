using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    private QuestOwner thisQuestOwner;
    public Objective obj3;

    private void Start()
    {
        thisQuestOwner = GetComponent<QuestOwner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            thisQuestOwner.GiveQuest(CurrentPlayersDataControl.CurrentQuestController);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            obj3.CompleteObjective(); 
        }
    }
}
