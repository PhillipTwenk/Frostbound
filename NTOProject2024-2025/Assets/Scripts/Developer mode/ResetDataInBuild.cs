using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unitilities;
using UnityEngine;

public class ResetDataInBuild : MonoBehaviour
{
    public TextMeshProUGUI devText;
    private void DeleteJsonFilesFromDirectory()
    {
        string folderPath = Application.persistentDataPath;

        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath, "*.json");

            if (files.Length > 0)
            {
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"Файл удалён: {file}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Ошибка при удалении файла {file}: {e.Message}");
                    }
                }
            }
            else
            {
                Debug.Log("Нет JSON файлов для удаления.");
            }
        }
        else
        {
            Debug.LogError("Папка не существует: " + folderPath);
        }
    }

    private void DevNotification(float await)
    {
        devText.gameObject.SetActive(true);
        devText.text = "Данные очищены";
        Utility.Invoke(this, () =>
        {
            devText.gameObject.SetActive(false);
        }, await);
    }


    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.N) && Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.O))
        {
            DeleteJsonFilesFromDirectory();
            DevNotification(5f);
        }
    }
}
