using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using APIControl.Global_Server_Event.Local_Save;
using Dialogues;
using UnityEngine;
using UnityEngine.Serialization;

public class JSONSerializeManager : MonoBehaviour
{
    public static JSONSerializeManager Instance { get; set; }
    
    [Header("Scriptable Objects")]
    [Tooltip("Информация об игроках")] public List<EntityID> entitiesScriptableObjects;
    [Tooltip("Информация о сохраненных игровых данных игроков")] public List<PlayerSaveData> psdScriptableObjects;
    [Tooltip("Информация о квестах")] public List<Quest> questsScriptableObjects;
    [Tooltip("Информация о целях квестов")] public List<Objective> objectivesScriptableObjects;
    [Tooltip("Диалоги")] public List<Dialogue> dialoguesScriptableObjects;
    public LocalEventSaveData localEventSaveData;
    
    private string savePath;
    private static readonly object _lock = new object();

    public static Action playerPrefsSaveMethods;
    public static Action streamingDataSaveEvent;

    private void Awake()
    {
        Instance = this;
        savePath = Application.persistentDataPath;
        Debug.Log($"Куда сохранять JSON {savePath}");
    
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    public void AwakeJSONLoad()
    {
        foreach (PlayerSaveData so in psdScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (Objective so in objectivesScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (Quest so in questsScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (Dialogue so in dialoguesScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (EntityID so in entitiesScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        
        AwakeJSONLoadFunctional(localEventSaveData);
    }

    private async void OnApplicationQuit()
    {
        await JSONSave();
    }

    public async Task JSONSave()
    {
        lock (_lock)
        {
            playerPrefsSaveMethods?.Invoke();
            streamingDataSaveEvent?.Invoke();
        }

        List<Task> saveTasks = new List<Task>();
        foreach (EntityID so in entitiesScriptableObjects)
        {
            saveTasks.Add(JSONSaveFunctionalAsync(so));
        }
        foreach (Quest so in questsScriptableObjects)
        {
            saveTasks.Add(JSONSaveFunctionalAsync(so));
        }
        foreach (Objective so in objectivesScriptableObjects)
        {
            saveTasks.Add(JSONSaveFunctionalAsync(so));
        }
        foreach (PlayerSaveData so in psdScriptableObjects)
        {
            saveTasks.Add(JSONSaveFunctionalAsync(so));
        }
        foreach (Dialogue so in dialoguesScriptableObjects)
        {
            saveTasks.Add(JSONSaveFunctionalAsync(so));
        }
        
        saveTasks.Add(JSONSaveFunctionalAsync(localEventSaveData));

        await Task.WhenAll(saveTasks);
    }

    /// <summary>
    /// Реализация функционала для первичной загрузки JSON файлов
    /// </summary>
    /// <param name="so"></param>
    private void AwakeJSONLoadFunctional(ScriptableObject so)
    {
        if (so is ISerializableSO serializableSO)
        {
            Debug.Log("1) SO реализует");
            string filePath = Path.Combine(savePath, $"{so.name}.json");
            Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                Debug.Log("Существует");
                try
                {
                    string json = File.ReadAllText(filePath);
                    serializableSO.DeserializeFromJson(json);
                    Debug.Log($"Загружено {so.name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка загрузки {so.name}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Файл не найден: {filePath}. Создаём новый файл с текущими данными.");
                try
                {
                    string json = serializableSO.SerializeToJson();
                    File.WriteAllText(filePath, json);
                    Debug.Log($"Создан файл: {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка создания файла {so.name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Реализация функционала для асинхронного сохранения JSON файлов
    /// </summary>
    /// <param name="so"></param>
    private async Task JSONSaveFunctionalAsync(ScriptableObject so)
    {
        if (so is ISerializableSO serializableSO)
        {
            try
            {
                string json = serializableSO.SerializeToJson();
                string filePath = Path.Combine(savePath, $"{so.name}.json");
                await File.WriteAllTextAsync(filePath, json);
                Debug.Log($"Сохранено {so.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка сохранения {so.name}: {ex.Message}");
            }
        }
    }
}