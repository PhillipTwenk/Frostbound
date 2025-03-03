using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// =========================================================================================
/// Предоставление методов для работы с активными квестами
/// Висит на игроке
/// =========================================================================================
/// </summary>
public class QuestController : MonoBehaviour
{
    private EntityID playerID;
    public static event Action<Quest> OnStartNewQuest;
    public static event Action<List<Quest>> OnInitializationQuests;

    private void Awake()
    {
        CurrentPlayersDataControl.CurrentQuestController = this;
    }

    /// <summary>
    /// Установление нового активного квеста
    /// </summary>
    /// <param name="quest"> Ссылка на квест </param>
    public void ReceiveNewQuest(Quest quest)
    {
        playerID.openQuests.Add(quest);
        quest.active = true;
        quest.OnQuestCompleted += UIManager.Instance.RemoveQuestItemInQuestPanel;
        quest.OnQuestCompleted += RemoveCompletedQuest;
        OnStartNewQuest?.Invoke(quest);
    }

    /// <summary>
    /// Убрать завершенный квест
    /// </summary>
    /// <param name="quest"> Ссылка на квест </param>
    void RemoveCompletedQuest(Quest quest)
    {
        quest.OnQuestCompleted -= UIManager.Instance.RemoveQuestItemInQuestPanel;
        quest.OnQuestCompleted -= RemoveCompletedQuest;
        playerID.openQuests.Remove(quest);
        Debug.Log($"<color=green> Квест {quest.Name} Завершен </color>");
    }

    /// <summary>
    /// При старте игры проверка завершенных квестов и их убирание из массива
    /// </summary>
    public void QuestInitialize()
    {
        Debug.Log("Инициализация квестов");
        playerID = CurrentPlayersDataControl.WhichPlayerCreate;
        for(int i = playerID.openQuests.Count -  1; i>=0; i--)
        {
            Debug.Log($"Проверка квестов: {playerID.openQuests[i].Name}");
            if (playerID.openQuests[i].completed)
            {
                RemoveCompletedQuest(playerID.openQuests[i]);
            }
            else
            {
                playerID.openQuests[i].OnQuestCompleted += UIManager.Instance.RemoveQuestItemInQuestPanel;
                playerID.openQuests[i].OnQuestCompleted += RemoveCompletedQuest;
                OnInitializationQuests?.Invoke(playerID.openQuests);
            }

            if (i == 0)
            {
                break;
            }
        }
    }
    
    /// <summary>
    /// Отписка от ивента при окончании игры
    /// </summary>
    void OnDisable()
    {
        foreach (Quest quest in playerID.openQuests)
        {
            quest.OnQuestCompleted -= RemoveCompletedQuest;
        }
    }

}
