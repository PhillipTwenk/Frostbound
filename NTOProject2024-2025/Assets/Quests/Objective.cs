using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Описание свойств цели определенного квеста
/// Предоставляет метод для завершения цели
/// </summary>
[CreateAssetMenu(menuName = "QuestScriptableObjects/Objective")]
public class Objective : ScriptableObject
{
    // //!!! При старте игры обнуляет прохождение выполненных целей, не учитывает сохранения игры в билде
    // public void OnEnable()
    // {
    //     this.completed = false;
    // }
    // //
    
    [Tooltip("Родительский квест")] public Quest parentQuest;
    
    [Tooltip("Необходим ли")] public bool required = true;
    
    [Tooltip("Завершен ли")] public bool completed;

    [Tooltip("Имя цели")] public string Name;
    
    [Tooltip("Описание")] [TextArea] public string description;

    /// <summary>
    /// Заврешение данной цели
    /// </summary>
    public void CompleteObjective()
    {
        if ((parentQuest.currentObjective == this || !required) && parentQuest.active)
        {
            Debug.Log($"===========Цель №{parentQuest.objectives.IndexOf(this)} - [{this.description}] в квесте {parentQuest.questDescription} выполненa==========");
            completed = true;
            parentQuest.TryEndQuest();
        }
    }
}
