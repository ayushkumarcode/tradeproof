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

            InitializeHardCoded();
            initialized = true;
            Debug.Log($"[NECDatabase] Initialized {codeMap.Count} NEC codes from hard-coded data");
        }

        private static void InitializeHardCoded()
        {
            // --- Original codes ---

            AddCode(new NECCodeEntry
            {
                code = "310.16",
                title = "Conductor Ampacity -- Allowable Ampacities",
                section = "Article 310 -- Conductors for General Wiring",
                simplifiedExplanation = "This table specifies the maximum current a wire can safely carry based on its gauge (size) and insulation type. For example, 14 AWG copper wire is rated for 15A, 12 AWG for 20A, and 10 AWG for 30A. Using undersized wire creates a fire hazard.",
                visualHintText = "Check wire thickness -- does it match the breaker amp rating? 14 AWG = 15A max, 12 AWG = 20A max, 10 AWG = 30A max.",
                category = "wiring",
                relatedCodes = new[] { "240.4", "210.3" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.4",
                title = "Circuit Directory or Circuit Identification",
                section = "Article 408 -- Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "Every panel must have a legible directory that identifies each circuit by location and purpose. The directory must be on the panel door or immediately adjacent. Blank or illegible directories are violations.",
                visualHintText = "Look at the panel door -- is there a label listing what each breaker controls?",
                category = "labeling",
                relatedCodes = new[] { "408.4(A)", "408.4(B)" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.7",
                title = "Unused Openings",
                section = "Article 408 -- Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "All unused knockout openings in the panel enclosure must be closed with covers rated for the enclosure. Open holes expose energized components and allow pests/debris inside the panel.",
                visualHintText = "Check the sides and bottom of the panel -- are all knockout holes covered?",
                category = "enclosure",
                relatedCodes = new[] { "110.12", "312.5" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.41",
                title = "Grounded and Ungrounded Conductor Terminations",
                section = "Article 408 -- Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "Each breaker terminal is designed for one conductor only, unless the breaker is specifically listed and labeled for multiple conductors. Connecting two wires to one breaker (double-tapping) can cause loose connections, arcing, and fire.",
                visualHintText = "Look at each breaker -- are there two wires going into any single terminal?",
                category = "breaker",
                relatedCodes = new[] { "110.14" }
            });

            AddCode(new NECCodeEntry
            {
                code = "408.54",
                title = "Maximum Number of Overcurrent Devices",
                section = "Article 408 -- Switchboards, Switchgear, and Panelboards",
                simplifiedExplanation = "A panel must not contain more overcurrent devices (breakers) than the number it is designed and listed for. The maximum number is marked on the panel. Exceeding this limit is a violation and safety hazard.",
                visualHintText = "Count the breakers -- does the panel label say it supports that many?",
                category = "enclosure",
                relatedCodes = new[] { "408.54" }
            });

            AddCode(new NECCodeEntry
            {
                code = "250.50",
                title = "Grounding Electrode System",
                section = "Article 250 -- Grounding and Bonding",
                simplifiedExplanation = "A grounding electrode system must include all available electrodes (ground rods, water pipe, building steel, concrete-encased electrode). All electrodes must be bonded together. Proper grounding protects against electric shock and lightning.",
                visualHintText = "Check the grounding connections -- is the ground wire properly attached to the ground bar and does it connect to the grounding electrode?",
                category = "grounding",
                relatedCodes = new[] { "250.52", "250.53", "250.64" }
            });

            AddCode(new NECCodeEntry
            {
                code = "200.6",
                title = "Means of Identifying Grounded Conductors",
                section = "Article 200 -- Use and Identification of Grounded Conductors",
                simplifiedExplanation = "The grounded (neutral) conductor must be identified by a continuous white or gray outer finish, or by three continuous white or gray stripes along the conductor's length. This ensures the neutral can always be distinguished from hot conductors.",
                visualHintText = "The neutral wire should be white or gray. Connect it to the neutral (silver-colored) bus bar, not the hot bus.",
                category = "wiring",
                relatedCodes = new[] { "200.7", "210.5" }
            });

            AddCode(new NECCodeEntry
            {
                code = "210.21",
                title = "Outlet Devices -- Receptacle Rating",
                section = "Article 210 -- Branch Circuits",
                simplifiedExplanation = "Receptacle outlets must have an amperage rating that matches the circuit. On a 20A circuit, 15A or 20A receptacles are allowed. On a 15A circuit, only 15A receptacles are allowed. Mismatched ratings are a violation.",
                visualHintText = "Check the outlet face -- does the amperage marking match the circuit breaker?",
                category = "outlet",
                relatedCodes = new[] { "210.21(B)", "Table 210.21(B)(3)" }
            });

            // --- New codes for expanded tasks ---

            AddCode(new NECCodeEntry
            {
                code = "300.14",
                title = "Length of Free Conductors at Outlets, Junctions, and Switch Points",
                section = "Article 300 -- General Requirements for Wiring Methods and Materials",
                simplifiedExplanation = "At each outlet, junction, and switch point, at least 150mm (6 inches) of free conductor must be left for splicing or connecting to devices. At least 75mm (3 inches) must extend outside the box opening. Short wires make connections difficult and unreliable.",
                visualHintText = "Check wire length in the box -- at least 6 inches of free wire should extend for making connections. Measure from where the wire enters the box.",
                category = "wiring",
                relatedCodes = new[] { "314.16", "300.15" }
            });

            AddCode(new NECCodeEntry
            {
                code = "110.14",
                title = "Electrical Connections -- Termination Provisions",
                section = "Article 110 -- Requirements for Electrical Installations",
                simplifiedExplanation = "Connections must be made with devices identified for the wire type and size. Terminals for more than one conductor must be so identified. Torque values specified by the manufacturer must be followed. Loose connections cause arcing, overheating, and fire.",
                visualHintText = "Check all screw terminals -- are they properly tightened? Use the correct torque. Each terminal should have only one wire unless rated for multiple.",
                category = "wiring",
                relatedCodes = new[] { "110.14(A)", "110.14(B)", "408.41" }
            });

            AddCode(new NECCodeEntry
            {
                code = "404.2",
                title = "Switch Connections",
                section = "Article 404 -- Switches",
                simplifiedExplanation = "Three-way and four-way switches must be wired so that switching occurs only in the ungrounded (hot) conductor. The common terminal of a 3-way switch connects to either the line hot or the load, while the traveler terminals connect to the traveler wires between switches.",
                visualHintText = "Identify the common terminal (darker screw) on each 3-way switch. Line hot goes to switch 1 common, load goes to switch 2 common. Travelers connect the brass-colored terminals.",
                category = "switching",
                relatedCodes = new[] { "404.2(A)", "404.2(B)", "200.7" }
            });

            AddCode(new NECCodeEntry
            {
                code = "200.7",
                title = "Use of White or Gray Conductors",
                section = "Article 200 -- Use and Identification of Grounded Conductors",
                simplifiedExplanation = "White or gray conductors shall only be used as grounded (neutral) conductors. Exception: in a switch loop (cable without a neutral), a white wire may be used as an ungrounded conductor if it is permanently re-identified (marked with tape or paint) at each location where it is visible and accessible.",
                visualHintText = "In a switch loop using 2-wire cable, the white wire may be used as a hot conductor but must be marked with black or red tape at both ends to show it is NOT a neutral.",
                category = "wiring",
                relatedCodes = new[] { "200.6", "404.2" }
            });

            AddCode(new NECCodeEntry
            {
                code = "210.8",
                title = "Ground-Fault Circuit-Interrupter Protection for Personnel",
                section = "Article 210 -- Branch Circuits",
                simplifiedExplanation = "GFCI protection is required for all 125V, 15A and 20A receptacles in: bathrooms, garages, outdoors, crawl spaces, unfinished basements, kitchens (serving countertops), laundry areas, and within 6 feet of sinks. GFCI outlets detect current imbalances as small as 5mA and trip to prevent electrocution.",
                visualHintText = "GFCI outlets have TEST and RESET buttons. Press TEST to trip the GFCI -- power should cut off. Press RESET to restore power. If it won't trip or reset, the GFCI is faulty and must be replaced.",
                category = "gfci",
                relatedCodes = new[] { "210.8(A)", "210.8(B)", "406.4(D)" }
            });

            AddCode(new NECCodeEntry
            {
                code = "406.4",
                title = "Receptacle Mounting and Tamper Resistance",
                section = "Article 406 -- Receptacles, Cord Connectors, and Attachment Plugs",
                simplifiedExplanation = "Receptacles must be mounted securely in their boxes. In dwelling units, all 125V 15A and 20A receptacles must be tamper-resistant (TR) type. Tamper-resistant receptacles have internal shutters that prevent insertion of foreign objects into the slots.",
                visualHintText = "Check that receptacles are firmly mounted and not loose. In residential locations, look for 'TR' marking on the outlet face indicating tamper resistance.",
                category = "outlet",
                relatedCodes = new[] { "406.4(D)", "210.52" }
            });

            AddCode(new NECCodeEntry
            {
                code = "358.24",
                title = "Bends -- Number of Bends (EMT)",
                section = "Article 358 -- Electrical Metallic Tubing (EMT)",
                simplifiedExplanation = "Between pull points (boxes, conduit bodies, fittings), there shall not be more than the equivalent of four quarter bends (360 degrees total) in a single conduit run. Excessive bends make it extremely difficult or impossible to pull wires through the conduit and can damage conductor insulation.",
                visualHintText = "Count all the bends in your conduit run. A 90-degree bend = one quarter bend. Total all bends -- they must not exceed 360 degrees between junction boxes.",
                category = "conduit",
                relatedCodes = new[] { "358.28", "358.30", "344.24" }
            });

            AddCode(new NECCodeEntry
            {
                code = "358.28",
                title = "Reaming and Deburring (EMT)",
                section = "Article 358 -- Electrical Metallic Tubing (EMT)",
                simplifiedExplanation = "All cut ends of EMT conduit must be reamed or otherwise finished to remove rough edges. Sharp edges on un-reamed conduit will damage wire insulation during pulling, leading to shorts, ground faults, or fire. Always ream both inside and outside edges after cutting.",
                visualHintText = "After cutting conduit, use a reaming tool to smooth the inside edges. Run your finger around the cut end -- if you feel any sharp burrs, it needs more reaming.",
                category = "conduit",
                relatedCodes = new[] { "358.24", "300.4" }
            });

            AddCode(new NECCodeEntry
            {
                code = "314.16",
                title = "Number of Conductors in Outlet, Device, and Junction Boxes",
                section = "Article 314 -- Outlet, Device, Pull, and Junction Boxes",
                simplifiedExplanation = "Boxes must be large enough to accommodate the number of conductors, devices, and fittings they contain. Each conductor counts as a volume allowance based on its gauge (14 AWG = 2.0 cubic inches, 12 AWG = 2.25 cubic inches). Overfilling a box creates a fire hazard from damaged insulation and poor heat dissipation.",
                visualHintText = "Count all conductors entering the box and calculate total volume. Compare to the box volume stamped on the box. Include allowances for devices, clamps, and grounds.",
                category = "wiring",
                relatedCodes = new[] { "314.16(A)", "314.16(B)", "300.14" }
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
