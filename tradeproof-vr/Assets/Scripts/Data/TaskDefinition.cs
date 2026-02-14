using UnityEngine;
using System;

namespace TradeProof.Data
{
    [Serializable]
    public class TaskDefinition
    {
        public string taskId;
        public string taskName;
        public string description;
        public string difficulty;
        public string badgeAwarded;
        public float timeLimit;
        public float passingScore;
        public ViolationDefinition[] violations;
        public StepDefinition[] steps;
    }

    [Serializable]
    public class ViolationDefinition
    {
        public string id;
        public string type;
        public string necCode;
        public string description;
        public ViolationPosition localPosition;
        public string hintText;
        public string severity;
    }

    [Serializable]
    public class ViolationPosition
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class StepDefinition
    {
        public string id;
        public int order;
        public string description;
        public string necCode;
        public string action;
        public string targetComponent;
        public string requiredWireGauge;
        public string requiredWireColor;
        public StepValidation validation;
        public string hintText;
    }

    [Serializable]
    public class StepValidation
    {
        public string type;
        public string expectedValue;
        public string snapPointId;
        public float tolerance;
    }

    [Serializable]
    public class TaskResult
    {
        public string taskId;
        public string taskName;
        public Training.TaskMode mode;
        public string badgeId;
        public float score;
        public bool passed;
        public ScoreBreakdown breakdown;
        public System.Collections.Generic.List<NECCodeResult> necCodeResults;
    }

    [Serializable]
    public class ScoreBreakdown
    {
        // Panel inspection
        public int violationsFound;
        public int totalViolations;
        public int falsePositives;

        // Circuit wiring
        public int stepsCompleted;
        public int totalSteps;
        public int stepsInCorrectOrder;
        public bool wireGaugeCorrect;
        public int connectionQuality;

        // Shared
        public float timeUsed;
        public float timeBonus;
    }

    [Serializable]
    public class NECCodeResult
    {
        public string necCode;
        public string description;
        public bool identified;
        public string severity;
    }
}
