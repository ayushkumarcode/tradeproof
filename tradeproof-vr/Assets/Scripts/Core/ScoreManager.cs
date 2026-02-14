using UnityEngine;
using System.Collections.Generic;
using TradeProof.Data;
using TradeProof.Training;

namespace TradeProof.Core
{
    public class ScoreManager : MonoBehaviour
    {
        private static ScoreManager _instance;
        public static ScoreManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ScoreManager");
                    _instance = go.AddComponent<ScoreManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public const float PASSING_THRESHOLD = 80f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public TaskResult CalculatePanelInspectionScore(
            int violationsFound,
            int totalViolations,
            int falsePositives,
            List<string> identifiedViolationIds,
            List<ViolationDefinition> allViolations,
            float timeUsed,
            float timeLimit,
            TaskMode mode)
        {
            TaskResult result = new TaskResult();
            result.mode = mode;

            // Base score: percentage of violations found
            float foundRatio = totalViolations > 0 ? (float)violationsFound / totalViolations : 0f;

            // False positive penalty: -10% per false positive
            float falsePositivePenalty = falsePositives * 10f;

            // Time bonus for test mode: up to +10% for finishing with more than half time remaining
            float timeBonus = 0f;
            if (mode == TaskMode.Test && timeLimit > 0)
            {
                float timeRatio = timeUsed / timeLimit;
                if (timeRatio < 0.5f)
                {
                    timeBonus = 10f;
                }
                else if (timeRatio < 0.75f)
                {
                    timeBonus = 5f;
                }
            }

            result.score = Mathf.Clamp((foundRatio * 100f) - falsePositivePenalty + timeBonus, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            // Build breakdown
            result.breakdown = new ScoreBreakdown();
            result.breakdown.violationsFound = violationsFound;
            result.breakdown.totalViolations = totalViolations;
            result.breakdown.falsePositives = falsePositives;
            result.breakdown.timeUsed = timeUsed;
            result.breakdown.timeBonus = timeBonus;

            // Build NEC code details
            result.necCodeResults = new List<NECCodeResult>();
            if (allViolations != null)
            {
                foreach (var violation in allViolations)
                {
                    NECCodeResult codeResult = new NECCodeResult();
                    codeResult.necCode = violation.necCode;
                    codeResult.description = violation.description;
                    codeResult.identified = identifiedViolationIds.Contains(violation.id);
                    codeResult.severity = violation.severity;
                    result.necCodeResults.Add(codeResult);
                }
            }

            Debug.Log($"[ScoreManager] Panel Inspection Score: {result.score:F1}% " +
                       $"({violationsFound}/{totalViolations} found, {falsePositives} false positives) " +
                       $"— {(result.passed ? "PASSED" : "FAILED")}");

            return result;
        }

        public TaskResult CalculateCircuitWiringScore(
            List<string> completedStepIds,
            StepDefinition[] allSteps,
            bool wireGaugeCorrect,
            int connectionQualityScore,
            float timeUsed,
            float timeLimit,
            TaskMode mode)
        {
            TaskResult result = new TaskResult();
            result.mode = mode;

            int totalSteps = allSteps != null ? allSteps.Length : 0;
            int stepsCompleted = completedStepIds.Count;

            // Step completion ratio: 50% of score
            float stepRatio = totalSteps > 0 ? (float)stepsCompleted / totalSteps : 0f;
            float stepScore = stepRatio * 50f;

            // Step order correctness: 30% of score
            int correctOrder = 0;
            if (allSteps != null)
            {
                for (int i = 0; i < completedStepIds.Count && i < allSteps.Length; i++)
                {
                    if (completedStepIds[i] == allSteps[i].id)
                    {
                        correctOrder++;
                    }
                }
            }
            float orderRatio = totalSteps > 0 ? (float)correctOrder / totalSteps : 0f;
            float orderScore = orderRatio * 30f;

            // Wire gauge correctness: 10% of score
            float gaugeScore = wireGaugeCorrect ? 10f : 0f;

            // Connection quality: 10% of score
            float maxQuality = totalSteps > 0 ? totalSteps * 10f : 10f;
            float qualityRatio = Mathf.Clamp01(connectionQualityScore / maxQuality);
            float qualityScore = qualityRatio * 10f;

            result.score = Mathf.Clamp(stepScore + orderScore + gaugeScore + qualityScore, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            // Build breakdown
            result.breakdown = new ScoreBreakdown();
            result.breakdown.stepsCompleted = stepsCompleted;
            result.breakdown.totalSteps = totalSteps;
            result.breakdown.stepsInCorrectOrder = correctOrder;
            result.breakdown.wireGaugeCorrect = wireGaugeCorrect;
            result.breakdown.connectionQuality = connectionQualityScore;
            result.breakdown.timeUsed = timeUsed;

            // Build NEC code results for wiring
            result.necCodeResults = new List<NECCodeResult>();
            if (allSteps != null)
            {
                HashSet<string> addedCodes = new HashSet<string>();
                foreach (var step in allSteps)
                {
                    if (!string.IsNullOrEmpty(step.necCode) && addedCodes.Add(step.necCode))
                    {
                        NECCodeResult codeResult = new NECCodeResult();
                        codeResult.necCode = step.necCode;
                        codeResult.description = step.description;
                        codeResult.identified = completedStepIds.Contains(step.id);
                        codeResult.severity = "standard";
                        result.necCodeResults.Add(codeResult);
                    }
                }
            }

            Debug.Log($"[ScoreManager] Circuit Wiring Score: {result.score:F1}% " +
                       $"(Steps: {stepsCompleted}/{totalSteps}, Order: {correctOrder}/{totalSteps}, " +
                       $"Gauge: {wireGaugeCorrect}, Quality: {connectionQualityScore}) " +
                       $"— {(result.passed ? "PASSED" : "FAILED")}");

            return result;
        }

        public string GetGradeFromScore(float score)
        {
            if (score >= 95f) return "A+";
            if (score >= 90f) return "A";
            if (score >= 85f) return "B+";
            if (score >= 80f) return "B";
            if (score >= 75f) return "C+";
            if (score >= 70f) return "C";
            if (score >= 60f) return "D";
            return "F";
        }

        public Color GetGradeColor(float score)
        {
            if (score >= 80f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (score >= 60f) return new Color(0.9f, 0.7f, 0.1f); // Yellow
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }
    }
}
