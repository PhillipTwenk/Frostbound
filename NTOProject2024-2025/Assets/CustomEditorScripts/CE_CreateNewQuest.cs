using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Пути для создания файлов квеста и его целей
/// </summary>
public static class QuestPathsEditorWindow
{
    public static string objectivesFolderName = "Objectives";
    public static string UIPrefabFolderName = "UI";
    
    
    public static string PathQuestFolder = "Assets/Quests/QuestSOs";
    public static string PathUiPrefab = "Assets/Quests/BaseQuestUIPanel.prefab";
}



#if UNITY_EDITOR
public class CE_CreateNewQuest: EditorWindow
{
    private Vector2 scrollPos1;
    private Vector2 scrollPos2;
    private Vector2 scrollPos3;
    private Vector2 allScrollPos;
    
    
    private string textInputQuestName;
    private string textInputQuestFile;
    private string textInputQuestDescription;
    private List<string> QuestObjectivesNames = new List<string>() { "" };
    private List<string> QuestObjectivesAndHisDescriptions = new List<string>() { "" };
    private List<bool> QuestObjectivesIsRequired = new List<bool>() { true };

    private string currentQuestFolderPath = "";
    private string currentQuestObjectivesFolderPath = "";
    private string currentQuestUIPrefabFolderPath = "";
    private Quest currentQuest;
    
    [MenuItem("Custom Tools/Create new quest")]
    public static void ShowWindow()
    {
        GetWindow<CE_CreateNewQuest>("Create new quest window");
    }

    private void OnGUI()
    {
        allScrollPos = EditorGUILayout.BeginScrollView(allScrollPos);

        #region Настройки квеста

        #region Имена квеста

        GUILayout.Label("Enter new quest name (file)", EditorStyles.boldLabel);
        textInputQuestFile = EditorGUILayout.TextField("File quest name:", textInputQuestFile);
        
        GUILayout.Space(10);
        
        GUILayout.Label("Enter new quest name ( for UI )", EditorStyles.boldLabel);
        textInputQuestName = EditorGUILayout.TextField("Quest name:", textInputQuestName);
        

        #endregion
        
        GUILayout.Space(10);

        #region Описание квеста

        GUILayout.Label("Enter new quest description", EditorStyles.boldLabel);
        textInputQuestDescription = EditorGUILayout.TextArea(textInputQuestDescription, GUILayout.Height(30));

        #endregion


        #endregion
        
        GUILayout.Space(30);

        #region Настройки целей квеста

        #region Имя цели квеста

        GUILayout.Label("Add Objections names ( for UI )", EditorStyles.boldLabel);
        
        if (GUILayout.Button("+ Add Objective", GUILayout.Height(20)))
        {
            QuestObjectivesNames.Add(""); // Добавляем новый пустой элемент
            QuestObjectivesAndHisDescriptions.Add("");
            QuestObjectivesIsRequired.Add(false);
        }
        
        GUILayout.Space(10);
        
        scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, GUILayout.Height(50));
        
