using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerMainMenu : MonoBehaviour
{
    [Header("GameEvents")]
    [SerializeField] private GameEvent StartGameButtonClickEvent;
    [SerializeField] private GameEvent SettingsButtonClickEvent;
    [SerializeField] private GameEvent ReturnToMainMenuEvent;
    [SerializeField] private GameEvent ReturnToPlayerChoicePanelEvent;
    [SerializeField] private GameEvent CreateNewPlayer1Event;
    [SerializeField] private GameEvent CreateNewPlayer2Event;
    [SerializeField] private GameEvent CreateNewPlayer3Event;
    [SerializeField] private GameEvent StartGameInChoiceCharacterPanel;
    [SerializeField] private GameEvent StartGameAfterCreatingCharacter;
    [SerializeField] private GameEvent StartTutorialEvent;
    
    [Header("Resources default values")]
    [SerializeField] private int StartValueIron;
    [SerializeField] private int StartValueEnergy;
    [SerializeField] private int StartValueFood;
    [SerializeField] private int StartValueCrioCrystal;
    
    [Header("Products default values")]
    [SerializeField] private PriceShopProduct StartValueApiaryShop;
    [SerializeField] private PriceShopProduct StartValueHoneyGunShop;
    [SerializeField] private PriceShopProduct StartValueMobileBaseShop;
    [SerializeField] private PriceShopProduct StartValueStorageShop;
    [SerializeField] private PriceShopProduct StartValueResidentialModuleShop;
    [SerializeField] private PriceShopProduct StartValueBreadwinnerShop;
    [SerializeField] private PriceShopProduct StartValuePierShop;
    
    public static EntityID WhichPlayerCreate;

    [Header("Settings")]
    [SerializeField] private VolumeSlider volumeMusic;
    [SerializeField] private VolumeSlider volumeEffect;
    [SerializeField] private TextMeshProUGUI ScreenModeText;
    [SerializeField] private TMP_InputField inputFieldNewNamePlayer1;
    [SerializeField] private TMP_InputField inputFieldNewNamePlayer2;
    [SerializeField] private TMP_InputField inputFieldNewNamePlayer3;
    private TMP_InputField currentIFNP;
    private string newPLayerName;
    [SerializeField] [TextArea] private string textFullScreen;
    [SerializeField] [TextArea] private string textWindowScreen;


    private void Start()
    {
        StartGameScreenMode();
    }

    /// <summary>
    /// Запускает игру по нажатию на кнопку старта
    /// </summary>
    public void StartGameClickButton()
    {
        Debug.Log("Нажата кнопка Старта игры");
        StartGameButtonClickEvent.TriggerEvent();
    }
    
    /// <summary>
    /// Открывает настройки
    /// </summary>
    public void SettingsClickButton()
    {
        Debug.Log("Нажата кнопка настроек");
        SettingsButtonClickEvent.TriggerEvent();
    }
    
    
    /// <summary>
    /// При загрузке главного меню, берем сохраненное значение ключа и устанавливаем режим игры
    /// Метод загружатеся в GameEventListener и исполняется при активации ивента MoveToMainMenuSceneEvent 
    /// </summary>
    public void StartGameScreenMode()
    {
        if (PlayerPrefs.HasKey("ScreenMode"))
        {
            if (Convert.ToBoolean(PlayerPrefs.GetInt("ScreenMode")))
            {
                Debug.Log("Установлен полноэкранный режим");
                Screen.SetResolution(Screen.width, Screen.height, true, 60);
                PlayerPrefs.SetInt("ScreenMode", 1);
                ScreenModeText.text = textFullScreen;
            }
            else
            {
                Debug.Log("Установлен оконный режим");
                Screen.SetResolution(Screen.width, Screen.height, false, 60);
                PlayerPrefs.SetInt("ScreenMode", 0);
                ScreenModeText.text = textWindowScreen;
            }
        }
        else
        {
            Debug.Log("Установлен полноэкранный режим");
            Screen.SetResolution(Screen.width, Screen.height, true);
            PlayerPrefs.SetInt("ScreenMode", 1);
            ScreenModeText.text = textFullScreen;
        }
    }
    
    /// <summary>
    /// Выход из игры
    /// </summary>
    public void QuitGame()
    {
        JSONSerializeManager.Instance.JSONSave();
        Application.Quit();
    }
    
    /// <summary>
    ///  Поменять режим экрана
    /// </summary>
    public void ChangeScreenMode()
    {
        int screenMode = PlayerPrefs.GetInt("ScreenMode");
        if (screenMode == 0)
        {
            Screen.SetResolution(Screen.width, Screen.height, true, 60);
            PlayerPrefs.SetInt("ScreenMode", 1);
            Debug.Log("Полноэкранный");
            ScreenModeText.text = textFullScreen;
        }
        else
        {
            Screen.SetResolution(Screen.width, Screen.height, false);
            PlayerPrefs.SetInt("ScreenMode", 0);
            Debug.Log("Оконный");
            ScreenModeText.text = textWindowScreen;
        }
    }
    /// <summary>
    /// Вернуться в основное главное меню
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Возврат в Главное меню");
        ReturnToMainMenuEvent.TriggerEvent(); 
    }
    
    /// <summary>
    /// Вернуться в меню выбора персонажа
    /// </summary>
    public void ReturnToPlayerChoicePanel()
    {
        Debug.Log("Возврат в меню выбора персонажа");
        ReturnToPlayerChoicePanelEvent.TriggerEvent(); 
    }
    
    /// <summary>
    /// Нажатие на кнопку создания 
    /// </summary>
    public void ChoiceNewPlayer(EntityID player)
    {
        if (player.Name == player.DefaultName)
        {
            Debug.Log("Создание нового персонажа");
            switch (player.thisPlayerID)
            {
                case 1:
                    CreateNewPlayer1Event.TriggerEvent();
                    currentIFNP = inputFieldNewNamePlayer1; 
                    break;
                case 2:
                    CreateNewPlayer2Event.TriggerEvent();
                    currentIFNP = inputFieldNewNamePlayer2; 
                    break;
                case 3:
                    CreateNewPlayer3Event.TriggerEvent();
                    currentIFNP = inputFieldNewNamePlayer3; 
                    break;
            }
            //ClickCreateNewPlayerButtonEvent.TriggerEvent(); 
            StartTutorialEvent.TriggerEvent();
        }
        else
        {
            LoadingCanvasController.Instance.LoadingCanvasNotTransparent.SetActive(true);
            Debug.Log("Вход в игру с существующим персонажем");
            WhichPlayerCreate = player;
            StartGameInChoiceCharacterPanel.TriggerEvent();
        }
        
    }
    
     /// <summary>
     /// Начать игру после создания персонажа
     /// </summary>
     public async void StartGameAfterCreateChoice()
     {
         Debug.Log("Нажата кнопка создания персонажа1");
         LoadingCanvasController.Instance.LoadingCanvasNotTransparent.SetActive(true);
         string newName = currentIFNP.text; 
         WhichPlayerCreate.Name = newName;
         await APIManager.Instance.CreatePlayer(WhichPlayerCreate, StartValueIron, StartValueEnergy,StartValueFood,StartValueCrioCrystal);

         string shopName = $"{newName}'sShop";
         await APIManager.Instance.CreateShop(WhichPlayerCreate, shopName, StartValueApiaryShop, StartValueHoneyGunShop,StartValueMobileBaseShop,StartValueStorageShop,StartValueResidentialModuleShop,StartValueBreadwinnerShop,StartValuePierShop);
         
         JSONSerializeManager.Instance.JSONSave();
         
         StartGameAfterCreatingCharacter.TriggerEvent();

         Debug.Log("Нажата кнопка создания персонажа2");
     }
     
     
     /// <summary>
     /// Назначить персонажа для создания
     /// </summary>
     public void ChangeActiveChoicePlayer(EntityID player)
     {
         WhichPlayerCreate = player;
     }
}
