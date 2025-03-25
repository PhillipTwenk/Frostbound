using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EntityActions.WorkersScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dialogues
{
    public class DialogueManager : MonoBehaviour
    {
        #region Ивенты для сообщении о действиях для квестов

        public static Action<Building, ActionTypeInteractWithObject> OnBuildingPlaced;

        public static Action<ActionTypeInteractWithObject> OnObjectInteracted;
        
        public static Action<ActionTypeUIPanel> OnPanelOpened;

        public static Action<ActionTypeCallWorker> OnWorkerCalled;
        
        public static Action<ActionTypeMoveUnit> OnUnitMoved;

        #endregion
        
        
        [Header("UI Elements")]
        public GameObject leftPanel;
        public GameObject rightPanel;
        public GameObject DialogueFolder;
        public TextMeshProUGUI leftText;
        public TextMeshProUGUI rightText;
        public Image leftImage;
        public Image rightImage;
        public GameObject fadeOverlay;
        public GameObject leftContinuePanel; // Панель продолжения для левой панели
        public GameObject rightContinuePanel; // Панель продолжения для правой панели
        public TextMeshProUGUI leftCharacterNameText;
        public TextMeshProUGUI rightCharacterNameText;

        [Header("Parameters")]
        public float charactersPerSecond = 30f;

        [Header("Dialogue Info")]
        public static Dialogue currentDialogue; 

        [Header("State Control")]
        private int currentPhraseIndex;
        private bool isTextWriting;
        private bool canContinue;
        public static bool IsDialogueInProcess;
        private CancellationTokenSource _cancellationTokenSource; // Для отмены задачи печати текста

        [Header("Action System")]
        private Phrase _currentActionPhrase;
        private UnityEvent _currentActionEvent;

        [Header("Events")]
        private List<Action> _currentSubscriptions = new List<Action>();
        public static Action<Dialogue> LaunchDialogue;
        public static Action OnEndTutorial;

        [Header("UnityEvents")]
        public UnityEvent OnStartDialogueUE;

        private void Start()
        {
            // Подписка на события
            LaunchDialogue += StartDialogue;
            LaunchDialogue += (Dialogue dialogue) => OnStartDialogueUE?.Invoke();
        }

        private void OnDestroy()
        {
            // Отписка от событий
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            LaunchDialogue -= StartDialogue;
            LaunchDialogue -= (Dialogue dialogue) => OnStartDialogueUE?.Invoke();
        }

        #region Общие методы контроля диалога

        private void Update()
        {
            if (Input.GetButtonDown("TutorialUpdate"))
            {
                if (Input.GetMouseButtonDown(0) && GeneralWorkersControl.BlockMouseClickThisFrame)
                {
                    return;
                }
                
                if (isTextWriting)
                {
                    ShowFullText();
                    return;
                }
                if (canContinue)
                {
                    ShowNextPhrase();
                }
            }
        }

        public void StartDialogue(Dialogue dialogue)
        {
            DialogueFolder.SetActive(true);
            currentDialogue = dialogue;
            currentPhraseIndex = 0;
            currentDialogue.isActive = true;
            IsDialogueInProcess = true;
            fadeOverlay.SetActive(false);
            ProcessCurrentPhrase();
        }

        private async Task ProcessCurrentPhrase()
        {
            var phrase = currentDialogue.phrases[currentPhraseIndex];
            canContinue = false;
            HideContinuePanels(); // Скрываем панели продолжения в начале новой фразы

            leftPanel.SetActive(phrase.side == DialogueSide.Left);
            rightPanel.SetActive(phrase.side == DialogueSide.Right);

            var currentImage = phrase.side == DialogueSide.Left ? leftImage : rightImage;
            currentImage.sprite = phrase.characterImage;

            var currentCharacterNameText = phrase.side == DialogueSide.Left ? leftCharacterNameText : rightCharacterNameText;
            currentCharacterNameText.text = phrase.characterName;

            fadeOverlay.SetActive(phrase.isFade);
            if (phrase.isFade)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            if (phrase.isActionAwait)
            {
                SetupActionRequirement(phrase);
            }

            var targetText = phrase.side == DialogueSide.Left ? leftText : rightText;
            await TypeText(targetText, phrase.text);
        }

        private async Task TypeText(TextMeshProUGUI target, string text)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            isTextWriting = true;
            target.text = "";

            try
            {
                foreach (char c in text)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    target.text += c;
                    await Task.Delay((int)(1000 / charactersPerSecond), cancellationToken);
                }

                // После завершения печати текста (если не было отмены)
                if (!cancellationToken.IsCancellationRequested)
                {
                    var phrase = currentDialogue.phrases[currentPhraseIndex];
                    if (!phrase.isActionAwait)
                    {
                        canContinue = true;
                        ShowContinuePanel(phrase.side);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Игнорируем отмену
            }
            finally
            {
                isTextWriting = false;
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void ShowFullText()
        {
            _cancellationTokenSource?.Cancel();
    
            var phrase = currentDialogue.phrases[currentPhraseIndex];
            var targetText = phrase.side == DialogueSide.Left ? leftText : rightText;
            targetText.text = phrase.text;
            isTextWriting = false;
    
            if (!phrase.isActionAwait)
            {
                canContinue = true;
                ShowContinuePanel(phrase.side);
            }
        }

        private void ShowNextPhrase()
        {
            // Скрываем панели продолжения
            HideContinuePanels();

            currentPhraseIndex++;

            if (currentPhraseIndex >= currentDialogue.phrases.Count)
            {
                EndDialogue();
                return;
            }

            ClearPanels();
            ProcessCurrentPhrase();
        }

        private void ClearPanels()
        {
            leftText.text = "";
            rightText.text = "";
            leftPanel.SetActive(false);
            rightPanel.SetActive(false);
        }

        private void EndDialogue()
        {
            EndTutorial();
            OnEndTutorial?.Invoke();
            IsDialogueInProcess = false;
            currentDialogue.isActive = false;
            currentDialogue.isCompleted = true;
            fadeOverlay.SetActive(false);
            DialogueFolder.SetActive(false);
            ClearPanels();
            HideContinuePanels(); // Скрываем панели продолжения при завершении диалога
        }

        /// <summary>
        /// Окончание туториала, если он был активен
        /// </summary>
        private void EndTutorial()
        {
            if (currentDialogue.isTutorial)
            {
                Time.timeScale = 1f;
                PlayerPrefs.SetInt("TutorialCompleted", 1);
            }
        }

        #endregion

        #region Методы отслеживания действия для продолжения диалога ( для туториала )

        /// <summary>
        /// Подписывается на ивент действия, которое ожидается для продорлжения туториала / диалога
        /// </summary>
        /// <param name="phrase"></param>
        private void SetupActionRequirement(Phrase phrase)
        {
            switch (phrase.actionCategory)
            {
                case ActionCategory.BuildBuilding:
                    Action<Building, ActionTypeInteractWithObject> buildHandler = (Building buildingType, ActionTypeInteractWithObject actionTypeInteractWithObject) => HandleBuildBuilding(phrase, buildingType, actionTypeInteractWithObject);     
                    OnBuildingPlaced += buildHandler;
                    _currentSubscriptions.Add(() => OnBuildingPlaced -= buildHandler);
                    break;

                case ActionCategory.UIPanel:
                    Action<ActionTypeUIPanel> panelHandler = (ActionTypeUIPanel panelType) => HandleOpenPanel(phrase, panelType);
                    OnPanelOpened += panelHandler;
                    _currentSubscriptions.Add(() => OnPanelOpened -= panelHandler);
                    break;

                case ActionCategory.CallWorker:
                    Action<ActionTypeCallWorker> workerHandler = (ActionTypeCallWorker workerType) => HandleCallWorker(phrase, workerType);
                    OnWorkerCalled += workerHandler;
                    _currentSubscriptions.Add(() => OnWorkerCalled -= workerHandler);
                    break;

                case ActionCategory.MoveUnit:
                    Action<ActionTypeMoveUnit> moveHandler = (ActionTypeMoveUnit unitType) => HandleMoveUnit(phrase, unitType);
                    OnUnitMoved += moveHandler;
                    _currentSubscriptions.Add(() => OnUnitMoved -= moveHandler);
                    break;

                case ActionCategory.InteractWithObject:
                    Action<ActionTypeInteractWithObject> interactHandler = (ActionTypeInteractWithObject objectType) => HandleInteractWithObject(phrase, objectType);
                    OnObjectInteracted += interactHandler;
                    _currentSubscriptions.Add(() => OnObjectInteracted -= interactHandler);
                    break;
            }
        }

        #region Методы проверки выполненности действия для каждой категории действий

        private void HandleBuildBuilding(Phrase phrase, Building buildingType, ActionTypeInteractWithObject actionTypeInteractWithObject)
        {
            if (buildingType.buildingType == phrase.actionParameterBuildBuilding && phrase.actionParameterInteractWithObject == actionTypeInteractWithObject)
            {
                HandleActionCompleted();
            }
        }

        private void HandleOpenPanel(Phrase phrase, ActionTypeUIPanel panelType)
        {
            if (panelType == phrase.actionParameterUIPanel)
            {
                HandleActionCompleted();
            }
        }

        private void HandleCallWorker(Phrase phrase, ActionTypeCallWorker workerType)
        {
            if (workerType == phrase.actionParameterCallWorker)
            {
                HandleActionCompleted();
            }
        }

        private void HandleMoveUnit(Phrase phrase, ActionTypeMoveUnit unitActionType)
        {
            if (unitActionType == phrase.actionParameterMoveUnit)
            {
                HandleActionCompleted();
            }
        }

        private void HandleInteractWithObject(Phrase phrase, ActionTypeInteractWithObject objectType)
        {
            if (objectType == phrase.actionParameterInteractWithObject)
            {
                HandleActionCompleted();
            }
        }

        #endregion

        private void HandleActionCompleted()
        {
            // Отписываемся от всех событий
            UnsubscribeFromAllEvents();

            // Продолжаем диалог
            ShowNextPhrase();
        }
        
        private void UnsubscribeFromAllEvents()
        {
            foreach (var unsubscribeAction in _currentSubscriptions)
            {
                unsubscribeAction();
            }
            _currentSubscriptions.Clear();
        }

        #endregion

        #region Методы для управления панелями продолжения

        private void ShowContinuePanel(DialogueSide side)
        {
            if (side == DialogueSide.Left)
            {
                leftContinuePanel.SetActive(true);
            }
            else if (side == DialogueSide.Right)
            {
                rightContinuePanel.SetActive(true);
            }
        }

        private void HideContinuePanels()
        {
            leftContinuePanel.SetActive(false);
            rightContinuePanel.SetActive(false);
        }

        #endregion
    }
}