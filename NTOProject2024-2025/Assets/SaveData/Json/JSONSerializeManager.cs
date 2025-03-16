using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class JSONSerializeManager : MonoBehaviour
{
    public static JSONSerializeManager Instance { get; set; }
    
    [Header("Scriptable Objects")]
    [Tooltip("Информация об игроках")]public List<EntityID> entitiesScriptableObjects;
    [Tooltip("Информация о сохраненных игровых данных игроков")]public List<PlayerSaveData> psdScriptableObjects;
    [Tooltip("Информация о квестах")]public List<Quest> questsScriptableObjects;
    [Tooltip("Информация о целях квестов")]public List<Objective> objectivesScriptableObjects;
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
        foreach (EntityID so in entitiesScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (PlayerSaveData so in psdScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (Quest so in questsScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
        foreach (Objective so in objectivesScriptableObjects)
        {
            AwakeJSONLoadFunctional(so);
        }
    }

    private void OnApplicationQuit()
    {
        JSONSave();
    }

    public void JSONSave()
    {
        lock (_lock)
        {
            playerPrefsSaveMethods?.Invoke();
            streamingDataSaveEvent?.Invoke();
            
            
            foreach (EntityID so in entitiesScriptableObjects)
            {
                JSONSaveFunctional(so);
            }
            foreach (PlayerSaveData so in psdScriptableObjects)
            {
                JSONSaveFunctional(so);
            }
            foreach (Quest so in questsScriptableObjects)
            {
                JSONSaveFunctional(so);
            }
            foreach (Objective so in objectivesScriptableObjects)
            {
                JSONSaveFunctional(so);
            }
        }
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
    /// Реализация функционала для сохранения JSON файлов
    /// </summary>
    /// <param name="so"></param>
    private void JSONSaveFunctional(ScriptableObject so)
    {
        if (so is ISerializableSO serializableSO)
        {
            try
            {
                string json = serializableSO.SerializeToJson();
                string filePath = Path.Combine(savePath, $"{so.name}.json");
                File.WriteAllText(filePath, json);
                Debug.Log($"Сохранено {so.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка сохранения {so.name}: {ex.Message}");
            }
        }
    }
}
