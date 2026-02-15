using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TradeProof.Data;
using TradeProof.Training;
using TradeProof.Training.Trackers;

namespace TradeProof.Core
{
    public class TaskManager : MonoBehaviour
    {
        private static TaskManager _instance;
        public static TaskManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("TaskManager");
                    _instance = go.AddComponent<TaskManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Task State")]
        private TrainingTask activeTask;
        private TaskDefinition activeDefinition;
        private TaskMode activeMode;
        private float timer;
        private bool isTimerRunning;

        [Header("Tracker")]
        private ITaskTracker activeTracker;

        [Header("Loaded Definitions")]
        private Dictionary<string, TaskDefinition> taskDefinitions = new Dictionary<string, TaskDefinition>();

        public float TimeRemaining => timer;
        public bool IsTimerRunning => isTimerRunning;
        public TrainingTask ActiveTask => activeTask;
        public TaskDefinition ActiveDefinition => activeDefinition;
        public TaskMode ActiveMode => activeMode;
        public ITaskTracker ActiveTracker => activeTracker;

        // Legacy compatibility properties — delegate to tracker
        public int ViolationsFound
        {
            get
            {
                var t = GetTracker<PanelInspectionTracker>();
                return t != null ? t.ViolationsFound : 0;
            }
        }
        public int TotalViolations
        {
            get
            {
                var t = GetTracker<PanelInspectionTracker>();
                return t != null ? t.TotalViolations : 0;
            }
        }
        public int StepsCompleted
        {
            get
            {
                var t = GetTracker<CircuitWiringTracker>();
                return t != null ? t.StepsCompleted : 0;
            }
        }
        public int TotalSteps
        {
            get
            {
                var t = GetTracker<CircuitWiringTracker>();
                return t != null ? t.TotalSteps : 0;
            }
        }
        public int CurrentStepIndex
        {
            get
            {
                var t = GetTracker<CircuitWiringTracker>();
                return t != null ? t.CurrentStepIndex : 0;
            }
        }
        public int ConnectionQualityScore
        {
            get
            {
                var t = GetTracker<CircuitWiringTracker>();
                return t != null ? t.ConnectionQualityScore : 0;
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

            LoadAllTaskDefinitions();
        }

        private void Update()
        {
            if (isTimerRunning)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = 0f;
                    isTimerRunning = false;
                    OnTimerExpired();
                }
            }
        }

        private void LoadAllTaskDefinitions()
        {
            TextAsset[] taskFiles = Resources.LoadAll<TextAsset>("TaskDefinitions");
            foreach (TextAsset file in taskFiles)
            {
                TaskDefinition def = JsonUtility.FromJson<TaskDefinition>(file.text);
                if (def != null && !string.IsNullOrEmpty(def.taskId))
                {
                    taskDefinitions[def.taskId] = def;
                    Debug.Log($"[TaskManager] Loaded task definition: {def.taskId} ({def.taskName})");
                }
            }

            if (taskDefinitions.Count == 0)
            {
                Debug.LogWarning("[TaskManager] No task definitions loaded from Resources/TaskDefinitions/");
            }
        }

        public TaskDefinition GetTaskDefinition(string taskId)
        {
            if (taskDefinitions.TryGetValue(taskId, out TaskDefinition def))
            {
                return def;
            }
            Debug.LogError($"[TaskManager] Task definition not found: {taskId}");
            return null;
        }

        public List<TaskDefinition> GetAllTaskDefinitions()
        {
            return taskDefinitions.Values.ToList();
        }

        // --- Tracker Management ---

        public T GetTracker<T>() where T : class, ITaskTracker
        {
            return activeTracker as T;
        }

        public ITaskTracker GetActiveTracker()
        {
            return activeTracker;
        }

        private ITaskTracker CreateTrackerForTask(string taskId)
        {
            // Map task IDs to their tracker types
            if (taskId.StartsWith("panel-inspection"))
                return new PanelInspectionTracker();
            if (taskId.StartsWith("circuit-wiring"))
                return new CircuitWiringTracker();
            if (taskId.StartsWith("outlet-installation"))
                return new CircuitWiringTracker(); // Step-based, same pattern
            if (taskId.StartsWith("switch-wiring"))
                return new CircuitWiringTracker(); // Step-based
            if (taskId.StartsWith("gfci-testing"))
                return new CircuitWiringTracker(); // Step-based
            if (taskId.StartsWith("conduit-bending"))
                return new CircuitWiringTracker(); // Step-based
            if (taskId.StartsWith("troubleshooting"))
                return new CircuitWiringTracker(); // Step-based

            // Default to step-based tracker
            Debug.LogWarning($"[TaskManager] No specific tracker for task '{taskId}', using CircuitWiringTracker");
            return new CircuitWiringTracker();
        }

        // --- Task Initialization ---

        public void InitializeTask(TrainingTask task, TaskMode mode)
        {
            activeTask = task;
            activeMode = mode;
            activeDefinition = GetTaskDefinition(task.TaskId);

            if (activeDefinition == null)
            {
                Debug.LogError($"[TaskManager] Cannot initialize task -- definition not found for {task.TaskId}");
                return;
            }

            // Create and initialize tracker
            activeTracker = CreateTrackerForTask(task.TaskId);
            activeTracker.Initialize(activeDefinition, mode);

            // Timer setup for test mode
            if (mode == TaskMode.Test)
            {
                timer = activeDefinition.timeLimit;
                isTimerRunning = true;
            }
            else
            {
                timer = 0f;
                isTimerRunning = false;
            }

            // Start the appropriate mode
            switch (mode)
            {
                case TaskMode.Learn:
                    task.StartLearnMode();
                    break;
                case TaskMode.Practice:
                    task.StartPracticeMode();
                    break;
                case TaskMode.Test:
                    task.StartTestMode();
                    break;
            }

            Debug.Log($"[TaskManager] Task initialized: {activeDefinition.taskName} in {mode} mode (tracker: {activeTracker.GetType().Name})");
        }

