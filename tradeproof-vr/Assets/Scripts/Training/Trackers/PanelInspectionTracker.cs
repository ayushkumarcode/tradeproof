using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TradeProof.Core;
using TradeProof.Data;

namespace TradeProof.Training.Trackers
{
    public class PanelInspectionTracker : ITaskTracker
    {
        private TaskDefinition definition;
        private TaskMode mode;
        private HashSet<string> identifiedViolations = new HashSet<string>();
        private HashSet<string> falsePositives = new HashSet<string>();
        private int totalViolations;

        public int ViolationsFound => identifiedViolations.Count;
        public int TotalViolations => totalViolations;
        public int FalsePositiveCount => falsePositives.Count;

        public void Initialize(TaskDefinition definition, TaskMode mode)
        {
            this.definition = definition;
            this.mode = mode;
            Reset();
            if (definition.violations != null)
                totalViolations = definition.violations.Length;
        }

        public void Reset()
        {
            identifiedViolations.Clear();
            falsePositives.Clear();
            totalViolations = 0;
        }

        public bool IdentifyViolation(string violationId)
        {
            if (definition == null) return false;

            bool isRealViolation = false;
            if (definition.violations != null)
            {
                foreach (var v in definition.violations)
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
                    Debug.Log($"[PanelInspectionTracker] Violation identified: {violationId}");
                    AudioManager.Instance.PlayCorrectSound();
                    return true;
                }
                return false;
            }
            else
            {
                falsePositives.Add(violationId);
                Debug.Log($"[PanelInspectionTracker] False positive: {violationId}");
                AudioManager.Instance.PlayIncorrectSound();
                return false;
            }
        }

        public bool IsViolationIdentified(string violationId)
        {
            return identifiedViolations.Contains(violationId);
        }

        public bool AllViolationsFound()
        {
            return identifiedViolations.Count >= totalViolations && totalViolations > 0;
        }

        public ViolationDefinition GetNextHint()
        {
            if (definition == null || definition.violations == null) return null;
            foreach (var violation in definition.violations)
            {
                if (!identifiedViolations.Contains(violation.id))
                    return violation;
            }
            return null;
        }

        public List<string> GetIdentifiedViolationIds()
        {
            return identifiedViolations.ToList();
        }

        public float GetScore()
        {
            if (totalViolations == 0) return 0f;
            float foundRatio = (float)identifiedViolations.Count / totalViolations;
            float falsePositivePenalty = falsePositives.Count * 0.1f;
            return Mathf.Clamp01(foundRatio - falsePositivePenalty) * 100f;
        }

        public float GetCompletionPercentage()
        {
            if (totalViolations == 0) return 0f;
            return (float)identifiedViolations.Count / totalViolations * 100f;
        }

        public TaskResult BuildResult()
        {
            return ScoreManager.Instance.CalculatePanelInspectionScore(
                identifiedViolations.Count,
                totalViolations,
                falsePositives.Count,
                identifiedViolations.ToList(),
                definition.violations != null ? new List<ViolationDefinition>(definition.violations) : new List<ViolationDefinition>(),
                0f, // timeUsed filled by TaskManager
                definition.timeLimit,
                mode
            );
        }
    }
}
