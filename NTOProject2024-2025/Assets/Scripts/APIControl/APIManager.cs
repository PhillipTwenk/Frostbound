using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APIControl.Semaphore;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Класс для получения нужных ссылок в зависимости от задачи
/// </summary>
public class Requests
{
    private const string UUID = "ad9eeae2-76a0-4074-86e5-cc77b967816d";
    public static string CreatePlayerURL = $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/";
    public static string GetPlayerURL(string playerName) =>  $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/";

    public static string PutPlayerURL(string playerName) => $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/";

    public static string DeletePlayerURL(string playerName) => $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/";

    public static string CreateShopURL(string playerName) => $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/shops/";

    public static string GetShopURL(string playerName, string shopName) => $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/shops/{shopName}/";

    public static string PutShopURL(string playerName, string shopName) => $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/shops/{shopName}/";

    public static string DeleteShopURL(string playerName, string shopName) => $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/players/{playerName}/shops/{shopName}/";
    public static string CreateServerEventURL = $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/events/";
    public static string GetListOfServerEvents = $"https://final.2025.nti-gamedev.ru/api/games/{UUID}/events/";
}

/// <summary>
/// Класс хранит значения максимального времени запроса для каждого вида запроса
/// </summary>
public class TimeoutValues
{
    public const float CreatePlayerTimeoutValue = 8f;
    public const float GetPlayerResourcesTimeoutValue = 2f;
    public const float PutPlayerResourcesTimeoutValue = 2f;
    public const float DeletePlayerTimeoutValue = 3f;
    public const float CreatePlayerLogTimeoutValue = 2f;
    
    public const float CreateShopTimeoutValue = 4f;
    public const float GetShopResourcesTimeoutValue = 2f;
    public const float PutShopResourcesTimeoutValue = 2f; 
    public const float DeleteShopTimeoutValue = 3f;
    public const float CreateShopLogTimeoutValue = 2f;
    
    public const float CreateServerEventTimeoutValue = 8f;
    public const float GetListOfServerEventsTimeoutValue = 5f;
    
}

