using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class PathsEditorWindow
{
    public static List<string> EntitiesPaths = new List<string>()
    {
        "Assets/Entities/EntitiesSOs/Players/Player1/Player1.asset",
        "Assets/Entities/EntitiesSOs/Players/Player2/Player2.asset",
        "Assets/Entities/EntitiesSOs/Players/Player3/Player3.asset"
    };

    public static List<string> PSDPaths = new List<string>()
    {
        "Assets/SaveData/playerSaveDatas/StartValues.asset",
        "Assets/SaveData/playerSaveDatas/Player1SaveData.asset",
        "Assets/SaveData/playerSaveDatas/Player2SaveData.asset",
        "Assets/SaveData/playerSaveDatas/Player3SaveData.asset"
    };
}

#if UNITY_EDITOR
public class CE_ResetPlayersData: EditorWindow
{
    [MenuItem("Custom Tools/Reset players data")]
    public static void ShowWindow()
    {
        GetWindow<CE_ResetPlayersData>("Reset players data window");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Reset JSON data", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reset"))
        {
            DeleteJsonFilesFromDirectory();
        }
        
        GUILayout.Space(20);
                
        GUILayout.Label("Reset PlayerSaveData", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset"))
        {
            ResetPlayerData();
        }
        
        GUILayout.Space(20);
        
        
        GUILayout.Label("Reset Entities Datas" , EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reset"))
        {
            ResetEntitiesDatas();
        }
    }
    
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

    private void ResetPlayerData()
    {
        PlayerSaveData dPSD = AssetDatabase.LoadAssetAtPath<PlayerSaveData>(PathsEditorWindow.PSDPaths[0]);
        for (int i = 0; i < PathsEditorWindow.PSDPaths.Count; i++)
        {
            PlayerSaveData PSD = AssetDatabase.LoadAssetAtPath<PlayerSaveData>(PathsEditorWindow.PSDPaths[i]);
            
            PSD.playerBuildings =  dPSD.playerBuildings;
            PSD.buildingsTransform = dPSD.buildingsTransform;
            PSD.BuildingDatas = dPSD.BuildingDatas;
            PSD.BuildingWorkersInformationList = dPSD.BuildingWorkersInformationList;
        }
    }

    private void ResetEntitiesDatas()
    {
        PlayerSaveData dPSD = AssetDatabase.LoadAssetAtPath<PlayerSaveData>(PathsEditorWindow.PSDPaths[0]);
        
        for (int i = 0; i < PathsEditorWindow.EntitiesPaths.Count; i++)
        {
            EntityID entityID = AssetDatabase.LoadAssetAtPath<EntityID>(PathsEditorWindow.EntitiesPaths[i]);
            PlayerSaveData PSD = AssetDatabase.LoadAssetAtPath<PlayerSaveData>(PathsEditorWindow.PSDPaths[i + 1]);
            
            entityID.Name = entityID.DefaultName;
            entityID._playerSaveData = PSD;
            entityID.DefaultPlayerSaveData = dPSD;
        }
    }

}

#endif
