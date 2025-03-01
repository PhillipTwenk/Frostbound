using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Описание свойств квеста
/// </summary>
[CreateAssetMenu(menuName = "QuestScriptableObjects/Quest")]
public class Quest : ScriptableObject, ISerializableSO
{
    /// <summary>
    /// Реализация ISerializableSO
    /// </summary>
    /// <returns></returns>
    public string SerializeToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void DeserializeFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
    
    
    
    
    
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }
    
    [Tooltip("Имя квеста")] [SerializeField]private string name;
    
    [Tooltip("Ивент для завершения квеста")]public event Action<Quest> OnQuestCompleted;
    
    [Tooltip("Активен ли")] public bool active;
    
    [Tooltip("Завершен ли")] public bool completed;
    
    [Tooltip("Описание")] [TextArea] public string questDescription;
    
    [Tooltip("Список целей квеста")] public List<Objective> objectives = new List<Objective>();
    
    [Tooltip("Текущая цель для этого квеста")] public Objective currentObjective;

    [Space] [Header("UI Control")] [Tooltip("Префаб пункта квеста на панели с квестами")]
    public GameObject UIItemOnQuestPanel;
    

    /// <summary>
    /// Метод для попытки завершения квеста, если все его необходимые цели выполнены
    /// </summary>
    public void TryEndQuest()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].completed != true)
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
        
        completed = true;
        active = false;

        OnQuestCompleted?.Invoke(this);
    }

    /// <summary>
    /// У всех целей присваиение данного квеста родительским
    /// </summary>
    public void OnEnable()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].parentQuest = this;
        }

        currentObjective = objectives[0];
    }
}

