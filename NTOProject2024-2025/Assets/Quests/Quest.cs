using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Описание свойств квеста
/// </summary>
[CreateAssetMenu(menuName = "QuestScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    public string Name
    {
        get
        {
            return name;
        }
    }

    //Имя квеста
    [SerializeField]private string name;
    
    //Ивент для завершения квеста
    public event Action<Quest> OnQuestCompleted;

    //Активен ли
    public bool Active { get; set; }

    //Завершен ли
    public bool Completed { get; private set; }

    //Описание
    [TextArea]
    public string QuestDescription { get; private set; }

    //Список целей квеста
    public List<Objective> objectives = new List<Objective>();
    
    //Текущая цель для этого квеста
    public Objective currentObjective;
    
    //Соответствующий ивент старта этого квеста
    //public GameEvent startQuestEvent;
    
    //Ивент срабатывающий при получении новой задачи в данном квесте
    //public GameEvent startNewObjection;

    //Метод для попытки завершения квеста, если все его необходимые цели выполнены
    public void TryEndQuest()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].Completed != true)
            {
                if (objectives[i].required)
                {
                    // Квест не завершен

                    currentObjective = objectives[i];
                    
                    //startNewObjection.TriggerEvent();
                    
                    return;
                }
            }
        }
        
        Completed = true;
        Active = false;

        OnQuestCompleted?.Invoke(this);
    }

    //У всех целей присваиение данного квеста родительским
    public void OnEnable()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].parentQuest = this;
        }

        currentObjective = objectives[0];
    }
}

