using UnityEngine;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Data;

namespace TradeProof.Training.Trackers
{
    public class CircuitWiringTracker : ITaskTracker
    {
        private TaskDefinition definition;
        private TaskMode mode;
        private List<string> completedSteps = new List<string>();
        private int currentStepIndex;
        private int totalSteps;
        private bool wireGaugeCorrect;
        private int connectionQualityScore;

        public int StepsCompleted => completedSteps.Count;
        public int TotalSteps => totalSteps;
        public int CurrentStepIndex => currentStepIndex;
        public int ConnectionQualityScore => connectionQualityScore;
        public bool WireGaugeCorrect => wireGaugeCorrect;

        public void Initialize(TaskDefinition definition, TaskMode mode)
        {
            this.definition = definition;
            this.mode = mode;
            Reset();
            if (definition.steps != null)
                totalSteps = definition.steps.Length;
        }

        public void Reset()
        {
            completedSteps.Clear();
            currentStepIndex = 0;
            totalSteps = 0;
            wireGaugeCorrect = false;
            connectionQualityScore = 0;
        }

        public bool CompleteStep(string stepId)
        {
            if (definition == null || definition.steps == null) return false;
            if (currentStepIndex >= definition.steps.Length)
            {
                Debug.LogWarning("[CircuitWiringTracker] All steps already completed");
                return false;
            }

            StepDefinition expectedStep = definition.steps[currentStepIndex];

            if (mode == TaskMode.Test)
            {
                completedSteps.Add(stepId);
                currentStepIndex++;
                AudioManager.Instance.PlaySnapSound();
                return true;
            }

            if (stepId == expectedStep.id)
            {
                completedSteps.Add(stepId);
                currentStepIndex++;
                AudioManager.Instance.PlayCorrectSound();
                Debug.Log($"[CircuitWiringTracker] Step completed: {stepId} ({currentStepIndex}/{totalSteps})");
                return true;
            }
            else
            {
                Debug.Log($"[CircuitWiringTracker] Wrong step: expected {expectedStep.id}, got {stepId}");
                if (mode == TaskMode.Practice)
                {
                    AudioManager.Instance.PlayIncorrectSound();
                    var hintPanel = Object.FindObjectOfType<UI.HintPanel>();
                    if (hintPanel != null)
                        hintPanel.ShowHint($"Wrong step. Next: {expectedStep.description}\nNEC {expectedStep.necCode}");
                }
                return false;
            }
        }

        public bool AllStepsCompleted()
        {
            return currentStepIndex >= totalSteps && totalSteps > 0;
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
            if (definition == null || definition.steps == null) return null;
            if (currentStepIndex >= definition.steps.Length) return null;
            return definition.steps[currentStepIndex];
        }

        public List<string> GetCompletedStepIds()
        {
            return new List<string>(completedSteps);
        }

        public float GetScore()
        {
            if (totalSteps == 0) return 0f;
            float stepRatio = (float)completedSteps.Count / totalSteps;
            float gaugeBonus = wireGaugeCorrect ? 0.1f : -0.1f;
            float qualityBonus = (connectionQualityScore / (float)Mathf.Max(totalSteps, 1)) * 0.1f;

            int correctOrder = 0;
            if (definition != null && definition.steps != null)
            {
                for (int i = 0; i < completedSteps.Count && i < definition.steps.Length; i++)
                {
                    if (completedSteps[i] == definition.steps[i].id)
                        correctOrder++;
                }
            }
            float orderRatio = totalSteps > 0 ? (float)correctOrder / totalSteps : 0f;
            return Mathf.Clamp((stepRatio * 0.5f + orderRatio * 0.3f + gaugeBonus + qualityBonus) * 100f, 0f, 100f);
        }

        public float GetCompletionPercentage()
        {
            if (totalSteps == 0) return 0f;
            return (float)completedSteps.Count / totalSteps * 100f;
        }

        public TaskResult BuildResult()
        {
            return ScoreManager.Instance.CalculateCircuitWiringScore(
                completedSteps,
                definition.steps,
                wireGaugeCorrect,
                connectionQualityScore,
                0f, // timeUsed filled by TaskManager
                definition.timeLimit,
                mode
            );
        }
    }
}
