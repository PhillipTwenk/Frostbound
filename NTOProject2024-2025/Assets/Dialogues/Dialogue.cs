using System.Collections.Generic;
using UnityEngine;

namespace Dialogues
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogues/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        [Header("Info")] 
        [Tooltip("Название")] public string title;
        [Tooltip("Туториал")] public bool isTutorial;

        [Header("State")] 
        [Tooltip("Активен ли сейчас")] public bool isActive;
        [Tooltip("Завершен ли ")] public bool isCompleted;
        
        
        [Header("Phrases Info")] 
        [Tooltip("Все фразы данного диалога по порядку")] public List<Phrase> phrases;
    }
}
