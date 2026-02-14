using UnityEngine;
using System.Collections.Generic;

namespace TradeProof.Electrical
{
    /// <summary>
    /// Static reference for NEC codes used throughout the training tasks.
    /// Each entry provides the code section, title, simplified explanation, and visual hint text.
    /// </summary>
    public static class NECCodeReference
    {
        public struct CodeEntry
        {
            public string Section;
            public string Title;
            public string SimplifiedExplanation;
            public string VisualHintText;

            public CodeEntry(string section, string title, string explanation, string hint)
            {
                Section = section;
                Title = title;
                SimplifiedExplanation = explanation;
                VisualHintText = hint;
            }
        }

        private static Dictionary<string, CodeEntry> codes;

        static NECCodeReference()
        {
            codes = new Dictionary<string, CodeEntry>();

            codes["310.16"] = new CodeEntry(
                "Article 310 — Conductors for General Wiring",
                "Allowable Ampacities of Insulated Conductors",
                "Specifies maximum current a wire can carry based on gauge and insulation.\n" +
                "14 AWG = 15A, 12 AWG = 20A, 10 AWG = 30A (copper, 60C rating).\n" +
                "Using undersized wire is a serious fire hazard.",
                "Check wire thickness vs breaker rating: 14 AWG for 15A, 12 AWG for 20A, 10 AWG for 30A."
            );

            codes["408.4"] = new CodeEntry(
                "Article 408 — Switchboards, Switchgear, and Panelboards",
                "Circuit Directory or Circuit Identification",
                "Every circuit breaker panel must have a directory identifying each circuit.\n" +
                "The directory must be legible and mounted on the panel door or adjacent.\n" +
                "Each circuit must be labeled by its purpose and location served.",
                "Look at the panel door — is there a complete, legible label for every breaker?"
            );

            codes["408.7"] = new CodeEntry(
                "Article 408 — Switchboards, Switchgear, and Panelboards",
                "Unused Openings",
                "All unused openings (knockout holes) in panel enclosures must be covered.\n" +
                "Open holes expose live components to contact, pests, and debris.\n" +
                "Install listed knockout filler plates.",
                "Check sides and bottom of panel for any open/uncovered knockout holes."
            );

            codes["408.41"] = new CodeEntry(
                "Article 408 — Switchboards, Switchgear, and Panelboards",
                "Grounded and Ungrounded Conductor Terminations",
                "Each breaker terminal accepts ONE conductor only, unless specifically listed\n" +
                "and labeled for multiple conductors. Double-tapping (two wires on one breaker)\n" +
                "causes loose connections, arcing, and potential fire.",
                "Examine each breaker terminal — are two wires sharing one terminal?"
            );

            codes["408.54"] = new CodeEntry(
                "Article 408 — Switchboards, Switchgear, and Panelboards",
                "Maximum Number of Overcurrent Devices",
                "A panel must not contain more breakers than its listed/labeled maximum.\n" +
                "The number of spaces is marked on the panel. Exceeding this number is\n" +
                "a code violation and may overload the panel bus.",
                "Count all breakers including tandems — does it exceed the panel's labeled max spaces?"
            );

            codes["250.50"] = new CodeEntry(
                "Article 250 — Grounding and Bonding",
                "Grounding Electrode System",
                "The grounding electrode system must use all available electrodes:\n" +
                "ground rods, metal water pipe, building steel, concrete-encased electrode.\n" +
                "All must be bonded together. Proper grounding is critical for safety.",
                "Trace the ground wire — does it connect to the ground bar AND to an electrode?"
            );

            codes["210.21"] = new CodeEntry(
                "Article 210 — Branch Circuits",
                "Outlet Devices — Receptacle Rating",
                "Receptacles must match the circuit amperage:\n" +
                "- 15A circuit: 15A receptacles only\n" +
                "- 20A circuit: 15A or 20A receptacles\n" +
                "- 30A+ circuit: must match exactly\n" +
                "20A receptacles have a T-shaped neutral slot.",
                "Check the receptacle face plate marking — does the amp rating match the circuit?"
            );
        }

        public static CodeEntry GetCode(string codeNumber)
        {
            if (codes.TryGetValue(codeNumber, out CodeEntry entry))
            {
                return entry;
            }

            return new CodeEntry(
                "Unknown",
                $"NEC {codeNumber}",
                "Code reference not available in this training module.",
                "Refer to the National Electrical Code handbook."
            );
        }

        public static bool HasCode(string codeNumber)
        {
            return codes.ContainsKey(codeNumber);
        }

        public static List<string> GetAllCodeNumbers()
        {
            return new List<string>(codes.Keys);
        }

        public static string GetFormattedReference(string codeNumber)
        {
            CodeEntry entry = GetCode(codeNumber);
            return $"NEC {codeNumber} — {entry.Title}\n{entry.Section}\n\n{entry.SimplifiedExplanation}";
        }

        public static string GetShortReference(string codeNumber)
        {
            CodeEntry entry = GetCode(codeNumber);
            return $"NEC {codeNumber}: {entry.Title}";
        }

        /// <summary>
        /// Wire gauge ampacity table per NEC 310.16 (copper, 60C insulation).
        /// </summary>
        public static int GetWireAmpacity(int gaugeAWG)
        {
            switch (gaugeAWG)
            {
                case 18: return 7;
                case 16: return 10;
                case 14: return 15;
                case 12: return 20;
                case 10: return 30;
                case 8:  return 40;
                case 6:  return 55;
                case 4:  return 70;
                case 3:  return 85;
                case 2:  return 95;
                case 1:  return 110;
                default: return 0;
            }
        }

        /// <summary>
        /// Returns the minimum wire gauge required for a given amperage.
        /// </summary>
        public static int GetMinGaugeForAmperage(int amperage)
        {
            if (amperage <= 7)  return 18;
            if (amperage <= 10) return 16;
            if (amperage <= 15) return 14;
            if (amperage <= 20) return 12;
            if (amperage <= 30) return 10;
            if (amperage <= 40) return 8;
            if (amperage <= 55) return 6;
            if (amperage <= 70) return 4;
            if (amperage <= 85) return 3;
            if (amperage <= 95) return 2;
            return 1;
        }
    }
}
