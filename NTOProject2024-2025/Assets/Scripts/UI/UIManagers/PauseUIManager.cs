using System;
using TMPro;
using UnityEngine;

public class PauseUIManager : MonoBehaviour
{
    [SerializeField] private GameEvent PauseOnEvent;
    [SerializeField] private GameEvent PauseOffEvent;
    [SerializeField] private GameEvent ClickSettingsPauseEvent;
    [SerializeField] private GameEvent ArriveToPauseMenuEvent;

    [SerializeField] private GameObject PausePanel;
    
    [SerializeField] private TextMeshProUGUI ScreenModeText;
    [SerializeField] [TextArea] private string textFullScreen;
    [SerializeField] [TextArea] private string textWindowScreen;

    public void PauseOn()
    {
        Time.timeScale = 0f;
        PauseOnEvent.TriggerEvent();
        WorkersInterBuildingControl.possiilityControlEntities = false;
        UIManager.CancelLastOpenPanelEvent += PauseOff;
    }

    public void PauseOff()
    {
        Debug.Log($"<color=yellow> Вернулись в игру из паузы </color>");
        Time.timeScale = 1f;
        PauseOffEvent.TriggerEvent();
        WorkersInterBuildingControl.possiilityControlEntities = true;
        UIManager.CancelLastOpenPanelEvent -= PauseOff;
    }

    public void ClickSettingsPause()
    {
        ClickSettingsPauseEvent.TriggerEvent();
        UIManager.CancelLastOpenPanelEvent += ArriveToPauseMenu;
    }

    public void ArriveToPauseMenu()
    {
        Debug.Log($"<color=yellow> Вернулись в меню паузы </color>");
        ArriveToPauseMenuEvent.TriggerEvent();
        UIManager.CancelLastOpenPanelEvent -= ArriveToPauseMenu;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            PauseResume();
        }
    }
    
    public void PauseResume()
    {
        if (!PausePanel.activeSelf)
        {
            Debug.Log("Пауза");
            PauseOn();
        }
        else
        {
            Debug.Log("Продолжаем");
            if (TutorialManager.IsTutorialTimeStop)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
            PauseOff();
        }
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
    
    
    public async void QuitGame()
    {
        await JSONSerializeManager.Instance.JSONSave();
        Application.Quit();
        Debug.Log("Quit");
    }
}
