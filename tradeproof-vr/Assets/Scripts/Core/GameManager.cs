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
        Results
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

        [Header("References")]
        public Camera MainCamera;
        public Transform PlayerRig;

        private Data.PlayerProgress playerProgress;
        public Data.PlayerProgress Progress => playerProgress;

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
            // Configure OVR Manager settings at runtime
            // OVRManager is expected to be on the camera rig in the scene
            OVRManager ovrManager = FindObjectOfType<OVRManager>();
            if (ovrManager != null)
            {
                ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }
            else
            {
                Debug.LogWarning("[GameManager] OVRManager not found in scene. Ensure OVRCameraRig is present.");
            }

            // Request hand tracking support
#if UNITY_ANDROID && !UNITY_EDITOR
            OVRPermissionsRequester.Request(new string[] { OVRPermissionsRequester.Permission.HandTracking });
#endif
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
            }

            OnStateChanged?.Invoke(newState);
        }

        private void HandleMainMenuState()
        {
            UI.MainMenuUI mainMenu = FindObjectOfType<UI.MainMenuUI>(true);
            if (mainMenu != null) mainMenu.Show();

            UI.TaskSelectionUI taskSelection = FindObjectOfType<UI.TaskSelectionUI>(true);
            if (taskSelection != null) taskSelection.Hide();

            UI.ResultsScreenUI results = FindObjectOfType<UI.ResultsScreenUI>(true);
            if (results != null) results.Hide();

            UI.HUDController hud = FindObjectOfType<UI.HUDController>(true);
            if (hud != null) hud.Hide();
        }

        private void HandleTaskSelectionState()
        {
            UI.MainMenuUI mainMenu = FindObjectOfType<UI.MainMenuUI>(true);
            if (mainMenu != null) mainMenu.Hide();

            UI.TaskSelectionUI taskSelection = FindObjectOfType<UI.TaskSelectionUI>(true);
            if (taskSelection != null) taskSelection.Show();

            UI.ResultsScreenUI results = FindObjectOfType<UI.ResultsScreenUI>(true);
            if (results != null) results.Hide();
        }

        private void HandleTrainingState()
        {
            UI.TaskSelectionUI taskSelection = FindObjectOfType<UI.TaskSelectionUI>(true);
            if (taskSelection != null) taskSelection.Hide();

            UI.HUDController hud = FindObjectOfType<UI.HUDController>(true);
            if (hud != null) hud.Show();

            UI.ResultsScreenUI results = FindObjectOfType<UI.ResultsScreenUI>(true);
            if (results != null) results.Hide();
        }

        private void HandleResultsState()
        {
            UI.HUDController hud = FindObjectOfType<UI.HUDController>(true);
            if (hud != null) hud.Hide();

            UI.ResultsScreenUI results = FindObjectOfType<UI.ResultsScreenUI>(true);
            if (results != null)
            {
                results.Show();
                results.DisplayResults(LastTaskResult);
            }
        }

        public void StartTask(string taskId, Training.TaskMode mode)
        {
            CurrentTaskId = taskId;
            CurrentTaskMode = mode;
            TransitionToState(GameState.Training);
        }

        public void CompleteTask(Data.TaskResult result)
        {
            LastTaskResult = result;

            // Save progress
            playerProgress.RecordTaskCompletion(result);
            SavePlayerProgress();

            // Check for badge
            if (result.passed)
            {
                BadgeSystem.Instance.AwardBadge(result.badgeId, result.taskId, result.score);
            }

            TransitionToState(GameState.Results);
        }

        public void ReturnToMenu()
        {
            TransitionToState(GameState.MainMenu);
        }

        public void ReturnToTaskSelection()
        {
            TransitionToState(GameState.TaskSelection);
        }

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
