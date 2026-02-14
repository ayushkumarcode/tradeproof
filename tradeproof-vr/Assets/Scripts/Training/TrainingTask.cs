using UnityEngine;
using TradeProof.Data;

namespace TradeProof.Training
{
    public abstract class TrainingTask : MonoBehaviour
    {
        public TaskMode CurrentMode { get; protected set; }

        public abstract string TaskId { get; }
        public abstract string TaskName { get; }
        public abstract string Description { get; }

        protected bool hintsEnabled;
        protected float timeRemaining;
        protected float timeLimit;
        protected bool isActive;
        protected float elapsedTime;

        public float TimeRemaining => timeRemaining;
        public float TimeLimit => timeLimit;
        public float ElapsedTime => elapsedTime;
        public bool IsActive => isActive;
        public bool HintsEnabled => hintsEnabled;

        public abstract void StartLearnMode();
        public abstract void StartPracticeMode();
        public abstract void StartTestMode();
        public abstract float GetCompletionPercentage();
        public abstract TaskResult EvaluatePerformance();

        protected virtual void Update()
        {
            if (!isActive) return;

            elapsedTime += Time.deltaTime;

            // Timer countdown is handled by TaskManager to avoid double-decrement.
            // Sync local timeRemaining from TaskManager for display purposes.
            if (CurrentMode == TaskMode.Test)
            {
                timeRemaining = Core.TaskManager.Instance.TimeRemaining;
            }
        }

        protected virtual void OnTimeExpired()
        {
            Debug.Log($"[{TaskName}] Time expired!");
            isActive = false;
        }

        public void RequestHint()
        {
            if (CurrentMode == TaskMode.Practice && hintsEnabled)
            {
                ShowHint();
                Core.AudioManager.Instance.PlayHintSound();
            }
        }

        protected abstract void ShowHint();

        public virtual void ResetTask()
        {
            isActive = false;
            elapsedTime = 0f;
            timeRemaining = 0f;
            hintsEnabled = false;
            CurrentMode = TaskMode.Learn;
        }

        protected void SetupMode(TaskMode mode, float timeLimitSeconds)
        {
            CurrentMode = mode;
            isActive = true;
            elapsedTime = 0f;

            switch (mode)
            {
                case TaskMode.Learn:
                    hintsEnabled = true;
                    timeLimit = 0f;
                    timeRemaining = 0f;
                    break;

                case TaskMode.Practice:
                    hintsEnabled = true;
                    timeLimit = 0f;
                    timeRemaining = 0f;
                    break;

                case TaskMode.Test:
                    hintsEnabled = false;
                    timeLimit = timeLimitSeconds;
                    timeRemaining = timeLimitSeconds;
                    break;
            }
        }
    }
}
