using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Скрипт, меняющий режим на оффлайн, следящий за состоянием интернета и сервера, в случае чего переводит игру в оффлайн режим
/// </summary>
public class InternetMonitor : MonoBehaviour
{
    // Internet state control
    public static event Action OnInternetConnected; // Событие для подключения интернета
    private bool isInternetAvailable; // Текущее состояние интернета
    
    // Timeout control
    public static int FailedRequestLimit = 7; // Если достигнуто данное количество неудачных запросов подряд, игра переводится в оффлайн режим
    public static int CurrentNumberOfFailedRequest;
    
    //UI objects
    public GameObject NoInternetUI;
    public GameObject OfflineModeUI;
    
    // Offline mode control
    public static bool IsOfflineMode;
    
    private void OnEnable()
    {
        HTTPRequests.FailedRequestLimitExceededEvent += FailedRequestLimitExceeded;
    }

    private void OnDisable()
    {
        HTTPRequests.FailedRequestLimitExceededEvent -= FailedRequestLimitExceeded;
    }


    /// <summary>
    /// При привышении лимита неудачных запросов
    /// </summary>
    public void FailedRequestLimitExceeded()
    {
        Debug.LogWarning("Переход в оффлайн режим, похоже что Никита сломал сервер");
        IsOfflineMode = true;
        OfflineModeUI.SetActive(true);
    }
    
    /// <summary>
    ///  Для GameEvent 
    /// </summary>
    /// <param name="IsOn"></param>
    public void UpdateOfflineModeState(bool IsOn)
    {
        if (IsOn)
        {
            IsOfflineMode = true;
            PlayerPrefs.SetInt("OfflineMode", 1);
        }
        else
        {
            IsOfflineMode = false;
            CurrentNumberOfFailedRequest = 0;
            PlayerPrefs.SetInt("OfflineMode", 0);
        }
        
        OfflineModeUI.SetActive(IsOfflineMode);
    }
    private void Start()
    {
        // Инициализация состояния интернета
        isInternetAvailable = Application.internetReachability != NetworkReachability.NotReachable;
        
        // Проверка оффлайн режима
        if (PlayerPrefs.HasKey("OfflineMode"))
        {
            int OMvalue = PlayerPrefs.GetInt("OfflineMode");
            if (OMvalue == 1)
            {
                IsOfflineMode = true;
                Debug.Log("Оффлайн режим включен");
            }else if (OMvalue == 0)
            {
                IsOfflineMode = false;
                Debug.Log("Оффлайн режим отключен");
            }
            OfflineModeUI.SetActive(IsOfflineMode);
        }
        else
        {
            PlayerPrefs.SetInt("OfflineMode", 1);
            IsOfflineMode = true;
            OfflineModeUI.SetActive(true);
            Debug.Log("Оффлайн режим отключен, игрок играет в первый раз");
        }
        NoInternetUI.SetActive(!isInternetAvailable);
        
        //StartCoroutine(CheckInternetConnection());
    }

    private IEnumerator CheckInternetConnection()
    {
        while (true)
        {
            // Проверяем доступность интернета
            bool currentInternetState = Application.internetReachability != NetworkReachability.NotReachable;
            
            NoInternetUI.SetActive(!currentInternetState);
            
            if (!IsOfflineMode)
            {
                if (currentInternetState && !isInternetAvailable)
                {
                    Debug.Log("Интернет подключен!");
                    
                    NoInternetUI.SetActive(false);
                    isInternetAvailable = true;
                    IsOfflineMode = false;

                    // Вызываем событие подключения интернета
                    OnInternetConnected?.Invoke();
                }
                else if (!currentInternetState && isInternetAvailable)
                {      
                    Debug.Log("Интернет отключен!");
                
                    NoInternetUI.SetActive(true);
                    isInternetAvailable = false;
                    IsOfflineMode = true;
                }
            }
            
            // Проверка каждые 2 секунды
            yield return new WaitForSeconds(2f);
        }
    }
}