using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace TradeProof.Core
{
    public enum GameState
    {
        MainMenu,
        TaskSelection,
        Training,
        Results,
        DayBriefing,
        WorkOrderBoard,
        JobSiteTransition,
        DayResults
    }

    public enum CareerLevel
    {
        Apprentice = 0,
        Journeyman = 1,
        Master = 2
    }

    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public event Action<GameState> OnStateChanged;

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        public GameState CurrentState => currentState;

        [Header("Current Session")]
        public string CurrentTaskId;
        public Training.TaskMode CurrentTaskMode;
        public Data.TaskResult LastTaskResult;

        [Header("Day Mode")]
        public Data.WorkOrder CurrentWorkOrder;
        public bool IsDayMode;

        [Header("References")]
        public Camera MainCamera;
        public Transform PlayerRig;

        private Data.PlayerProgress playerProgress;
        public Data.PlayerProgress Progress => playerProgress;

        public CareerLevel CurrentCareerLevel
        {
            get
            {
                if (playerProgress == null) return CareerLevel.Apprentice;
                return playerProgress.GetCareerLevel();
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadPlayerProgress();
            InitializeMetaXR();
        }

        private void Start()
        {
            if (MainCamera == null)
                MainCamera = Camera.main;

            TransitionToState(GameState.MainMenu);
        }

        private void InitializeMetaXR()
        {
            OVRManager ovrManager = FindObjectOfType<OVRManager>();
            if (ovrManager != null)
            {
                ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }
            else
            {
                Debug.LogWarning("[GameManager] OVRManager not found in scene. Ensure OVRCameraRig is present.");
            }
        }

        public void TransitionToState(GameState newState)
        {
            GameState previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] State transition: {previousState} -> {newState}");

            switch (newState)
            {
                case GameState.MainMenu:
                    HandleMainMenuState();
                    break;
                case GameState.TaskSelection:
                    HandleTaskSelectionState();
                    break;
                case GameState.Training:
                    HandleTrainingState();
                    break;
                case GameState.Results:
                    HandleResultsState();
                    break;
                case GameState.DayBriefing:
                    HandleDayBriefingState();
                    break;
                case GameState.WorkOrderBoard:
                    HandleWorkOrderBoardState();
                    break;
                case GameState.JobSiteTransition:
                    HandleJobSiteTransitionState();
                    break;
                case GameState.DayResults:
                    HandleDayResultsState();
                    break;
            }

            OnStateChanged?.Invoke(newState);
        }

        private void HideAllUI()
        {
            UI.MainMenuUI mainMenu = FindObjectOfType<UI.MainMenuUI>(true);
            if (mainMenu != null) mainMenu.Hide();

            UI.TaskSelectionUI taskSelection = FindObjectOfType<UI.TaskSelectionUI>(true);
            if (taskSelection != null) taskSelection.Hide();

            UI.ResultsScreenUI results = FindObjectOfType<UI.ResultsScreenUI>(true);
            if (results != null) results.Hide();

            UI.HUDController hud = FindObjectOfType<UI.HUDController>(true);
            if (hud != null) hud.Hide();
        }

        private void HandleMainMenuState()
        {
            IsDayMode = false;
            HideAllUI();

            UI.MainMenuUI mainMenu = FindObjectOfType<UI.MainMenuUI>(true);
            if (mainMenu != null) mainMenu.Show();
        }

        private void HandleTaskSelectionState()
        {
            HideAllUI();

            UI.TaskSelectionUI taskSelection = FindObjectOfType<UI.TaskSelectionUI>(true);
            if (taskSelection != null) taskSelection.Show();
        }

        private void HandleTrainingState()
        {
            HideAllUI();

            UI.HUDController hud = FindObjectOfType<UI.HUDController>(true);
            if (hud != null) hud.Show();
        }

        private void HandleResultsState()
        {
            HideAllUI();

            UI.ResultsScreenUI results = FindObjectOfType<UI.ResultsScreenUI>(true);
            if (results != null)
            {
                results.Show();
                results.DisplayResults(LastTaskResult);
            }
        }

        private void HandleDayBriefingState()
        {
            IsDayMode = true;
            HideAllUI();

            UI.DayBriefingUI briefing = FindObjectOfType<UI.DayBriefingUI>(true);
            if (briefing != null) briefing.Show();
        }

        private void HandleWorkOrderBoardState()
        {
            HideAllUI();

            UI.WorkOrderBoardUI board = FindObjectOfType<UI.WorkOrderBoardUI>(true);
            if (board != null) board.Show();
        }

        private void HandleJobSiteTransitionState()
        {
            HideAllUI();
            // JobSiteManager handles loading the environment, then transitions to Training
            var jobSiteManager = FindObjectOfType<Environment.JobSiteManager>();
            if (jobSiteManager != null && CurrentWorkOrder != null)
            {
                jobSiteManager.LoadJobSite(CurrentWorkOrder.jobSiteType);
            }
            TransitionToState(GameState.Training);
        }

        private void HandleDayResultsState()
        {
            HideAllUI();

            UI.DayBriefingUI briefing = FindObjectOfType<UI.DayBriefingUI>(true);
            if (briefing != null) briefing.ShowDayResults();
        }

        // --- Task Management ---

        public void StartTask(string taskId, Training.TaskMode mode)
        {
            CurrentTaskId = taskId;
            CurrentTaskMode = mode;
            TransitionToState(GameState.Training);
        }

        public void StartWorkOrder(Data.WorkOrder order, Training.TaskMode mode)
        {
            CurrentWorkOrder = order;
            CurrentTaskId = order.taskId;
            CurrentTaskMode = mode;
            TransitionToState(GameState.JobSiteTransition);
        }

        public void CompleteTask(Data.TaskResult result)
        {
            LastTaskResult = result;

            playerProgress.RecordTaskCompletion(result);

            // Award XP
            int xpEarned = CalculateXP(result);
            playerProgress.AddXP(xpEarned);

            SavePlayerProgress();

            if (result.passed)
            {
                BadgeSystem.Instance.AwardBadge(result.badgeId, result.taskId, result.score);
            }

            TransitionToState(GameState.Results);
        }

        private int CalculateXP(Data.TaskResult result)
        {
            int baseXP = 0;
            switch (result.mode)
            {
                case Training.TaskMode.Learn: baseXP = 10; break;
                case Training.TaskMode.Practice: baseXP = 25; break;
                case Training.TaskMode.Test: baseXP = 50; break;
            }

            if (result.passed) baseXP *= 2;

            // Bonus for high scores
            if (result.score >= 95f) baseXP += 25;
            else if (result.score >= 90f) baseXP += 15;

            // Work order bonus
            if (CurrentWorkOrder != null)
            {
                baseXP = Mathf.RoundToInt(baseXP * CurrentWorkOrder.bonusMultiplier);
            }

            return baseXP;
        }

        public void ReturnToMenu()
        {
            TransitionToState(GameState.MainMenu);
        }

        public void ReturnToTaskSelection()
        {
            if (IsDayMode)
                TransitionToState(GameState.WorkOrderBoard);
            else
                TransitionToState(GameState.TaskSelection);
        }

        // --- Day Mode ---

        public void StartDay()
        {
            TransitionToState(GameState.DayBriefing);
        }

        public void EndDay()
        {
            playerProgress.daysCompleted++;
            SavePlayerProgress();
            TransitionToState(GameState.DayResults);
        }

        // --- Persistence ---

        private void LoadPlayerProgress()
        {
            string json = PlayerPrefs.GetString("TradeProof_PlayerProgress", "");
            if (!string.IsNullOrEmpty(json))
            {
                playerProgress = JsonUtility.FromJson<Data.PlayerProgress>(json);
            }
            else
            {
                playerProgress = new Data.PlayerProgress();
            }
        }

        public void SavePlayerProgress()
        {
            string json = JsonUtility.ToJson(playerProgress);
            PlayerPrefs.SetString("TradeProof_PlayerProgress", json);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SavePlayerProgress();
            }
        }

        private void OnApplicationQuit()
        {
            SavePlayerProgress();
        }
    }
}
