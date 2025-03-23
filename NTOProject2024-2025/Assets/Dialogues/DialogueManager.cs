using System;
using System.Collections.Generic;
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

        [Header("Parameters")]
        public float charactersPerSecond = 30f;

        [Header("Dialogue Info")]
        public static Dialogue currentDialogue; 

        [Header("State Control")]
        private int currentPhraseIndex;
        private bool isTextWriting;
        private bool canContinue;
        public static bool IsDialogueInProcess;

        [Header("Action System")]
        private Phrase _currentActionPhrase;
        private UnityEvent _currentActionEvent;

        [Header("Events")]
        private List<Action> _currentSubscriptions = new List<Action>();
        public static Action<Dialogue> LaunchDialogue;

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
            LaunchDialogue -= StartDialogue;
            LaunchDialogue -= (Dialogue dialogue) => OnStartDialogueUE?.Invoke();
        }

        #region Общие методы контроля диалога

        private void Update()
        {
            if (Input.GetButtonDown("TutorialUpdate"))
            {
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

            // Настройка панелей
            leftPanel.SetActive(phrase.side == DialogueSide.Left);
            rightPanel.SetActive(phrase.side == DialogueSide.Right);

            // Настройка изображения персонажа
            var currentImage = phrase.side == DialogueSide.Left ? leftImage : rightImage;
            currentImage.sprite = phrase.characterImage;

            // Настройка затемнения
            fadeOverlay.SetActive(phrase.isFade);
            if (phrase.isFade)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            // Если фраза требует действия, настраиваем подписку
            if (phrase.isActionAwait)
            {
                SetupActionRequirement(phrase);
            }

            // Запуск печати текста
            var targetText = phrase.side == DialogueSide.Left ? leftText : rightText;
            await TypeText(targetText, phrase.text);

            // Если фраза не требует действия, разрешаем продолжение
            if (!phrase.isActionAwait)
            {
                canContinue = true;
            }
        }

        private async Task TypeText(TextMeshProUGUI target, string text)
        {
            isTextWriting = true;
            target.text = "";

            foreach (char c in text)
            {
                if (!isTextWriting) break;
                target.text += c;
                await Task.Delay((int)(1000 / charactersPerSecond));
            }

            isTextWriting = false;
        }

        private void ShowFullText()
        {
            isTextWriting = false;
            var phrase = currentDialogue.phrases[currentPhraseIndex];
            var targetText = phrase.side == DialogueSide.Left ? leftText : rightText;
            targetText.text = phrase.text;

            // Если фраза не требует действия, разрешаем продолжение
            if (!phrase.isActionAwait)
            {
                canContinue = true;
            }
        }

        private void ShowNextPhrase()
        {
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
            IsDialogueInProcess = false;
            currentDialogue.isActive = false;
            currentDialogue.isCompleted = true;
            fadeOverlay.SetActive(false);
            DialogueFolder.SetActive(false);
            ClearPanels();
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
    }
}