        for (int i = 0; i < QuestObjectivesNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            QuestObjectivesNames[i] = EditorGUILayout.TextField(QuestObjectivesNames[i]); // Поле ввода
            if (GUILayout.Button("-", GUILayout.Width(30))) // Кнопка удаления элемента
            {
                QuestObjectivesNames.RemoveAt(i);
                QuestObjectivesAndHisDescriptions.RemoveAt(i);
                QuestObjectivesIsRequired.RemoveAt(i);
                break; // Выходим из цикла после удаления
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();


        #endregion

        #region Описание квеста

        GUILayout.Label("Objective's description ( Automatic synchronization )");
        
        GUILayout.Space(10);
        
        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.Height(100));
        
        for (int i = 0; i < QuestObjectivesAndHisDescriptions.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            QuestObjectivesAndHisDescriptions[i] = EditorGUILayout.TextArea(QuestObjectivesAndHisDescriptions[i], GUILayout.Height(50)); // Поле ввода
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        #endregion

        #region Необходим ли

        GUILayout.Label("Is this objective required ( Automatic synchronization )");
        
        GUILayout.Space(10);
        
        scrollPos3 = EditorGUILayout.BeginScrollView(scrollPos3, GUILayout.Height(50));
        
        for (int i = 0; i < QuestObjectivesIsRequired.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            QuestObjectivesIsRequired[i] = EditorGUILayout.Toggle(QuestObjectivesIsRequired[i]); // Поле ввода
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        #endregion

        #endregion

        GUILayout.Space(15);
        
        #region Создать квест

        if (GUILayout.Button("Сreate quest", GUILayout.Height(20)))
        {
            CreateFoldersHierarhy(out currentQuestFolderPath, out currentQuestObjectivesFolderPath, out currentQuestUIPrefabFolderPath, textInputQuestFile);
            CreateFileQuest(currentQuestFolderPath, out currentQuest);
            currentQuest = CreateObjectivesFiles(currentQuestObjectivesFolderPath, currentQuest);
            CreateUIPrefab(currentQuestUIPrefabFolderPath, currentQuest);

            currentQuestFolderPath = "";
            currentQuestObjectivesFolderPath = "";
            currentQuestUIPrefabFolderPath = "";
            currentQuest = null;
        }

        #endregion
        
        EditorGUILayout.EndScrollView(); // Завершаем ScrollView
    }

    #region Методы для создания квеста

    /// <summary>
    /// Создать первичную организацию папок для квеста и его целей
    /// </summary>
    /// <param name="path"></param>
    private void CreateFoldersHierarhy(out string path, out string objectivesPath, out string uiPrefabPath, string questFileName)
    {
        path = $"{QuestPathsEditorWindow.PathQuestFolder}/{questFileName}";
        objectivesPath = $"{path}/{QuestPathsEditorWindow.objectivesFolderName}";
        uiPrefabPath = $"{path}/{QuestPathsEditorWindow.UIPrefabFolderName}";
        
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Ошибка: Путь не может быть пустым!");
            return;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path);
            string folderName = Path.GetFileName(path);
            
            if (!AssetDatabase.IsValidFolder(parent))
            {
                Debug.LogError($"Ошибка: Родительская папка '{parent}' не существует!");
                return;
            }
            AssetDatabase.CreateFolder(parent, folderName);
            
            // Дочерние папки
            AssetDatabase.CreateFolder(path,QuestPathsEditorWindow.objectivesFolderName);
            AssetDatabase.CreateFolder(path,QuestPathsEditorWindow.UIPrefabFolderName);
            //
            
            AssetDatabase.Refresh();
            Debug.Log($"Папки созданы: {path}");
        }
        else
        {
            Debug.LogWarning($"Папка уже существует: {path}");
        }
    }

    /// <summary>
    /// Создание файла квеста и его параметров
    /// </summary>
    /// <param name="path"></param>
    private void CreateFileQuest(string path, out Quest quest)
    {
        Quest newQuest = ScriptableObject.CreateInstance<Quest>();

        newQuest.name = textInputQuestFile;
        newQuest.Name = textInputQuestName;
        newQuest.questDescription = textInputQuestDescription;

        quest = newQuest;

        string pathThis = $"{path}/{newQuest.name}.asset";
        
        AssetDatabase.CreateAsset(newQuest, pathThis);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Quest created at: " + path);
    }
    
    /// <summary>
    /// Создание файлов целей и загрузка их в нужную папку
    /// </summary>
    /// <param name="path"></param>
    private Quest CreateObjectivesFiles(string path, Quest quest)
    {
        for (int i = 0; i < QuestObjectivesNames.Count; i++)
        {
            Objective newObjective = ScriptableObject.CreateInstance<Objective>();

            newObjective.name = $"[{quest.name}] - Obj{i+1}";
            newObjective.Name = QuestObjectivesNames[i];
            newObjective.description = QuestObjectivesAndHisDescriptions[i];
            newObjective.required = QuestObjectivesIsRequired[i];

            quest.objectives.Add(newObjective);
            
            newObjective.parentQuest = quest;
            
            string pathThis = $"{path}/{newObjective.name}.asset";
            AssetDatabase.CreateAsset(newObjective, pathThis);
        
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Objective created at: " + path);
        }

        return quest;
    }

    /// <summary>
    /// Создание UI префаба для панели квестов
    /// </summary>
    /// <param name="path"></param>
    private void CreateUIPrefab(string path, Quest quest)
    {
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuestPathsEditorWindow.PathUiPrefab);
        
        if (basePrefab == null)
        {
            Debug.LogError($"Префаб не найден по пути: {QuestPathsEditorWindow.PathUiPrefab}");
            return;
        }

        string newName = $"[{quest.name}] - UI Item";
        string thisPath = $"{path}/{newName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(basePrefab, thisPath);
        
        GameObject newGO = AssetDatabase.LoadAssetAtPath<GameObject>(thisPath);

        quest.UIItemOnQuestPanel = newGO;
    }

    #endregion
}

#endif
