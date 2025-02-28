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

    /// <summary>
    /// Установление нового активного квеста
    /// </summary>
    /// <param name="quest"> Ссылка на квест </param>
    public void ReceiveNewQuest(Quest quest)
    {
        playerID.openQuests.Add(quest);
        quest.Active = true;
        quest.OnQuestCompleted += RemoveCompletedQuest;
    }

    /// <summary>
    /// Убрать завершенный квест
    /// </summary>
    /// <param name="quest"> Ссылка на квест </param>
    void RemoveCompletedQuest(Quest quest)
    {
        quest.OnQuestCompleted -= RemoveCompletedQuest;
        playerID.openQuests.Remove(quest);
    }

    /// <summary>
    /// При старте игры проверка завершенных квестов и их убирание из массива
    /// </summary>
    public void QuestInitialize()
    {
        playerID = CurrentPlayersDataControl.WhichPlayerCreate;
        for(int i = playerID.openQuests.Count; i>0; i--)
        {
            if (playerID.openQuests[i].Completed)
            {
                RemoveCompletedQuest(playerID.openQuests[i]);
            }
            else
            {
                playerID.openQuests[i].OnQuestCompleted += RemoveCompletedQuest;
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
