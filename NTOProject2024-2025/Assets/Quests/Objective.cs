using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Описание свойств цели определенного квеста
/// Предоставляет метод для завершения цели
/// </summary>
[CreateAssetMenu(menuName = "QuestScriptableObjects/Objective")]
public class Objective : ScriptableObject, ISerializableSO
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
    
    
    // //!!! При старте игры обнуляет прохождение выполненных целей, не учитывает сохранения игры в билде
    // public void OnEnable()
    // {
    //     this.completed = false;
    // }
    // //
    
    [Tooltip("Родительский квест")] public Quest parentQuest;
    
    [Tooltip("Необходим ли")] public bool required = true;
    
    [Tooltip("Завершен ли")] public bool completed;

    [Tooltip("Имя цели")] public string name;
    
    [Tooltip("Описание")] [TextArea] public string description;

    public event Action OnObjectiveComplete;  

    /// <summary>
    /// Заврешение данной цели
    /// </summary>
    public void CompleteObjective()
    {
        if ((parentQuest.currentObjective == this || !required) && parentQuest.active)
        {
            Debug.Log($"<color=green>===========Цель №{parentQuest.objectives.IndexOf(this)} - [{this.description}] в квесте {parentQuest.Name} выполненa==========</color>");
            completed = true;
            OnObjectiveComplete?.Invoke();
            parentQuest.TryEndQuest();
        }
    }
}
