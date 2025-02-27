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

    public void PauseOn() => PauseOnEvent.TriggerEvent();
    public void PauseOff() => PauseOffEvent.TriggerEvent();
    public void ClickSettingsPause() => ClickSettingsPauseEvent.TriggerEvent();
    public void ArriveToPauseMenu() => ArriveToPauseMenuEvent.TriggerEvent();

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
            Time.timeScale = 0f;
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
    
    
    public void QuitGame()
    {
        JSONSerializeManager.Instance.JSONSave();
        Application.Quit();
        Debug.Log("Quit");
    }
}
