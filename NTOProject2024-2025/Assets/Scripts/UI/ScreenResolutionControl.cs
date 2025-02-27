using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScreenResolutionControl : MonoBehaviour
{
    private Resolution[] _resolutions;
    private string[] strRes;
    private bool WhichScreenMode;
    private int WhichResolutionSaved;
    private Resolution currentResolution;
    [SerializeField] private TextMeshProUGUI currentResText;
    
    public void Initialization()
    {
        // Получаем разрешения экрана
        Resolution[] allResolutions = Screen.resolutions;

        // Убираем дубли по ширине и высоте
        _resolutions = allResolutions
            .GroupBy(res => new { res.width, res.height })
            .Select(group => group.First())
            .ToArray();

        // Преобразуем в строку для отображения в dropdown
        strRes = new string[_resolutions.Length];
        for (int i = 0; i < _resolutions.Length; i++)
        {
            strRes[i] = _resolutions[i].width + "x" + _resolutions[i].height;
        }

        // Читаем настройки из PlayerPrefs
        
        // Режим экрана
        WhichScreenMode = PlayerPrefs.HasKey("ScreenMode")
            ? Convert.ToBoolean(PlayerPrefs.GetInt("ScreenMode"))
            : true;

        // Какое резрешение экрана сохранено
        WhichResolutionSaved = PlayerPrefs.HasKey("ResolutionMode")
            ? PlayerPrefs.GetInt("ResolutionMode")
            : _resolutions.Length - 1;

        // Если первый раз в игре 
        if (!PlayerPrefs.HasKey("ResolutionMode"))
        {
            PlayerPrefs.SetInt("ResolutionMode", WhichResolutionSaved);
        }

        currentResText.text = strRes[WhichResolutionSaved];

        currentResolution = _resolutions[WhichResolutionSaved];
        
        // Применяем разрешение
        Screen.SetResolution(_resolutions[WhichResolutionSaved].width, _resolutions[WhichResolutionSaved].height, WhichScreenMode);
        
        Debug.Log(
            $"Инициализировано разрешение экрана: {currentResolution.width}x{currentResolution.height} --- Номер в массиве:{WhichResolutionSaved}");
    }

    /// <summary>
    /// Установка и сохранение нового разрешения экрана
    /// </summary>
    public void SetResolution()
    {
        currentResolution = _resolutions[WhichResolutionSaved];
        currentResText.text = strRes[WhichResolutionSaved];
        Screen.SetResolution(currentResolution.width, currentResolution.height, WhichScreenMode);
        PlayerPrefs.SetInt("ResolutionMode", WhichResolutionSaved);
        Debug.Log(
            $"Установлено новое разрешение экрана: {currentResolution.width}x{currentResolution.height} --- Номер в массиве:{WhichResolutionSaved}");
    }

    /// <summary>
    /// Листать разрешения экрана
    /// </summary>
    /// <param name="IsRight"></param>
    public void UpdateResolution(bool IsRight)
    {
        if (IsRight)
        {
            if (WhichResolutionSaved < _resolutions.Length - 1) // 
            {
                WhichResolutionSaved++;
                SetResolution();
            }
        }
        else
        {
            if (WhichResolutionSaved > 0) // 
            {
                WhichResolutionSaved--;
                SetResolution();
            }
        }
    }
}
