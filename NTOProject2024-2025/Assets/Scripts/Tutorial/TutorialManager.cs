using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static bool IsTutorialActive;
    private bool CanEnterM;
    private bool CanEnterT;
    private Action TextUpdateEvent;
    public static Action UpdateTutorialStateEvent;
    private TutorialObjective currentTutorialObjective;
    private int i;
    public static bool IsTutorialTimeStop;
    private bool isTextWriting; // Флаг, который указывает, идет ли печать текста

    [Header("Quest Info")]
    public List<TutorialObjective> TutorialObjectives;
    public QuestController BearQC;
    private QuestOwner questOwner;

    [Header("UI elements")]
    public TextMeshProUGUI MainUIPanel;
    public TextMeshProUGUI TechnicalUIPanel;
    public TextMeshProUGUI TitleText;
    public GameObject FadeFone;

    [Header("Parameters")]
    public float charactersPerSecond;
    public List<Vector3> PositionTypes;

    [Header("GameEvents")]
    [SerializeField] private GameEvent ClosePlanMenu;

    [Header("Audio")]
    [SerializeField] private GameEvent StartTutorialSong;
    [SerializeField] private GameEvent StartGameSong;

    private void Awake()
    {
        questOwner = GetComponent<QuestOwner>();
        UpdateTutorialStateEvent += () => UpdateTutorialState();
    }

    private void Update()
    {
        if (Input.GetButtonDown("TutorialUpdate") && IsTutorialActive && currentTutorialObjective.IsAutomaticalyEnterOnThisStep)
        {
            if (isTextWriting) // Если идет печать текста
            {
                ShowFullText(MainUIPanel, currentTutorialObjective.TextOnThisObjective); // Показать текст полностью
                ShowFullText(TechnicalUIPanel, currentTutorialObjective.TechnicalTExtOnThisObjective);
                return;
            }

            if (CanEnterM)
            {
                if (currentTutorialObjective.IsTechPanelActiveOnThisStep)
                {
                    if (CanEnterT)
                    {
                        Debug.Log("SkibidiINC");
                        UpdateTutorialStateEvent?.Invoke();
                    }
                }
                else
                {
                    Debug.Log("SkibidiINC");
                    UpdateTutorialStateEvent?.Invoke();
                }
            }
        }
    }

    public void UpdateTutorialState()
    {
        Debug.Log("SKIBIDIDOPDOPDOPDOPDOPDOP");
        i++;
        if (i == TutorialObjectives.Count)
        {
            EndTutorial();
            return;
        }
        if ((i - 1) >= 0)
        {
            currentTutorialObjective.IsActive = false;
        }
        currentTutorialObjective = TutorialObjectives[i];
        currentTutorialObjective.IsActive = true;

        TextUpdateEvent?.Invoke();
    }

    public void StartTutorial()
    {
        Debug.Log("Туториал начат");
        i = 0;
        IsTutorialActive = true;
        MainUIPanel.transform.parent.gameObject.SetActive(true);
        currentTutorialObjective = TutorialObjectives[i];
        TextUpdateEvent += () => UIPanelUpdate(MainUIPanel, currentTutorialObjective);
        TextUpdateEvent += () => UIPanelUpdate(TechnicalUIPanel, currentTutorialObjective);
        TextUpdateEvent?.Invoke();
        currentTutorialObjective.IsActive = true;
        StartTutorialSong.TriggerEvent();
    }

    public void EndTutorial()
    {
        IsTutorialActive = false;
        MainUIPanel.transform.parent.gameObject.SetActive(false);
        TechnicalUIPanel.transform.parent.gameObject.SetActive(false);
        TextUpdateEvent -= () => UIPanelUpdate(MainUIPanel, currentTutorialObjective);
        TextUpdateEvent -= () => UIPanelUpdate(TechnicalUIPanel, currentTutorialObjective);
        currentTutorialObjective.IsActive = false;
        Time.timeScale = 1f;
        IsTutorialTimeStop = false;
        FadeFone.SetActive(false);
        StartGameSong.TriggerEvent();
    }

    private async void UIPanelUpdate(TextMeshProUGUI UIPanel, TutorialObjective currentTutorialObjective)
    {
        Debug.Log("Панель обновляется");
        CanEnterM = false;
        CanEnterT = false;
        TitleText.text = currentTutorialObjective.Title;

        if (currentTutorialObjective.IsTimeStopOnThisStep)
        {
            WorkersInterBuildingControl.possiilityControlEntities = false;
            FadeFone.SetActive(true);
            Time.timeScale = 0f;
            IsTutorialTimeStop = true;
        }
        else
        {
            WorkersInterBuildingControl.possiilityControlEntities = true;
            FadeFone.SetActive(false);
            Time.timeScale = 1f;
            IsTutorialTimeStop = false;
        }

        if (currentTutorialObjective.IsClosePlanMenuOnThisStep)
        {
            ClosePlanMenu.TriggerEvent();
        }

        if (UIPanel == TechnicalUIPanel)
        {
            if (currentTutorialObjective.IsTechPanelActiveOnThisStep)
            {
                UIPanel.transform.parent.gameObject.SetActive(true);
                UIPanel.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = PositionTypes[currentTutorialObjective.TechUIPositionTypeOnThisStep];
                await TextWriter(UIPanel, currentTutorialObjective.TechnicalTExtOnThisObjective);
                CanEnterT = true;
            }
            else
            {
                UIPanel.transform.parent.gameObject.SetActive(false);
            }
        }
        else
        {
            UIPanel.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = PositionTypes[currentTutorialObjective.MainUIPositionTypeOnThisStep];
            await TextWriter(UIPanel, currentTutorialObjective.TextOnThisObjective);
            CanEnterM = true;
        }
    }

    private async Task TextWriter(TextMeshProUGUI UIPanel, string Text)
    {
        isTextWriting = true; // Устанавливаем флаг печати текста
        string textBuffer = null;
        UIPanel.text = textBuffer;
        foreach (var c in Text)
        {
            if (!isTextWriting) // Если процесс печати был прерван
            {
                UIPanel.text = Text;
                break;
            }

            textBuffer += c;
            UIPanel.text = textBuffer;
            await Task.Delay((int)(1000 / charactersPerSecond));
        }

        isTextWriting = false; // Печать текста завершена
        Debug.Log("Текст полностью напечатан");
    }

    private void ShowFullText(TextMeshProUGUI UIPanel, string fullText)
    {
        isTextWriting = false; // Останавливаем процесс печати текста
        UIPanel.text = fullText; // Устанавливаем полный текст
    }
}
