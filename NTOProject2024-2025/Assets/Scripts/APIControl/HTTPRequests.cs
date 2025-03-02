using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// Класс - менеджер, предоставляющий методы для работы с запросами
/// Получить ответ можно через onSucces -> responce
/// </summary>
public class HTTPRequests: MonoBehaviour
 { 
     public static HTTPRequests Instance { get; private set; }
     
     // Timeout Control
     public static event Action FailedRequestLimitExceededEvent;
     private const float Timeout = 5f;

     private void Awake()
     {
         Instance = this;
     }

     
     /// <summary>
    /// Выполнить GET запрос
    /// </summary>
    public void Get(string url, float timeoutValue, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendRequest(url, timeoutValue, UnityWebRequest.kHttpVerbGET, null, onSuccess, onError));
    }

    /// <summary>
    /// Выполнить POST запрос
    /// </summary>
    public void Post(string url, float timeoutValue, string jsonData, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendRequest(url, timeoutValue, UnityWebRequest.kHttpVerbPOST, jsonData, onSuccess, onError));
    }

    /// <summary>
    /// Выполнить PUT запрос
    /// </summary>
    public void Put(string url, float timeoutValue, string jsonData, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendRequest(url, timeoutValue,UnityWebRequest.kHttpVerbPUT, jsonData, onSuccess, onError));
    }

    /// <summary>
    /// Выполнить DELETE запрос
    /// </summary>
    public void Delete(string url, float timeoutValue, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendRequest(url, timeoutValue, UnityWebRequest.kHttpVerbDELETE, null, onSuccess, onError));
    }

    /// <summary>
    /// Общая корутина для выполнения запросов
    /// </summary>
    private IEnumerator SendRequest(string url, float timeoutValue, string method, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        float startTime = Time.realtimeSinceStartup;
        
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest();

            while (!request.isDone && !InternetMonitor.IsOfflineMode)
            {
                //Debug.Log($"Current request time: {Time.realtimeSinceStartup - startTime} 0.o");
                if (Time.realtimeSinceStartup - startTime > timeoutValue)
                {
                    if (InternetMonitor.CurrentNumberOfFailedRequest >= InternetMonitor.FailedRequestLimit)
                    {
                        FailedRequestLimitExceededEvent?.Invoke();
                    }
                    else
                    {
                        InternetMonitor.CurrentNumberOfFailedRequest++;
                        Debug.Log($"Неудачных запросов уже целых <color=red>{InternetMonitor.CurrentNumberOfFailedRequest}</color>");
                    }
                    
                    request.Abort();
                    onError?.Invoke(request.error);
                    Debug.LogError("Request aborted due to timeout ~UwU~");
                    yield break;
                }
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success && !InternetMonitor.IsOfflineMode)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
                InternetMonitor.CurrentNumberOfFailedRequest = 0;
            }
            else if (!InternetMonitor.IsOfflineMode)
            {
                string errorMessage = request.result == UnityWebRequest.Result.ConnectionError 
                    ? "Request aborted due to timeout ~UwU~"
                    : $"Request Error: {request.error}, URL: {url}, Execution time: {Time.realtimeSinceStartup - startTime} :3";
                onError?.Invoke(errorMessage);
            }
            else if (InternetMonitor.IsOfflineMode)
            {
                Debug.LogError($"Offline mode on :3");
                onError?.Invoke(request.error);
            }
        }
    }
 }
