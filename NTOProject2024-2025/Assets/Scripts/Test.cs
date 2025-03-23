using Dialogues;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Dialogue sss;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && !DialogueManager.IsDialogueInProcess)
        {
            DialogueManager.LaunchDialogue?.Invoke(sss);
        }
    }
}
