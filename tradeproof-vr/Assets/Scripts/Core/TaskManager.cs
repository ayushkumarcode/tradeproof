using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TradeProof.Data;
using TradeProof.Training;

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

        [Header("Tracking - Panel Inspection")]
        private HashSet<string> identifiedViolations = new HashSet<string>();
        private HashSet<string> falsePositives = new HashSet<string>();
        private int totalViolations;

        [Header("Tracking - Circuit Wiring")]
        private List<string> completedSteps = new List<string>();
        private int currentStepIndex;
        private int totalSteps;
        private bool wireGaugeCorrect;
        private int connectionQualityScore;

        [Header("Loaded Definitions")]
        private Dictionary<string, TaskDefinition> taskDefinitions = new Dictionary<string, TaskDefinition>();

        public float TimeRemaining => timer;
        public bool IsTimerRunning => isTimerRunning;
        public int ViolationsFound => identifiedViolations.Count;
        public int TotalViolations => totalViolations;
        public int StepsCompleted => completedSteps.Count;
        public int TotalSteps => totalSteps;
        public int CurrentStepIndex => currentStepIndex;
        public int ConnectionQualityScore => connectionQualityScore;

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

        public void InitializeTask(TrainingTask task, TaskMode mode)
        {
            activeTask = task;
            activeMode = mode;
            activeDefinition = GetTaskDefinition(task.TaskId);

            if (activeDefinition == null)
            {
                Debug.LogError($"[TaskManager] Cannot initialize task — definition not found for {task.TaskId}");
                return;
            }

            // Reset tracking
            identifiedViolations.Clear();
            falsePositives.Clear();
            completedSteps.Clear();
            currentStepIndex = 0;
            wireGaugeCorrect = false;
            connectionQualityScore = 0;

            // Set up violation/step counts
            if (activeDefinition.violations != null)
                totalViolations = activeDefinition.violations.Length;
            if (activeDefinition.steps != null)
                totalSteps = activeDefinition.steps.Length;

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

            Debug.Log($"[TaskManager] Task initialized: {activeDefinition.taskName} in {mode} mode");
        }

        // --- Panel Inspection Tracking ---

        public bool IdentifyViolation(string violationId)
        {
            if (activeDefinition == null) return false;

            // Check if this is a real violation
            bool isRealViolation = false;
            if (activeDefinition.violations != null)
            {
                foreach (var v in activeDefinition.violations)
                {
                    if (v.id == violationId)
                    {
                        isRealViolation = true;
                        break;
                    }
                }
            }

            if (isRealViolation)
            {
                if (identifiedViolations.Add(violationId))
                {
                    Debug.Log($"[TaskManager] Violation identified: {violationId}");
                    AudioManager.Instance.PlayCorrectSound();

                    // Check if all violations found
                    if (identifiedViolations.Count >= totalViolations)
                    {
                        OnAllViolationsFound();
                    }
                    return true;
                }
                return false; // Already identified
            }
            else
            {
                falsePositives.Add(violationId);
                Debug.Log($"[TaskManager] False positive: {violationId}");
                AudioManager.Instance.PlayIncorrectSound();
                return false;
            }
        }

        public bool IsViolationIdentified(string violationId)
        {
            return identifiedViolations.Contains(violationId);
        }

        public ViolationDefinition GetNextHint()
        {
            if (activeDefinition == null || activeDefinition.violations == null) return null;

            foreach (var violation in activeDefinition.violations)
            {
                if (!identifiedViolations.Contains(violation.id))
                {
                    return violation;
                }
            }
            return null;
        }

        // --- Circuit Wiring Tracking ---

        public bool CompleteStep(string stepId)
        {
            if (activeDefinition == null || activeDefinition.steps == null) return false;

            if (currentStepIndex >= activeDefinition.steps.Length)
            {
                Debug.LogWarning("[TaskManager] All steps already completed");
                return false;
            }

            StepDefinition expectedStep = activeDefinition.steps[currentStepIndex];

            if (activeMode == TaskMode.Test)
            {
                // In test mode, just record whatever step they do
                completedSteps.Add(stepId);
                currentStepIndex++;
                AudioManager.Instance.PlaySnapSound();

                if (currentStepIndex >= totalSteps)
                {
                    OnAllStepsCompleted();
                }
                return true;
            }

            // In learn/practice mode, enforce correct order
            if (stepId == expectedStep.id)
            {
                completedSteps.Add(stepId);
                currentStepIndex++;
                AudioManager.Instance.PlayCorrectSound();
                Debug.Log($"[TaskManager] Step completed: {stepId} ({currentStepIndex}/{totalSteps})");

                if (currentStepIndex >= totalSteps)
                {
                    OnAllStepsCompleted();
                }
                return true;
            }
            else
            {
                Debug.Log($"[TaskManager] Wrong step: expected {expectedStep.id}, got {stepId}");
                if (activeMode == TaskMode.Practice)
                {
                    AudioManager.Instance.PlayIncorrectSound();
                    // Show feedback about correct step
                    UI.HintPanel hintPanel = FindObjectOfType<UI.HintPanel>();
                    if (hintPanel != null)
                    {
                        hintPanel.ShowHint($"Wrong step. Next: {expectedStep.description}\nNEC {expectedStep.necCode}");
                    }
                }
                return false;
            }
        }

        public void SetWireGaugeCorrect(bool correct)
        {
            wireGaugeCorrect = correct;
        }

        public void AddConnectionQuality(int points)
        {
            connectionQualityScore += points;
        }

        public StepDefinition GetCurrentStep()
        {
            if (activeDefinition == null || activeDefinition.steps == null) return null;
            if (currentStepIndex >= activeDefinition.steps.Length) return null;
            return activeDefinition.steps[currentStepIndex];
        }

        // --- Completion ---

        private void OnAllViolationsFound()
        {
            Debug.Log("[TaskManager] All violations found!");
            if (activeMode == TaskMode.Test)
            {
                isTimerRunning = false;
            }
            FinishTask();
        }

        private void OnAllStepsCompleted()
        {
            Debug.Log("[TaskManager] All steps completed!");
            if (activeMode == TaskMode.Test)
            {
                isTimerRunning = false;
            }
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

            // Enrich result with tracking data
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

        // --- Score Building Helpers ---

        public float GetPanelInspectionScore()
        {
            if (totalViolations == 0) return 0f;

            float foundRatio = (float)identifiedViolations.Count / totalViolations;
            float falsePositivePenalty = falsePositives.Count * 0.1f;
            float score = Mathf.Clamp01(foundRatio - falsePositivePenalty) * 100f;
            return score;
        }

        public float GetCircuitWiringScore()
        {
            if (totalSteps == 0) return 0f;

            float stepRatio = (float)completedSteps.Count / totalSteps;
            float gaugeBonus = wireGaugeCorrect ? 0.1f : -0.1f;
            float qualityBonus = (connectionQualityScore / (float)Mathf.Max(totalSteps, 1)) * 0.1f;

            // Check step order correctness
            int correctOrder = 0;
            if (activeDefinition != null && activeDefinition.steps != null)
            {
                for (int i = 0; i < completedSteps.Count && i < activeDefinition.steps.Length; i++)
                {
                    if (completedSteps[i] == activeDefinition.steps[i].id)
                    {
                        correctOrder++;
                    }
                }
            }
            float orderRatio = totalSteps > 0 ? (float)correctOrder / totalSteps : 0f;

            float score = (stepRatio * 0.5f + orderRatio * 0.3f + gaugeBonus + qualityBonus) * 100f;
            return Mathf.Clamp(score, 0f, 100f);
        }

        public int GetIdentifiedViolationCount()
        {
            return identifiedViolations.Count;
        }

        public int GetFalsePositiveCount()
        {
            return falsePositives.Count;
        }

        public List<string> GetIdentifiedViolationIds()
        {
            return identifiedViolations.ToList();
        }

        public List<string> GetCompletedStepIds()
        {
            return new List<string>(completedSteps);
        }
    }
}
