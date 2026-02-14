using UnityEngine;
using System;
using System.Collections.Generic;

namespace TradeProof.Data
{
    [Serializable]
    public class NECCodeEntry
    {
        public string code;
        public string title;
        public string section;
        public string simplifiedExplanation;
        public string visualHintText;
        public string category;
        public string[] relatedCodes;
    }

    [Serializable]
    public class NECCodeDatabase
    {
        public NECCodeEntry[] codes;
    }

    public static class NECDatabase
    {
        private static Dictionary<string, NECCodeEntry> codeMap;
        private static bool initialized = false;

        public static void Initialize()
        {
            if (initialized) return;

            codeMap = new Dictionary<string, NECCodeEntry>();

            // Try to load from JSON resource first
            TextAsset jsonAsset = Resources.Load<TextAsset>("NECCodes/nec_codes");
            if (jsonAsset != null)
            {
                NECCodeDatabase db = JsonUtility.FromJson<NECCodeDatabase>(jsonAsset.text);
                if (db != null && db.codes != null)
                {
                    foreach (var entry in db.codes)
                    {
                        codeMap[entry.code] = entry;
                    }
                    initialized = true;
                    Debug.Log($"[NECDatabase] Loaded {codeMap.Count} NEC codes from JSON");
                    return;
                }
            }

            // Fallback: hard-coded entries
            InitializeHardCoded();
            initialized = true;
            Debug.Log($"[NECDatabase] Initialized {codeMap.Count} NEC codes from hard-coded data");
        }

        private static void InitializeHardCoded()
        {
            AddCode(new NECCodeEntry
            {
                code = "310.16",
                title = "Conductor Ampacity — Allowable Ampacities",
                section = "Article 310 — Conductors for General Wiring",
                simplifiedExplanation = "This table specifies the maximum current a wire can safely carry based on its gauge (size) and insulation type. For example, 14 AWG copper wire is rated for 15A, 12 AWG for 20A, and 10 AWG for 30A. Using undersized wire creates a fire hazard.",
                visualHintText = "Check wire thickness — does it match the breaker amp rating? 14 AWG = 15A max, 12 AWG = 20A max, 10 AWG = 30A max.",
                category = "wiring",
                relatedCodes = new[] { "240.4", "210.3" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.4",
                title = "Circuit Directory or Circuit Identification",
                section = "Article 408 — Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "Every panel must have a legible directory that identifies each circuit by location and purpose. The directory must be on the panel door or immediately adjacent. Blank or illegible directories are violations.",
                visualHintText = "Look at the panel door — is there a label listing what each breaker controls?",
                category = "labeling",
                relatedCodes = new[] { "408.4(A)", "408.4(B)" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.7",
                title = "Unused Openings",
                section = "Article 408 — Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "All unused knockout openings in the panel enclosure must be closed with covers rated for the enclosure. Open holes expose energized components and allow pests/debris inside the panel.",
                visualHintText = "Check the sides and bottom of the panel — are all knockout holes covered?",
                category = "enclosure",
                relatedCodes = new[] { "110.12", "312.5" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.41",
                title = "Grounded and Ungrounded Conductor Terminations",
                section = "Article 408 — Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "Each breaker terminal is designed for one conductor only, unless the breaker is specifically listed and labeled for multiple conductors. Connecting two wires to one breaker (double-tapping) can cause loose connections, arcing, and fire.",
                visualHintText = "Look at each breaker — are there two wires going into any single terminal?",
                category = "breaker",
                relatedCodes = new[] { "110.14" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.54",
                title = "Maximum Number of Overcurrent Devices",
                section = "Article 408 — Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "A panel must not contain more overcurrent devices (breakers) than the number it is designed and listed for. The maximum number is marked on the panel. Exceeding this limit is a violation and safety hazard.",
                visualHintText = "Count the breakers — does the panel label say it supports that many?",
                category = "enclosure",
                relatedCodes = new[] { "408.54" }
            });

            AddCode(new NECCodeEntry
            {
                code = "250.50",
                title = "Grounding Electrode System",
                section = "Article 250 — Grounding and Bonding",
                simplifiedExplanation = "A grounding electrode system must include all available electrodes (ground rods, water pipe, building steel, concrete-encased electrode). All electrodes must be bonded together. Proper grounding protects against electric shock and lightning.",
                visualHintText = "Check the grounding connections — is the ground wire properly attached to the ground bar and does it connect to the grounding electrode?",
                category = "grounding",
                relatedCodes = new[] { "250.52", "250.53", "250.64" }
            });

            AddCode(new NECCodeEntry
            {
                code = "200.6",
                title = "Means of Identifying Grounded Conductors",
                section = "Article 200 — Use and Identification of Grounded Conductors",
                simplifiedExplanation = "The grounded (neutral) conductor must be identified by a continuous white or gray outer finish, or by three continuous white or gray stripes along the conductor's length. This ensures the neutral can always be distinguished from hot conductors.",
                visualHintText = "The neutral wire should be white or gray. Connect it to the neutral (silver-colored) bus bar, not the hot bus.",
                category = "wiring",
                relatedCodes = new[] { "200.7", "210.5" }
            });

            AddCode(new NECCodeEntry
            {
                code = "210.21",
                title = "Outlet Devices — Receptacle Rating",
                section = "Article 210 — Branch Circuits",
                simplifiedExplanation = "Receptacle outlets must have an amperage rating that matches the circuit. On a 20A circuit, 15A or 20A receptacles are allowed. On a 15A circuit, only 15A receptacles are allowed. Mismatched ratings are a violation.",
                visualHintText = "Check the outlet face — does the amperage marking match the circuit breaker?",
                category = "outlet",
                relatedCodes = new[] { "210.21(B)", "Table 210.21(B)(3)" }
            });
        }

        private static void AddCode(NECCodeEntry entry)
        {
            codeMap[entry.code] = entry;
        }

        public static NECCodeEntry GetCode(string code)
        {
            if (!initialized) Initialize();

            if (codeMap.TryGetValue(code, out NECCodeEntry entry))
            {
                return entry;
            }
            Debug.LogWarning($"[NECDatabase] NEC code not found: {code}");
            return null;
        }

        public static bool HasCode(string code)
        {
            if (!initialized) Initialize();
            return codeMap.ContainsKey(code);
        }

        public static List<NECCodeEntry> GetAllCodes()
        {
            if (!initialized) Initialize();
            return new List<NECCodeEntry>(codeMap.Values);
        }

        public static List<NECCodeEntry> GetCodesByCategory(string category)
        {
            if (!initialized) Initialize();

            List<NECCodeEntry> results = new List<NECCodeEntry>();
            foreach (var entry in codeMap.Values)
            {
                if (entry.category == category)
                {
                    results.Add(entry);
                }
            }
            return results;
        }

        public static string GetFormattedReference(string code)
        {
            NECCodeEntry entry = GetCode(code);
            if (entry == null) return $"NEC {code}";
            return $"NEC {entry.code}: {entry.title}";
        }

        public static string GetQuickExplanation(string code)
        {
            NECCodeEntry entry = GetCode(code);
            if (entry == null) return "Code reference not available.";
            return entry.simplifiedExplanation;
        }
    }
}
