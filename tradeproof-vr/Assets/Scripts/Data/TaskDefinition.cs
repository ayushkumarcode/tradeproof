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

        // Task type classification
        public string taskType; // "inspection", "wiring", "testing", "measurement", "troubleshooting"
        public string jobSiteType; // "residential-kitchen", "residential-garage", "residential-bathroom", "commercial-office"
        public string[] requiredTools; // ["multimeter", "voltage-tester", "conduit-bender"]
        public string[] necCodes; // All NEC codes tested by this task
        public string[] prerequisites; // Task IDs that must be completed first

        // Panel inspection data
        public ViolationDefinition[] violations;

        // Step-based task data
        public StepDefinition[] steps;

        // Measurement-based task data (conduit bending)
        public MeasurementDefinition[] measurements;

        // Troubleshooting task data
        public DialogueDefinition[] dialogues;
        public FaultDefinition[] faults;
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
        public string type; // "wire-gauge", "position", "snap", "action", "measurement", "dialogue"
        public string expectedValue;
        public string snapPointId;
        public float tolerance;
    }

    [Serializable]
    public class MeasurementDefinition
    {
        public string id;
        public string description;
        public string measurementType; // "angle", "distance", "stub-up"
        public float targetValue;
        public float tolerance;
        public string unit; // "degrees", "inches", "mm"
        public string necCode;
        public string hintText;
    }

    [Serializable]
    public class DialogueDefinition
    {
        public string id;
        public string speakerName;
        public string text;
        public DialogueChoice[] choices;
        public string nextDialogueId; // For linear dialogue
        public string condition; // Optional condition for branching
    }

    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextDialogueId;
        public int diagnosticPoints; // Points for asking good diagnostic questions
        public string responseText; // NPC response to this choice
    }

    [Serializable]
    public class FaultDefinition
    {
        public string id;
        public string faultType; // "loose-connection", "bad-splice", "tripped-gfci", "tripped-breaker", "broken-wire", "overloaded-circuit"
        public string description;
        public string affectedComponent;
        public string repairAction; // "reconnect-wire", "replace-wire-nut", "reset-gfci", "replace-outlet"
        public string hintText;
        public string necCode;
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
        public int xpEarned;
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

        // Circuit wiring / step-based tasks
        public int stepsCompleted;
        public int totalSteps;
        public int stepsInCorrectOrder;
        public bool wireGaugeCorrect;
        public int connectionQuality;

        // Measurement-based tasks
        public float measurementAccuracy;
        public int measurementsCompleted;
        public int totalMeasurements;

        // Troubleshooting tasks
        public int diagnosticPoints;
        public int maxDiagnosticPoints;
        public bool faultIdentified;
        public bool faultRepaired;

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