        public void InitializeTask(TrainingTask task, TaskMode mode, ITaskTracker tracker)
        {
            activeTask = task;
            activeMode = mode;
            activeDefinition = GetTaskDefinition(task.TaskId);
            activeTracker = tracker;

            if (activeDefinition == null)
            {
                Debug.LogError($"[TaskManager] Cannot initialize task -- definition not found for {task.TaskId}");
                return;
            }

            activeTracker.Initialize(activeDefinition, mode);

            if (mode == TaskMode.Test)
            {
                timer = activeDefinition.timeLimit;
                isTimerRunning = true;
            }
            else
            {
                timer = 0f;
                isTimerRunning = false;
            }

            switch (mode)
            {
                case TaskMode.Learn:
                    task.StartLearnMode();
                    break;
                case TaskMode.Practice:
                    task.StartPracticeMode();
                    break;
                case TaskMode.Test:
                    task.StartTestMode();
                    break;
            }

            Debug.Log($"[TaskManager] Task initialized with custom tracker: {activeDefinition.taskName} in {mode} mode");
        }

        // --- Panel Inspection Tracking (delegates to PanelInspectionTracker) ---

        public bool IdentifyViolation(string violationId)
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            if (tracker == null)
            {
                Debug.LogError("[TaskManager] IdentifyViolation called but active tracker is not PanelInspectionTracker");
                return false;
            }

            bool result = tracker.IdentifyViolation(violationId);
            if (result && tracker.AllViolationsFound())
            {
                OnAllViolationsFound();
            }
            return result;
        }

        public bool IsViolationIdentified(string violationId)
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            return tracker != null && tracker.IsViolationIdentified(violationId);
        }

        public ViolationDefinition GetNextHint()
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            return tracker?.GetNextHint();
        }

        // --- Circuit Wiring Tracking (delegates to CircuitWiringTracker) ---

        public bool CompleteStep(string stepId)
        {
            var tracker = GetTracker<CircuitWiringTracker>();
            if (tracker == null)
            {
                Debug.LogError("[TaskManager] CompleteStep called but active tracker is not CircuitWiringTracker");
                return false;
            }

            bool result = tracker.CompleteStep(stepId);
            if (result && tracker.AllStepsCompleted())
            {
                OnAllStepsCompleted();
            }
            return result;
        }

        public void SetWireGaugeCorrect(bool correct)
        {
            var tracker = GetTracker<CircuitWiringTracker>();
            tracker?.SetWireGaugeCorrect(correct);
        }

        public void AddConnectionQuality(int points)
        {
            var tracker = GetTracker<CircuitWiringTracker>();
            tracker?.AddConnectionQuality(points);
        }

        public StepDefinition GetCurrentStep()
        {
            var tracker = GetTracker<CircuitWiringTracker>();
            return tracker?.GetCurrentStep();
        }

        // --- Completion ---

        private void OnAllViolationsFound()
        {
            Debug.Log("[TaskManager] All violations found!");
            if (activeMode == TaskMode.Test)
                isTimerRunning = false;
            FinishTask();
        }

        private void OnAllStepsCompleted()
        {
            Debug.Log("[TaskManager] All steps completed!");
            if (activeMode == TaskMode.Test)
                isTimerRunning = false;
            FinishTask();
        }

        private void OnTimerExpired()
        {
            Debug.Log("[TaskManager] Timer expired!");
            AudioManager.Instance.PlayTimerEndSound();
            FinishTask();
        }

        public void FinishTask()
        {
            isTimerRunning = false;

            if (activeTask == null)
            {
                Debug.LogError("[TaskManager] FinishTask called but no active task");
                return;
            }

            TaskResult result = activeTask.EvaluatePerformance();

            result.taskId = activeTask.TaskId;
            result.taskName = activeTask.TaskName;
            result.mode = activeMode;
            result.badgeId = activeDefinition != null ? activeDefinition.badgeAwarded : "";

            GameManager.Instance.CompleteTask(result);
        }

        public void ForceFinishTask()
        {
            FinishTask();
        }

        // --- Score Building Helpers (legacy, delegate to trackers) ---

        public float GetPanelInspectionScore()
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            return tracker != null ? tracker.GetScore() : 0f;
        }

        public float GetCircuitWiringScore()
        {
            var tracker = GetTracker<CircuitWiringTracker>();
            return tracker != null ? tracker.GetScore() : 0f;
        }

        public int GetIdentifiedViolationCount()
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            return tracker != null ? tracker.ViolationsFound : 0;
        }

        public int GetFalsePositiveCount()
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            return tracker != null ? tracker.FalsePositiveCount : 0;
        }

        public List<string> GetIdentifiedViolationIds()
        {
            var tracker = GetTracker<PanelInspectionTracker>();
            return tracker != null ? tracker.GetIdentifiedViolationIds() : new List<string>();
        }

        public List<string> GetCompletedStepIds()
        {
            var tracker = GetTracker<CircuitWiringTracker>();
            return tracker != null ? tracker.GetCompletedStepIds() : new List<string>();
        }
    }
}
