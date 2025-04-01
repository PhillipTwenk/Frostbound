using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogues
{
    [Serializable]
    public class DialogueSaveData
    {
        public bool isActive;
        public bool isCompleted;
    }
    
    
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogues/Dialogue")]
    public class Dialogue : ScriptableObject, ISerializableSO
    {

        #region Сохранение в JSON

        /// <summary>
        /// Реализация ISerializableSO
        /// </summary>
        /// <returns></returns>
        public string SerializeToJson()
        {
            DialogueSaveData dialogueSaveData = new DialogueSaveData();
            dialogueSaveData.isActive = isActive;
            dialogueSaveData.isCompleted = isCompleted;
            return JsonUtility.ToJson(dialogueSaveData, true);
        }

        public void DeserializeFromJson(string json)
        {
            DialogueSaveData dialogueSaveData = new DialogueSaveData();
            JsonUtility.FromJsonOverwrite(json, dialogueSaveData);
            isActive = dialogueSaveData.isActive;
            isCompleted = dialogueSaveData.isCompleted;
        }


        #endregion
        
        
        [Header("Info")] 
        [Tooltip("Название")] public string title;
        [Tooltip("Туториал")] public bool isTutorial;

        [Header("State")] 
        [Tooltip("Активен ли сейчас")] public bool isActive;
        [Tooltip("Завершен ли ")] public bool isCompleted;
        
        
        [Header("Phrases Info")] 
        [Tooltip("Все фразы данного диалога по порядку")] public List<Phrase> phrases;

        #region Методы

        /// <summary>
        /// Запуск данного диалога
        /// </summary>
        public void LaunchDialogue()
        {
            DialogueManager.LaunchDialogue?.Invoke(this);
        }

        #endregion
    }
}
