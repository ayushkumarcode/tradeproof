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

        // --- Panel Inspection Score ---

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

            float foundRatio = totalViolations > 0 ? (float)violationsFound / totalViolations : 0f;
            float falsePositivePenalty = falsePositives * 10f;

            float timeBonus = 0f;
            if (mode == TaskMode.Test && timeLimit > 0)
            {
                float timeRatio = timeUsed / timeLimit;
                if (timeRatio < 0.5f)
                    timeBonus = 10f;
                else if (timeRatio < 0.75f)
                    timeBonus = 5f;
            }

            result.score = Mathf.Clamp((foundRatio * 100f) - falsePositivePenalty + timeBonus, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            result.breakdown = new ScoreBreakdown();
            result.breakdown.violationsFound = violationsFound;
            result.breakdown.totalViolations = totalViolations;
            result.breakdown.falsePositives = falsePositives;
            result.breakdown.timeUsed = timeUsed;
            result.breakdown.timeBonus = timeBonus;

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
                       $"-- {(result.passed ? "PASSED" : "FAILED")}");

            return result;
        }

        // --- Circuit Wiring Score ---

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

            float stepRatio = totalSteps > 0 ? (float)stepsCompleted / totalSteps : 0f;
            float stepScore = stepRatio * 50f;

            int correctOrder = 0;
            if (allSteps != null)
            {
                for (int i = 0; i < completedStepIds.Count && i < allSteps.Length; i++)
                {
                    if (completedStepIds[i] == allSteps[i].id)
                        correctOrder++;
                }
            }
            float orderRatio = totalSteps > 0 ? (float)correctOrder / totalSteps : 0f;
            float orderScore = orderRatio * 30f;

            float gaugeScore = wireGaugeCorrect ? 10f : 0f;

            float maxQuality = totalSteps > 0 ? totalSteps * 10f : 10f;
            float qualityRatio = Mathf.Clamp01(connectionQualityScore / maxQuality);
            float qualityScore = qualityRatio * 10f;

            result.score = Mathf.Clamp(stepScore + orderScore + gaugeScore + qualityScore, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            result.breakdown = new ScoreBreakdown();
            result.breakdown.stepsCompleted = stepsCompleted;
            result.breakdown.totalSteps = totalSteps;
            result.breakdown.stepsInCorrectOrder = correctOrder;
            result.breakdown.wireGaugeCorrect = wireGaugeCorrect;
            result.breakdown.connectionQuality = connectionQualityScore;
            result.breakdown.timeUsed = timeUsed;

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
                       $"-- {(result.passed ? "PASSED" : "FAILED")}");

            return result;
        }

        // --- Generic Step-Based Score (for new tasks like outlet, switch, GFCI) ---

        public TaskResult CalculateGenericStepScore(
            List<string> completedStepIds,
            StepDefinition[] allSteps,
            float timeUsed,
            float timeLimit,
            TaskMode mode,
            Dictionary<string, float> bonusScores = null)
        {
            TaskResult result = new TaskResult();
            result.mode = mode;

            int totalSteps = allSteps != null ? allSteps.Length : 0;
            int stepsCompleted = completedStepIds.Count;

            // 60% step completion
            float stepRatio = totalSteps > 0 ? (float)stepsCompleted / totalSteps : 0f;
            float stepScore = stepRatio * 60f;

            // 25% step order
            int correctOrder = 0;
            if (allSteps != null)
            {
                for (int i = 0; i < completedStepIds.Count && i < allSteps.Length; i++)
                {
                    if (completedStepIds[i] == allSteps[i].id)
                        correctOrder++;
                }
            }
            float orderRatio = totalSteps > 0 ? (float)correctOrder / totalSteps : 0f;
            float orderScore = orderRatio * 25f;

            // 5% time bonus
            float timeBonus = 0f;
            if (mode == TaskMode.Test && timeLimit > 0)
            {
                float timeRatio = timeUsed / timeLimit;
                if (timeRatio < 0.5f) timeBonus = 5f;
                else if (timeRatio < 0.75f) timeBonus = 2.5f;
            }

            // 10% bonus scores (task-specific)
            float totalBonus = 0f;
            if (bonusScores != null)
            {
                foreach (var bonus in bonusScores.Values)
                    totalBonus += bonus;
                totalBonus = Mathf.Clamp(totalBonus, 0f, 10f);
            }

            result.score = Mathf.Clamp(stepScore + orderScore + timeBonus + totalBonus, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            result.breakdown = new ScoreBreakdown();
            result.breakdown.stepsCompleted = stepsCompleted;
            result.breakdown.totalSteps = totalSteps;
            result.breakdown.stepsInCorrectOrder = correctOrder;
            result.breakdown.timeUsed = timeUsed;
            result.breakdown.timeBonus = timeBonus;

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

            return result;
        }

        // --- Measurement Score (conduit bending) ---

        public TaskResult CalculateMeasurementScore(
            List<MeasurementResult> measurements,
            MeasurementDefinition[] allMeasurements,
            float timeUsed,
            float timeLimit,
            TaskMode mode)
        {
            TaskResult result = new TaskResult();
            result.mode = mode;

            int totalMeasurements = allMeasurements != null ? allMeasurements.Length : 0;
            int completed = measurements.Count;

            // 70% measurement accuracy
            float totalAccuracy = 0f;
            foreach (var m in measurements)
                totalAccuracy += m.accuracy;
            float avgAccuracy = completed > 0 ? totalAccuracy / completed : 0f;
            float accuracyScore = avgAccuracy * 70f;

            // 20% completion
            float completionRatio = totalMeasurements > 0 ? (float)completed / totalMeasurements : 0f;
            float completionScore = completionRatio * 20f;

            // 10% time bonus
            float timeBonus = 0f;
            if (mode == TaskMode.Test && timeLimit > 0)
            {
                float timeRatio = timeUsed / timeLimit;
                if (timeRatio < 0.5f) timeBonus = 10f;
                else if (timeRatio < 0.75f) timeBonus = 5f;
            }

            result.score = Mathf.Clamp(accuracyScore + completionScore + timeBonus, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            result.breakdown = new ScoreBreakdown();
            result.breakdown.measurementAccuracy = avgAccuracy;
            result.breakdown.measurementsCompleted = completed;
            result.breakdown.totalMeasurements = totalMeasurements;
            result.breakdown.timeUsed = timeUsed;
            result.breakdown.timeBonus = timeBonus;

            result.necCodeResults = new List<NECCodeResult>();

            return result;
        }

        // --- Troubleshooting Score ---

        public TaskResult CalculateTroubleshootingScore(
            int diagnosticPoints,
            int maxDiagnosticPoints,
            bool faultIdentified,
            bool faultRepaired,
            List<string> completedStepIds,
            StepDefinition[] allSteps,
            float timeUsed,
            float timeLimit,
            TaskMode mode)
        {
            TaskResult result = new TaskResult();
            result.mode = mode;

            // 30% diagnostic questioning
            float diagnosticRatio = maxDiagnosticPoints > 0 ? (float)diagnosticPoints / maxDiagnosticPoints : 0f;
            float diagnosticScore = diagnosticRatio * 30f;

            // 25% fault identification
            float faultIdScore = faultIdentified ? 25f : 0f;

            // 25% fault repair
            float faultRepairScore = faultRepaired ? 25f : 0f;

            // 10% step completion
            int totalSteps = allSteps != null ? allSteps.Length : 0;
            float stepRatio = totalSteps > 0 ? (float)completedStepIds.Count / totalSteps : 0f;
            float stepScore = stepRatio * 10f;

            // 10% time bonus
            float timeBonus = 0f;
            if (mode == TaskMode.Test && timeLimit > 0)
            {
                float timeRatio = timeUsed / timeLimit;
                if (timeRatio < 0.5f) timeBonus = 10f;
                else if (timeRatio < 0.75f) timeBonus = 5f;
            }

            result.score = Mathf.Clamp(diagnosticScore + faultIdScore + faultRepairScore + stepScore + timeBonus, 0f, 100f);
            result.passed = result.score >= PASSING_THRESHOLD;

            result.breakdown = new ScoreBreakdown();
            result.breakdown.diagnosticPoints = diagnosticPoints;
            result.breakdown.maxDiagnosticPoints = maxDiagnosticPoints;
            result.breakdown.faultIdentified = faultIdentified;
            result.breakdown.faultRepaired = faultRepaired;
            result.breakdown.stepsCompleted = completedStepIds.Count;
            result.breakdown.totalSteps = totalSteps;
            result.breakdown.timeUsed = timeUsed;
            result.breakdown.timeBonus = timeBonus;

            result.necCodeResults = new List<NECCodeResult>();

            return result;
        }

        // --- Utility ---

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
            if (score >= 80f) return new Color(0.2f, 0.8f, 0.2f);
            if (score >= 60f) return new Color(0.9f, 0.7f, 0.1f);
            return new Color(0.9f, 0.2f, 0.2f);
        }
    }

    // Helper class for measurement results
    [System.Serializable]
    public class MeasurementResult
    {
        public string measurementId;
        public float actualValue;
        public float targetValue;
        public float accuracy; // 0-1, how close to target
    }
}
