using System;
using System.Collections.Generic;
using Dialogues;
using TMPro;
using UnityEngine;

public class EntryLocationControl : MonoBehaviour
{
    [SerializeField] private GameEvent UpdateResourcesEvent;
    [SerializeField] private GameEvent BaseSongStartEvent;

    public Dialogue tutorialDialogue;
    
    public List<QuestOwner> startGameQuests = new List<QuestOwner>();

    public void InitizilizePLayer()
    {
        DialogueManager.OnEndTutorial += InitializatonQuest;
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
            if (isTutorialCompleted == 0 )
            {
                DialogueManager.LaunchDialogue?.Invoke(tutorialDialogue);
            }
            else
            {
                InitializatonQuest();
                return;
            }
        }
        else
        {
            PlayerPrefs.GetInt("TutorialCompleted", 0);
            DialogueManager.LaunchDialogue?.Invoke(tutorialDialogue);
        }
    }

    /// <summary>
    /// Инициализация первых квестов
    /// </summary>
    private void InitializatonQuest()
    {
        foreach (QuestOwner questOwner in startGameQuests)
        {
            questOwner.GiveQuest(CurrentPlayersDataControl.CurrentQuestController);
        }
    }

    private void OnDestroy()
    {
        DialogueManager.OnEndTutorial -= InitializatonQuest;
    }
}
