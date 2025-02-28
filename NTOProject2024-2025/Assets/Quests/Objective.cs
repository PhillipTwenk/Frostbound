using UnityEngine;

/// <summary>
/// Описание свойств цели определенного квеста
/// Предоставляет метод для завершения цели
/// </summary>
[CreateAssetMenu(menuName = "QuestScriptableObjects/Objective")]
public class Objective : ScriptableObject
{
    //!!! При старте игры обнуляет прохождение выполненных целей, не учитывает сохранения игры в билде
    public void OnEnable()
    {
        this.Completed = false;
    }
    //
    
    //Родительский квест
    public Quest parentQuest;

    //Необходим ли
    public bool required = true;

    //Завершен ли
    public bool Completed { get; set; }

    //Расположение цели
    // public Transform waypoint;

    //Описание
    [TextArea]
    public string description;

    /// <summary>
    /// Заврешение данной цели
    /// </summary>
    public void CompleteObjective()
    {
        if ((parentQuest.currentObjective == this || !required) && parentQuest.Active)
        {
            Debug.Log($"===========Цель №{parentQuest.objectives.IndexOf(this)} - [{this.description}] в квесте {parentQuest.QuestDescription} выполненa==========");
            Completed = true;
            parentQuest.TryEndQuest();
        }
    }
}
