using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private GameEvent EndMoveToSceneLocationEvent;
    [SerializeField] private string MainLocationSceneName;
    [SerializeField] private string MainMenuSceneName;
    [SerializeField] private string UISceneName;
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha0))
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
        }
    }
    
    /// <summary>
    /// Метод перехода на основную сцену
    /// </summary>
    public void MoveToSceneLocation()
    {
        StartCoroutine(MoveToSceneLocationCoroutine());
    }
    
    /// <summary>
    /// Корутина, реализующая загрузку сцен и запускающая ивент окончания перехода на первую сцену
    /// </summary>
    private IEnumerator MoveToSceneLocationCoroutine()
    {
        EntityID ActivePlayer = UIManagerMainMenu.WhichPlayerCreate;
        // //Панель загрузки
        // LoadingCanvas.SetActive(true);
        
        Debug.Log("----- Переход на игровую сцену -----");
        
        //Загрузка уровня
        AsyncOperation LoadingSceneLocation = 
            SceneManager.LoadSceneAsync(MainLocationSceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(()=>LoadingSceneLocation.isDone);
        
        CurrentPlayersDataControl.WhichPlayerCreate = ActivePlayer;

        //Установка уровня как основной сцены
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(MainLocationSceneName));
        

        //Проверка на активность нужных сцен, и их выгрузка
        bool isSceneMainMenuActive = SceneManager.GetSceneByName(MainMenuSceneName).isLoaded;
        if (isSceneMainMenuActive)
        {
            Debug.Log("Выгрузка сцены главного меню");
            AsyncOperation UnloadingMainMenu = SceneManager.UnloadSceneAsync(MainMenuSceneName);
            Debug.Log("закончена");
            //yield return new WaitUntil(()=>UnloadingMainMenu.isDone);
        }
        
        
        //Проверка, если UI цена не активна, загружаем её
        bool isSceneUIActive = SceneManager.GetSceneByName(UISceneName).isLoaded;
        if (!isSceneUIActive)
        {
            Debug.Log("Выгрузка сцены UI");
            AsyncOperation UILoadingScene = SceneManager.LoadSceneAsync(UISceneName, LoadSceneMode.Additive);
            yield return new WaitUntil(()=>UILoadingScene.isDone);
            Debug.Log("закончена");
        }
        
        //LoadingCanvas.SetActive(false);
        EndMoveToSceneLocationEvent.TriggerEvent();
    }
}