/// <summary>
/// Класс для получения сообщений в логи
/// </summary>
public class LogComment
{
    public static string ChangedIronNaming = "Changed_Iron";
    public static string ChangedCryoCrystalNaming = "Changed_CryoCrystal";
    public static string ChangedEnergyNaming = "Changed_Energy";
    public static string ChangedFoodNaming = "Changed_Food";
}

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    
    private void OnEnable()
    {
        InternetMonitor.OnInternetConnected += HandleInternetConnected;
    }

    private void OnDisable()
    {
        InternetMonitor.OnInternetConnected -= HandleInternetConnected;
    }

    private async void HandleInternetConnected()
    {
        Debug.Log("Интернет снова доступен! Выполняю нужные действия...");

        EntityID playerID = CurrentPlayersDataControl.WhichPlayerCreate;
        await PutPlayerResources(playerID, playerID.playerResources.Iron, playerID.playerResources.Energy, playerID.playerResources.Food, playerID.playerResources.CryoCrystal);
            
        await PutShopResources(playerID, $"{playerID.entityName}'sShop", playerID.shopResources.Apiary, playerID.shopResources.MobileBase, playerID.shopResources.Storage, playerID.shopResources.ResidentialModule, playerID.shopResources.Minner, playerID.shopResources.Pier);
            // PlayerResources playerResources = await GetPlayerResources(UIManagerMainMenu.WhichPlayerCreate);
            // await PutPlayerResources(UIManagerMainMenu.WhichPlayerCreate, playerResources.Iron, playerResources.Energy, playerResources.Food, playerResources.CryoCrystal); 
    }

    #region Методы контроля игрока и его ресурсов 

    /// <summary>
        /// Создание персонажа
        /// </summary>
        /// <param name="playerName"> Имя персонажа</param>
        /// <param name="playerIron"> Количество металла </param>
        /// <param name="playerEnergy"> Количество энергии </param>
        /// <param name="playerFood"> Количество пищи </param>
        /// <param name="playerCrioCrystal"> Количество Криокристаллов </param>
        public async Task CreatePlayer(EntityID playerID, int playerIron, int playerEnergy, int playerFood, int playerCrioCrystal)
        {
            await SyncManager.Enqueue(async () =>
            {
                // Создаем объект PlayerData
                PlayerData playerData = new PlayerData()
                {
                    name = playerID.entityName,
                    resources = new PlayerResources()
                    {
                        Iron = playerIron,
                        Energy = playerEnergy,
                        Food = playerFood,
                        CryoCrystal = playerCrioCrystal
                    }
                };
                
                //Для оффлайн
                playerID.playerResources = playerData.resources;
        
                // Преобразуем в JSON
                string json = JsonUtility.ToJson(playerData, true);
        
                // Создаем TaskCompletionSource для ожидания ответа
                var taskCompletionSource = new TaskCompletionSource<bool>();
        
                if (!InternetMonitor.IsOfflineMode)
                {
                    // Выполняем POST-запрос
                    HTTPRequests.Instance.Post(Requests.CreatePlayerURL, TimeoutValues.CreatePlayerTimeoutValue, json,  
                        onSuccess: response =>
                        {
                            Debug.Log("Персонаж успешно создан");
                            taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                        },
                        onError: error =>
                        {
                            Debug.LogError($"Ошибка при создании персонажа: {error}");
                        
                            taskCompletionSource.SetResult(false); // Завершаем Task 
                        });
                }
                else
                {
                    Debug.Log("OfflineMode on - CreatePlayer");
                    taskCompletionSource.SetResult(true);
                }
        
                // Ждем завершения Task
                await taskCompletionSource.Task;
            });
            
        }
        
        
    
        
        /// <summary>
        /// Получение ресурсов игрока 
        /// </summary>
        /// <param name="playerName"> Имя игрока </param>
        /// <returns></returns>
        public async Task<PlayerResources> GetPlayerResources(EntityID playerID)          
        {
            string URL = Requests.GetPlayerURL(playerID.entityName);
    
            // Создаем TaskCompletionSource для ожидания результата запроса
            var taskCompletionSource = new TaskCompletionSource<PlayerResources>();
    
            if (!InternetMonitor.IsOfflineMode)
            {
                HTTPRequests.Instance.Get(URL, TimeoutValues.GetPlayerResourcesTimeoutValue, 
                    onSuccess: response =>
                    {
                        Debug.Log("Данные о ресурсах персонажа успешно получены");
                        PlayerData playerData = JsonUtility.FromJson<PlayerData>(response);
                        taskCompletionSource.SetResult(playerData.resources); // Устанавливаем результат
                    },
                    onError: error =>
                    {
    
                        Debug.LogError("Возникла ошибка при получении данных о персонаже: " + error);
                        taskCompletionSource.SetResult(playerID.playerResources);
                    });
            }
            else
            {
                Debug.Log("OfflineMode on - GetPlayerResources");
                taskCompletionSource.SetResult(playerID.playerResources);
            }
            
            
    
            // Ждем завершения Task и возвращаем результат
            return await taskCompletionSource.Task;
        }
        
        /// <summary>
        /// Загрузка новых ресурсов определенному игроку
        /// </summary>
        /// <param name="playerName"> Имя игрока </param>
        /// <param name="playerIron"> Металл </param>
        /// <param name="playerEnergy"> Энергомед </param>
        /// <param name="playerFood"> Еда </param>
        /// <param name="playerCrioCrystal"> Криосталы </param>
        public async Task PutPlayerResources(EntityID playerID, int playerIron, int playerEnergy, int playerFood, int playerCryoCrystal)
        {
            await SyncManager.Enqueue(async () =>
            {
                // Создаем объект PlayerData
                PlayerData playerData = new PlayerData()
                {
                    name = playerID.entityName,
                    resources = new PlayerResources()
                    {
                        Iron = playerIron,
                        Energy = playerEnergy,
                        Food = playerFood,
                        CryoCrystal = playerCryoCrystal
                    }
                };
        
                //Для оффлайн
                playerID.playerResources = playerData.resources;
                
                // Преобразуем объект в JSON
                string json = JsonUtility.ToJson(playerData, true);
                
                Debug.Log(json);
        
                // Формируем URL для PUT-запроса
                string URL = Requests.PutPlayerURL(playerID.entityName);
        
                // Создаем TaskCompletionSource для ожидания завершения запроса
                var taskCompletionSource = new TaskCompletionSource<bool>();
        
        
                if (!InternetMonitor.IsOfflineMode)
                {
                    // Выполняем PUT-запрос
                    HTTPRequests.Instance.Put(URL, TimeoutValues.PutPlayerResourcesTimeoutValue, json,
                        onSuccess: response =>
                        {
                            Debug.Log("Ресурсы персонажа успешно обновлены");
                            Debug.Log(response);
                            taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                        },
                        onError: error =>
                        {
                            Debug.LogError($"Ошибка при обновлении ресурсов персонажа: {error}");
                            taskCompletionSource.SetResult(false); // Завершаем Task
                        });
                }
                else
                {
                    Debug.Log("OfflineMode on - PutPlayerResources");
                    taskCompletionSource.SetResult(true);
                }
        
                // Ждем завершения Task
                await taskCompletionSource.Task;
            });
            
        }
    
    
        /// <summary>
        /// Удаление игрока
        /// </summary>
        /// <param name="playerName"></param>
        public async Task DeletePlayer(EntityID playerID)
        {
            await SyncManager.Enqueue(async () =>
            {
                // Формируем URL для удаления игрока
                string URL = Requests.DeletePlayerURL(playerID.entityName);
        
                // Создаем TaskCompletionSource для обработки результата запроса
                var taskCompletionSource = new TaskCompletionSource<bool>();
                
                if (!InternetMonitor.IsOfflineMode)
                {
                    // Выполняем DELETE-запрос
                    HTTPRequests.Instance.Delete(URL, TimeoutValues.DeletePlayerTimeoutValue,
                        onSuccess: response =>
                        {
                            Debug.Log("Персонаж успешно удален");
                            taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                        },
                        onError: error =>
                        {
                            Debug.LogError($"Ошибка при удалении персонажа: {error}");
                            
                            taskCompletionSource.SetResult(false); // Завершаем Task
                        });
        
                }
                else
                {
                    Debug.Log("OfflineMode on - DeletePlayer");
                    taskCompletionSource.SetResult(true);
                }
                
                
                // Ждем завершения Task
                await taskCompletionSource.Task;
            });
            
        }   

    #endregion

    #region Методы контроля магазина и его товаров
        
    /// <summary>
        /// Создает магазин
        /// </summary>
        /// <param name="playerID"> имя игрока </param>
        /// <param name="shopName"> имя магазина </param>
        /// <param name="apiaryShop"> чертеж пасеки </param>
        /// <param name="mobileBaseShop"> чертеж мобильной базы </param>
        /// <param name="storageShop"> чертеж хранилища </param>
        /// <param name="residentialModuleShop"> чертеж жилого модуля </param>
        /// <param name="breadwinnerShop"> чертеж добытчика</param>
        /// <param name="pierShop"> чертеж пристани </param>
        /// <returns></returns>
        public async Task CreateShop(EntityID playerID, string shopName, PriceShopProduct apiaryShop, PriceShopProduct mobileBaseShop, PriceShopProduct storageShop, PriceShopProduct residentialModuleShop, PriceShopProduct breadwinnerShop, PriceShopProduct pierShop)
        {
            await SyncManager.Enqueue(async () =>
            {
                // Создаем объект ShopData
                ShopData shopData = new ShopData()
                {
                    name = shopName,
                    resources = new ShopResources()
                    {
                        Apiary = apiaryShop,
                        MobileBase = mobileBaseShop,
                        Storage = storageShop,
                        ResidentialModule = residentialModuleShop,
                        Minner = breadwinnerShop,
                        Pier = pierShop
                    }
                };
        
                //Для оффлайн
                playerID.shopResources= shopData.resources;
                
                // Преобразуем в JSON
                string json = JsonUtility.ToJson(shopData, true);
        
                // Создаем TaskCompletionSource для ожидания ответа
                var taskCompletionSource = new TaskCompletionSource<bool>();
        
                if (!InternetMonitor.IsOfflineMode)
                {
                    // Выполняем POST-запрос
                    HTTPRequests.Instance.Post(Requests.CreateShopURL(playerID.entityName), TimeoutValues.CreateShopTimeoutValue,json, 
                        onSuccess: response =>
                        {
                            Debug.Log("Магазин персонажа успешно создан");
                            taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                        },
                        onError: error =>
                        {
                            Debug.LogError($"Ошибка при создании Магазина персонажа: {error}");
                            taskCompletionSource.SetResult(false); // Завершаем Task
                        });
                }
                else
                {
                    Debug.Log("OfflineMode on - CreateShop");
                    taskCompletionSource.SetResult(true);
                }
                
                
        
                // Ждем завершения Task
                await taskCompletionSource.Task;
            });
            
        }
    
        /// <summary>
        /// Получение данных о ресурсах магазина 
        /// </summary>
        /// <param name="playerID"> Имя игрока </param>
        /// <param name="shopName"> Имя магазина </param>
        /// <returns></returns>
        public async Task<ShopResources> GetShopResources(EntityID playerID, string shopName)
        {
            string URL = Requests.GetShopURL(playerID.entityName, shopName);
    
            // Создаем TaskCompletionSource для ожидания результата запроса
            var taskCompletionSource = new TaskCompletionSource<ShopResources>();
            
            if (!InternetMonitor.IsOfflineMode)
            {
                HTTPRequests.Instance.Get(URL, TimeoutValues.GetShopResourcesTimeoutValue,
                    onSuccess: response =>
                    {
                        Debug.Log("Данные о ресурсах магазина успешно получены");
                        ShopData shopData = JsonUtility.FromJson<ShopData>(response);
                        taskCompletionSource.SetResult(shopData.resources); // Устанавливаем результат
                    },
                    onError: error =>
                    {
                        Debug.LogError("Возникла ошибка при получении данных о магазине: " + error);
                        taskCompletionSource.SetResult(playerID.shopResources); // Устанавливаем результат 
                    });
            }
            else
            {
                Debug.Log("OfflineMode on - GetShopResources");
                taskCompletionSource.SetResult(playerID.shopResources);
            }
    
            // Ждем завершения Task и возвращаем результат
            return await taskCompletionSource.Task;
        }
    
        /// <summary>
        /// Обновляет ресурсы в магазине
        /// </summary>
        /// <param name="playerID"> имя игрока </param>
        /// <param name="shopName"> имя магазина </param>
        /// <param name="apiaryShop"> чертеж пасеки </param>
        /// <param name="mobileBaseShop"> чертеж мобильной базы </param>
        /// <param name="storageShop"> чертеж хранилища </param>
        /// <param name="residentialModuleShop"> чертеж жилого модуля </param>
        /// <param name="breadwinnerShop"> чертеж добытчика</param>
        /// <param name="pierShop"> чертеж пристани </param>
        /// <returns></returns>
        public async Task PutShopResources(EntityID playerID, string shopName, PriceShopProduct apiaryShop, PriceShopProduct mobileBaseShop, PriceShopProduct storageShop, PriceShopProduct residentialModuleShop, PriceShopProduct breadwinnerShop, PriceShopProduct pierShop)
        {
            await SyncManager.Enqueue(async () =>
            {
                // Создаем объект ShopData
                ShopData shopData = new ShopData()
                {
                    name = shopName,
                    resources = new ShopResources()
                    {
                        Apiary = apiaryShop,
                        MobileBase = mobileBaseShop,
                        Storage = storageShop,
                        ResidentialModule = residentialModuleShop,
                        Minner = breadwinnerShop,
                        Pier = pierShop
                    }
                };
        
                //Для оффлайн
                playerID.shopResources = shopData.resources;
                
                // Преобразуем в JSON
                string json = JsonUtility.ToJson(shopData, true);
        
                // Создаем TaskCompletionSource для ожидания ответа
                var taskCompletionSource = new TaskCompletionSource<bool>();
        
                if (!InternetMonitor.IsOfflineMode)
                {
                     // Выполняем PUT-запрос
                    HTTPRequests.Instance.Put(Requests.PutShopURL(playerID.entityName, shopName), TimeoutValues.PutShopResourcesTimeoutValue,json, 
                        onSuccess: response =>
                        {
                            Debug.Log("Магазин персонажа успешно обновлен");
                            taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                        },
                        onError: error =>
                        {
                            Debug.LogError($"Ошибка при обновлении Магазина персонажа: {error}");
                            taskCompletionSource.SetResult(false); // Завершаем Task
                        });
        
                }
                else
                {
                    Debug.Log("OfflineMode on - PutShopResources");
                    taskCompletionSource.SetResult(true);
                }
               
                // Ждем завершения Task
                await taskCompletionSource.Task;
            });
            
        }
    
        /// <summary>
        /// Удаляет магазин определенного игрока
        /// </summary>
        /// <param name="playerID"> имя игрока </param>
        /// <param name="shopName"> имя магазина </param>
        /// <returns></returns>
        public async Task DeleteShop(EntityID playerID, string shopName)
        {
            await SyncManager.Enqueue(async () =>
            {
                // Формируем URL для удаления игрока
                string URL = Requests.DeleteShopURL(playerID.entityName, shopName);
        
                // Создаем TaskCompletionSource для обработки результата запроса
                var taskCompletionSource = new TaskCompletionSource<bool>();
        
                if (!InternetMonitor.IsOfflineMode)
                {
                    // Выполняем DELETE-запрос
                    HTTPRequests.Instance.Delete(URL, TimeoutValues.DeleteShopTimeoutValue,
                        onSuccess: response =>
                        {
                            Debug.Log("Магазин успешно удален");
                            taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                        },
                        onError: error =>
                        {
                            Debug.LogError($"Ошибка при удалении магазина: {error}");
                            taskCompletionSource.SetResult(false); // Завершаем Task
                        });
                }
                else
                {
                    Debug.Log("OfflineMode on - DeleteShop");
                    taskCompletionSource.SetResult(true);
                }
                
                
                // Ждем завершения Task
                await taskCompletionSource.Task;
            });
            
        }
    #endregion

    #region Методы контроля ивентов

    /// <summary>
    /// Отправка запроа на сервер с целью создания глобального серверного ивента 
    /// </summary>
    /// <param name="serverEvent"></param>
    public async Task PostCreateServerEvent(ServerEvent serverEvent)
    {
        await SyncManager.Enqueue(async () =>
        {
            // Преобразуем в JSON
            string json = JsonUtility.ToJson(serverEvent, true);
            
            Debug.Log(json);
            
            // Создаем TaskCompletionSource для ожидания ответа
            var taskCompletionSource = new TaskCompletionSource<bool>();
        
            if (!InternetMonitor.IsOfflineMode)
            {
                // Выполняем POST-запрос
                HTTPRequests.Instance.Post(Requests.CreateServerEventURL, TimeoutValues.CreateServerEventTimeoutValue, json,  
                    onSuccess: response =>
                    {
                        Debug.Log("Глобальный ивент успешно создан");
                        taskCompletionSource.SetResult(true); // Завершаем Task успешным результатом
                    },
                    onError: error =>
                    {
                        Debug.LogError($"Ошибка при создании глобального ивента: {error}");
                        
                        taskCompletionSource.SetResult(false); // Завершаем Task 
                    });
            }
            else
            {
                Debug.Log("OfflineMode on - CreatePlayer");
                taskCompletionSource.SetResult(true);
            }
        
            // Ждем завершения Task
            await taskCompletionSource.Task;
        });
        
    }

    /// <summary>
    /// Получение списка глобальных ивентов 
    /// </summary>
    /// <returns></returns>
    public async Task<List<ServerEvent>> GetServerEventList()
    {
        var taskCompletionSource = new TaskCompletionSource<List<ServerEvent>>();
        string URL = Requests.GetListOfServerEvents;
        
        HTTPRequests.Instance.Get(URL, TimeoutValues.GetListOfServerEventsTimeoutValue,
            onSuccess: response =>
            {
                try
                {
                    // Парсим ответ
                    List<ServerEvent> servers = JsonUtility.FromJson<ServerEventList>($"{{\"events\":{response}}}").events;
                    
                    // Завершаем Task успешным результатом
                    taskCompletionSource.SetResult(servers);
                }
                catch (Exception ex)
                {
                    // Завершаем Task с ошибкой при возникновении исключения
                    Debug.LogError($"Ошибка при обработке данных: {ex.Message}");
                    taskCompletionSource.SetException(ex);
                }
            },
            onError: error =>
            {
                // Завершаем Task с ошибкой при проблемах с запросом
                Debug.LogError($"Ошибка запроса: {error}");
                taskCompletionSource.SetException(new Exception(error));
            });

        // Ждем завершения Task и возвращаем результат
        return await taskCompletionSource.Task;
    }
    #endregion
}
   
