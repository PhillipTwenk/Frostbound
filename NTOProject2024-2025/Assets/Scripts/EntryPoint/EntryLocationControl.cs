using System;
using Dialogues;
using TMPro;
using UnityEngine;

public class EntryLocationControl : MonoBehaviour
{
    [SerializeField] private GameEvent UpdateResourcesEvent;
    [SerializeField] private GameEvent BaseSongStartEvent;

    public Dialogue tutorialDialogue;

    public void InitizilizePLayer()
    {
        UpdateResourcesEvent.TriggerEvent();

        PlayerSaveData pLayerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        pLayerSaveData.InitializeData();
        BaseSongStartEvent.TriggerEvent();
        
        LoadingCanvasController.Instance.LoadingCanvasNotTransparent.SetActive(false);

        StartTutorialControl();
    }

    private void StartTutorialControl()
    {
        if (PlayerPrefs.HasKey("TutorialCompleted"))
        {
            int isTutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted");
            if (isTutorialCompleted == 0)
            {
                DialogueManager.LaunchDialogue?.Invoke(tutorialDialogue);
            }
            else
            {
                return;
            }
        }
        else
        {
            PlayerPrefs.GetInt("TutorialCompleted", 0);
            DialogueManager.LaunchDialogue?.Invoke(tutorialDialogue);
        }
    }
}
