using System;
using System.Collections.Generic;
using Dialogues;
using TMPro;
using UnityEngine;

public class EntryLocationControl : MonoBehaviour
{
    [SerializeField] private GameEvent UpdateResourcesEvent;
    [SerializeField] private GameEvent BaseSongStartEvent;
    public GameEvent StartMainGameGameEvent;

    public Dialogue tutorialDialogue;
    
    public List<QuestOwner> startGameQuests = new List<QuestOwner>();

    public async void InitizilizePLayer()
    {
        DialogueManager.OnEndTutorial += InitializationQuest;
        UpdateResourcesEvent.TriggerEvent();

        PlayerSaveData pLayerSaveData = CurrentPlayersDataControl.Instance.WhichPlayerDataUse();
        await pLayerSaveData.InitializeData();
        StartTutorialControl();
        BaseSongStartEvent.TriggerEvent();
        
        LoadingCanvasController.Instance.LoadingCanvasNotTransparent.SetActive(false);
        
        Debug.Log("Инициализация игрока закончена");
    }

    /// <summary>
    /// Завршен ли туториал, если нет или не был начат, запускаем заново
    /// </summary>
    public void StartTutorialControl()
    {
        EntityID player = CurrentPlayersDataControl.WhichPlayerCreate;

        Debug.Log($"Состояние туториала у текущего игрока: {player.isTutorialComplete}");
        if (!player.isTutorialComplete)
        {
            Debug.Log("Инициализация туториала");
            DialogueManager.LaunchDialogue?.Invoke(tutorialDialogue);
        }
        else
        {
            Debug.Log("Инициализация первичных квестов");
            StartMainGameGameEvent.TriggerEvent();
            InitializationQuest();
        }
    }

    /// <summary>
    /// Инициализация первых квестов
    /// </summary>
    public void InitializationQuest()
    {
        foreach (QuestOwner questOwner in startGameQuests)
        {
            questOwner.GiveQuest(CurrentPlayersDataControl.CurrentQuestController);
        }
    }

    private void OnDestroy()
    {
        DialogueManager.OnEndTutorial -= InitializationQuest;
    }
}
