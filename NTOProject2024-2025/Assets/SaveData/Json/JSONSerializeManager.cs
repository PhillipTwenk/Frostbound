using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSONSerializeManager : MonoBehaviour
{
    public static JSONSerializeManager Instance { get; set; }
    
    
    public List<ScriptableObject> serializableObjects;
    private string savePath;
    private static readonly object _lock = new object();

    public static Action playerPrefsSaveMethods;

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
        foreach (var so in serializableObjects)
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
    }

    private void OnApplicationQuit()
    {
        JSONSave();
    }

    public void JSONSave()
    {
        lock (_lock)
        {
            foreach (var so in serializableObjects)
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
            
            playerPrefsSaveMethods?.Invoke();
        }
    }
}
