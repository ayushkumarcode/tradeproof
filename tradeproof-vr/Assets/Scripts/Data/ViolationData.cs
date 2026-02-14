using UnityEngine;
using System;
using System.Collections.Generic;

namespace TradeProof.Data
{
    [Serializable]
    public class ViolationType
    {
        public string typeId;
        public string displayName;
        public string necCode;
        public string category;
        public string defaultSeverity;
        public string visualDescription;
    }

    public static class ViolationDatabase
    {
        private static Dictionary<string, ViolationType> violationTypes;

        public static void Initialize()
        {
            violationTypes = new Dictionary<string, ViolationType>();

            AddViolationType(new ViolationType
            {
                typeId = "double-tapped-breaker",
                displayName = "Double-Tapped Breaker",
                necCode = "408.41",
                category = "breaker",
                defaultSeverity = "critical",
                visualDescription = "Two or more conductors connected to a single breaker terminal not rated for multiple connections"
            });

            AddViolationType(new ViolationType
            {
                typeId = "missing-knockout-cover",
                displayName = "Missing Knockout Cover",
                necCode = "408.7",
                category = "enclosure",
                defaultSeverity = "major",
                visualDescription = "Open knockout hole in the panel enclosure without a filler plate"
            });

            AddViolationType(new ViolationType
            {
                typeId = "incorrect-wire-gauge",
                displayName = "Incorrect Wire Gauge",
                necCode = "310.16",
                category = "wiring",
                defaultSeverity = "critical",
                visualDescription = "Wire gauge does not match the breaker amperage rating per NEC Table 310.16"
            });

            AddViolationType(new ViolationType
            {
                typeId = "missing-directory",
                displayName = "Missing Panel Directory",
                necCode = "408.4",
                category = "labeling",
                defaultSeverity = "major",
                visualDescription = "Panel directory (circuit identification) is missing or incomplete"
            });

            AddViolationType(new ViolationType
            {
                typeId = "improper-grounding",
                displayName = "Improper Grounding",
                necCode = "250.50",
                category = "grounding",
                defaultSeverity = "critical",
                visualDescription = "Grounding electrode system is incomplete or improperly connected"
            });

            AddViolationType(new ViolationType
            {
                typeId = "overfilled-panel",
                displayName = "Overfilled Panel",
                necCode = "408.54",
                category = "enclosure",
                defaultSeverity = "major",
                visualDescription = "Panel has more breakers installed than the rated number of spaces"
            });

            AddViolationType(new ViolationType
            {
                typeId = "incorrect-outlet-rating",
                displayName = "Incorrect Outlet Rating",
                necCode = "210.21",
                category = "outlet",
                defaultSeverity = "major",
                visualDescription = "Outlet amperage rating does not match the circuit breaker and wire gauge"
            });
        }

        private static void AddViolationType(ViolationType vt)
        {
            violationTypes[vt.typeId] = vt;
        }

        public static ViolationType GetViolationType(string typeId)
        {
            if (violationTypes == null)
                Initialize();

            if (violationTypes.TryGetValue(typeId, out ViolationType vt))
            {
                return vt;
            }
            return null;
        }

        public static List<ViolationType> GetAllViolationTypes()
        {
            if (violationTypes == null)
                Initialize();

            return new List<ViolationType>(violationTypes.Values);
        }

        public static List<ViolationType> GetViolationsByCategory(string category)
        {
            if (violationTypes == null)
                Initialize();

            List<ViolationType> results = new List<ViolationType>();
            foreach (var vt in violationTypes.Values)
            {
                if (vt.category == category)
                {
                    results.Add(vt);
                }
            }
            return results;
        }

        public static Color GetSeverityColor(string severity)
        {
            switch (severity)
            {
                case "critical":
                    return new Color(0.9f, 0.1f, 0.1f, 1f); // Red
                case "major":
                    return new Color(0.9f, 0.6f, 0.1f, 1f); // Orange
                case "minor":
                    return new Color(0.9f, 0.9f, 0.1f, 1f); // Yellow
                default:
                    return Color.white;
            }
        }
    }
}
