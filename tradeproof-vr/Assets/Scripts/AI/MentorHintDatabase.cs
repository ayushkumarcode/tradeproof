using System.Collections.Generic;
using UnityEngine;

namespace TradeProof.AI
{
    /// <summary>
    /// Static database of mentor hints indexed by (taskId, stepId, hintLevel).
    /// Contains progressive hints for all 7 task types plus encouragement and correction text.
    /// </summary>
    public static class MentorHintDatabase
    {
        // Key: (taskId, stepId, hintLevel) -> hint text
        private static Dictionary<(string taskId, string stepId, int level), string> hints;

        // Correction text: (taskId, stepId) -> correction explanation
        private static Dictionary<(string taskId, string stepId), string> corrections;

        private static readonly string[] EncouragementMessages = {
            "Great job!",
            "That's correct!",
            "Well done!",
            "Perfect!",
            "Exactly right!",
            "You nailed it!",
            "Good eye!",
            "Solid work!",
            "Right on the money!",
            "That's how a pro does it!"
        };

        static MentorHintDatabase()
        {
            hints = new Dictionary<(string, string, int), string>();
            corrections = new Dictionary<(string, string), string>();
            PopulateAllHints();
            PopulateAllCorrections();
        }

        /// <summary>
        /// Returns a hint for the given task, step, and level.
        /// Level 0: gentle nudge. Level 1: directional. Level 2: answer/explanation.
        /// </summary>
        public static string GetHint(string taskId, string stepId, int level)
        {
            if (string.IsNullOrEmpty(taskId)) return GetGenericHint(level);

            // Try exact match
            var key = (taskId, stepId ?? "", level);
            if (hints.TryGetValue(key, out string hint))
            {
                return hint;
            }

            // Try task-level default (empty stepId)
            var defaultKey = (taskId, "", level);
            if (hints.TryGetValue(defaultKey, out string defaultHint))
            {
                return defaultHint;
            }

            // Fallback to generic
            return GetGenericHint(level);
        }

        /// <summary>
        /// Returns a random encouragement message.
        /// </summary>
        public static string GetEncouragement()
        {
            return EncouragementMessages[Random.Range(0, EncouragementMessages.Length)];
        }

        /// <summary>
        /// Returns correction text explaining what the correct action was for a given task/step.
        /// </summary>
        public static string GetCorrection(string taskId, string stepId)
        {
            if (string.IsNullOrEmpty(taskId)) return null;

            var key = (taskId, stepId ?? "");
            if (corrections.TryGetValue(key, out string correction))
            {
                return correction;
            }

            // Try task-level default
            var defaultKey = (taskId, "");
            if (corrections.TryGetValue(defaultKey, out string defaultCorrection))
            {
                return defaultCorrection;
            }

            return null;
        }

        private static string GetGenericHint(int level)
        {
            switch (level)
            {
                case 0: return "Take a moment to look around. What do you notice?";
                case 1: return "Check each component carefully. Look for anything unusual.";
                case 2: return "Review the NEC code references in the task description for guidance.";
                default: return "Keep going, you're doing fine!";
            }
        }

        private static void AddHint(string taskId, string stepId, int level, string text)
        {
            hints[(taskId, stepId, level)] = text;
        }

        private static void AddCorrection(string taskId, string stepId, string text)
        {
            corrections[(taskId, stepId)] = text;
        }

        // =====================================================================
        // HINT POPULATION FOR ALL 7 TASKS
        // =====================================================================

        private static void PopulateAllHints()
        {
            PopulatePanelInspectionHints();
            PopulateCircuitWiringHints();
            PopulateOutletInstallationHints();
            PopulateSwitchWiringHints();
            PopulateGFCITestingHints();
            PopulateConduitBendingHints();
            PopulateTroubleshootingHints();
        }

        // --- 1. Panel Inspection ---
        private static void PopulatePanelInspectionHints()
        {
            string task = "panel-inspection-residential";

            // Default / general hints for task
            AddHint(task, "", 0, "What should you check first? Look at the overall panel condition.");
            AddHint(task, "", 1, "Inspect each breaker connection, the grounding, and the panel labeling.");
            AddHint(task, "", 2, "Common violations include double-tapped breakers (NEC 408.41), missing knockouts (NEC 408.7), and improper grounding (NEC 250.50).");

            // Double-tapped breaker
            AddHint(task, "check-double-tap", 0, "Look closely at each breaker terminal. Anything seem off?");
            AddHint(task, "check-double-tap", 1, "Count the wires on each breaker terminal. Most breakers are rated for one conductor only.");
            AddHint(task, "check-double-tap", 2, "That breaker has two wires on one terminal -- that's a double-tap violation per NEC 408.41.");

            // Missing knockout
            AddHint(task, "check-knockouts", 0, "Look at the panel enclosure itself. Is it properly sealed?");
            AddHint(task, "check-knockouts", 1, "Check all the knockout openings. Every unused one should have a cover.");
            AddHint(task, "check-knockouts", 2, "There's an open knockout without a filler plate. NEC 408.7 requires all openings be closed.");

            // Wire gauge
            AddHint(task, "check-wire-gauge", 0, "What about the wires themselves? Do they look appropriate?");
            AddHint(task, "check-wire-gauge", 1, "Look at the wire gauge on each circuit. Match it to the breaker amperage.");
            AddHint(task, "check-wire-gauge", 2, "That's 14 AWG on a 20A circuit -- NEC 310.16 violation. A 20A circuit requires 12 AWG minimum.");

            // Panel directory
            AddHint(task, "check-directory", 0, "Is there anything missing from the panel door?");
            AddHint(task, "check-directory", 1, "Every panel needs a circuit directory. Check if it's present and filled out.");
            AddHint(task, "check-directory", 2, "The panel directory is missing. NEC 408.4 requires each circuit to be legibly identified.");

            // Grounding
            AddHint(task, "check-grounding", 0, "Don't forget to check the grounding system.");
            AddHint(task, "check-grounding", 1, "Trace the ground wire. Is it properly connected to the grounding electrode?");
            AddHint(task, "check-grounding", 2, "The grounding electrode connection is improper. NEC 250.50 requires a complete grounding electrode system.");

            // Overfilled panel
            AddHint(task, "check-overfill", 0, "Count the breakers. Does anything seem crowded?");
            AddHint(task, "check-overfill", 1, "Compare the number of breakers to the panel's rated capacity.");
            AddHint(task, "check-overfill", 2, "This panel has more breakers than its rated spaces. NEC 408.54 violation -- panel is overfilled.");
        }

        // --- 2. Circuit Wiring ---
        private static void PopulateCircuitWiringHints()
        {
            string task = "circuit-wiring-20a";

            AddHint(task, "", 0, "Let's start by planning your circuit. What do you need first?");
            AddHint(task, "", 1, "Begin by turning off power at the breaker, then gather the right wire gauge.");
            AddHint(task, "", 2, "For a 20A circuit, you need 12 AWG wire per NEC 310.16. Start by verifying the breaker is off.");

            AddHint(task, "turn-off-breaker", 0, "Safety first! What's the first thing to do before working?");
            AddHint(task, "turn-off-breaker", 1, "Always de-energize the circuit before starting work.");
            AddHint(task, "turn-off-breaker", 2, "Flip the breaker to OFF. NEC 110.26 requires safe working conditions. Lock out/tag out if needed.");

            AddHint(task, "select-wire", 0, "Now pick the right wire. Think about the circuit amperage.");
            AddHint(task, "select-wire", 1, "A 20A circuit needs a specific wire gauge. Check NEC Table 310.16.");
            AddHint(task, "select-wire", 2, "Select 12 AWG wire (yellow sheathing for NM-B). 14 AWG is only rated for 15A per NEC 310.16.");

            AddHint(task, "strip-wire", 0, "Time to prepare the wire ends. Got your strippers ready?");
            AddHint(task, "strip-wire", 1, "Strip about 3/4 inch of insulation from each conductor end.");
            AddHint(task, "strip-wire", 2, "Use wire strippers set to 12 AWG. Strip 3/4\" of insulation. Don't nick the copper conductor.");

            AddHint(task, "connect-breaker", 0, "Now connect the wire to the breaker. Which wire goes where?");
            AddHint(task, "connect-breaker", 1, "The hot wire (black) connects to the breaker terminal. Tighten securely.");
            AddHint(task, "connect-breaker", 2, "Insert the black (hot) wire into the breaker terminal and torque to manufacturer specs. NEC 110.14 requires proper connections.");

            AddHint(task, "connect-neutral", 0, "What about the neutral wire?");
            AddHint(task, "connect-neutral", 1, "The white wire goes to the neutral bus bar.");
            AddHint(task, "connect-neutral", 2, "Connect the white (neutral) wire to the neutral bus bar. One wire per terminal per NEC 408.41.");

            AddHint(task, "connect-ground", 0, "Almost done. Don't forget the safety conductor.");
            AddHint(task, "connect-ground", 1, "The bare copper ground wire goes to the ground bus bar.");
            AddHint(task, "connect-ground", 2, "Connect the bare ground wire to the grounding bus bar per NEC 250.119. This is critical for safety.");
        }

        // --- 3. Outlet Installation ---
        private static void PopulateOutletInstallationHints()
        {
            string task = "outlet-installation-duplex";

            AddHint(task, "", 0, "Let's install this outlet. What should we do first?");
            AddHint(task, "", 1, "Verify the power is off, then identify your wires.");
            AddHint(task, "", 2, "Start by confirming the circuit is de-energized with a voltage tester. Then connect hot, neutral, and ground.");

            AddHint(task, "verify-power-off", 0, "Safety is always step one. How do we make sure it's safe?");
            AddHint(task, "verify-power-off", 1, "Use a voltage tester on the wires before touching anything.");
            AddHint(task, "verify-power-off", 2, "Test between hot and neutral, hot and ground with a non-contact voltage tester. No voltage means it's safe.");

            AddHint(task, "connect-hot", 0, "Now connect the hot wire. Which terminal does it go to?");
            AddHint(task, "connect-hot", 1, "The black (hot) wire connects to the brass-colored screw terminal.");
            AddHint(task, "connect-hot", 2, "Wrap the black wire clockwise around the brass screw and tighten. NEC 110.14 -- connections must be secure.");

            AddHint(task, "connect-neutral", 0, "What about the white wire?");
            AddHint(task, "connect-neutral", 1, "The neutral wire goes to the silver screw terminal.");
            AddHint(task, "connect-neutral", 2, "Connect the white (neutral) wire to the silver screw. Per NEC 200.11, neutral must connect to the identified terminal.");

            AddHint(task, "connect-ground", 0, "One more connection to make. Which wire is left?");
            AddHint(task, "connect-ground", 1, "The ground wire connects to the green screw on the outlet.");
            AddHint(task, "connect-ground", 2, "Attach the bare copper ground wire to the green grounding screw. NEC 250.146 requires outlet grounding.");

            AddHint(task, "mount-outlet", 0, "The outlet is wired. What's next?");
            AddHint(task, "mount-outlet", 1, "Carefully fold the wires and mount the outlet into the box.");
            AddHint(task, "mount-outlet", 2, "Push the outlet into the box, align the mounting screws, and tighten. Install the cover plate. NEC 406.6 requires faceplates.");
        }

        // --- 4. Switch Wiring (3-way) ---
        private static void PopulateSwitchWiringHints()
        {
            string task = "switch-wiring-3way";

            AddHint(task, "", 0, "Three-way switches can be tricky. Let's think through the wiring.");
            AddHint(task, "", 1, "A 3-way switch has a common terminal and two traveler terminals.");
            AddHint(task, "", 2, "The common (dark screw) gets the hot feed on one switch and the load on the other. Travelers connect between both switches.");

            AddHint(task, "identify-common", 0, "Look at the switch terminals. One is different from the others.");
            AddHint(task, "identify-common", 1, "The common terminal is usually a darker color (black or dark brass).");
            AddHint(task, "identify-common", 2, "The dark/black screw is the common terminal. On the feed switch, hot goes here. On the load switch, the light wire goes here.");

            AddHint(task, "connect-travelers", 0, "Now for the traveler wires. Where do they go?");
            AddHint(task, "connect-travelers", 1, "The two traveler wires connect between matching terminals on both switches.");
            AddHint(task, "connect-travelers", 2, "Connect the red and white (re-identified as hot with tape) traveler wires to the brass traveler screws on each switch. NEC 200.7 allows re-identification.");

            AddHint(task, "connect-common-feed", 0, "Which wire feeds power to the first switch?");
            AddHint(task, "connect-common-feed", 1, "The incoming hot (black) wire goes to the common terminal on the first switch.");
            AddHint(task, "connect-common-feed", 2, "Connect the black hot wire from the panel to the common (dark) screw on switch 1. This is the power feed.");

            AddHint(task, "connect-common-load", 0, "And on the other switch?");
            AddHint(task, "connect-common-load", 1, "The wire going to the light fixture connects to the common terminal on switch 2.");
            AddHint(task, "connect-common-load", 2, "The black wire to the light fixture connects to the common (dark) screw on switch 2. This completes the switched leg.");

            AddHint(task, "connect-grounds", 0, "What connections are still needed?");
            AddHint(task, "connect-grounds", 1, "Ground wires must connect to both switches and be bonded together.");
            AddHint(task, "connect-grounds", 2, "Pigtail the ground wires and connect to the green screw on each switch. NEC 404.9 requires switch grounding.");
        }

        // --- 5. GFCI Testing ---
        private static void PopulateGFCITestingHints()
        {
            string task = "gfci-testing-residential";

            AddHint(task, "", 0, "Time to test this GFCI outlet. Know how it works?");
            AddHint(task, "", 1, "A GFCI detects ground faults and trips to protect against shock. Test and reset buttons are on the face.");
            AddHint(task, "", 2, "Press TEST -- the outlet should lose power. Press RESET to restore. Also test with a GFCI tester to verify correct wiring. NEC 210.8 requires GFCI protection.");

            AddHint(task, "press-test", 0, "Start with the built-in test. What button should you press?");
            AddHint(task, "press-test", 1, "Press the TEST button on the GFCI outlet face.");
            AddHint(task, "press-test", 2, "Press TEST. The button should click and power should drop. If it doesn't trip, the GFCI is faulty and must be replaced per NEC 210.8.");

            AddHint(task, "verify-tripped", 0, "The button clicked. How do you verify it actually tripped?");
            AddHint(task, "verify-tripped", 1, "Use a voltage tester or plug in a lamp to verify power is off.");
            AddHint(task, "verify-tripped", 2, "Test the outlet with a voltage tester. It should read 0V. Also check downstream outlets -- they should also be off if daisy-chained.");

            AddHint(task, "press-reset", 0, "Good. Now restore power.");
            AddHint(task, "press-reset", 1, "Press the RESET button to restore the circuit.");
            AddHint(task, "press-reset", 2, "Press RESET firmly. Power should return. If it won't reset, check wiring: LINE and LOAD may be reversed.");

            AddHint(task, "tester-check", 0, "Let's do a more thorough test.");
            AddHint(task, "tester-check", 1, "Use a GFCI plug-in tester to check wiring and trip function.");
            AddHint(task, "tester-check", 2, "Plug in the GFCI tester. Three green lights = correct wiring. Press the tester's trip button to verify external trip capability.");

            AddHint(task, "check-downstream", 0, "Are there other outlets on this circuit?");
            AddHint(task, "check-downstream", 1, "GFCI protection extends to downstream outlets on the LOAD side.");
            AddHint(task, "check-downstream", 2, "Test each downstream outlet. They should also be protected. NEC 406.4(D)(2) requires marking downstream outlets as 'GFCI Protected'.");
        }

        // --- 6. Conduit Bending ---
        private static void PopulateConduitBendingHints()
        {
            string task = "conduit-bending-emt";

            AddHint(task, "", 0, "Conduit bending requires good measurements. Got your tape measure?");
            AddHint(task, "", 1, "Measure carefully, mark your bend points, and use the right bender for the conduit size.");
            AddHint(task, "", 2, "For 3/4\" EMT, use the correct bender shoe. Deduct amount for a 90 is 6\". NEC 358.24 limits total bends to 360 degrees between pull points.");

            AddHint(task, "measure-stub", 0, "First, let's figure out our stub-up height.");
            AddHint(task, "measure-stub", 1, "Measure from the floor to the connection point. Subtract the deduct for your bend.");
            AddHint(task, "measure-stub", 2, "Stub-up height minus deduct = mark on conduit. For 3/4\" EMT, deduct is 6\". Mark that distance from the end.");

            AddHint(task, "make-90", 0, "Time to make the bend. Position the bender correctly.");
            AddHint(task, "make-90", 1, "Align the arrow on the bender with your mark. Apply steady foot pressure.");
            AddHint(task, "make-90", 2, "Place the conduit in the bender, arrow at your mark. Step on the foot pedal and pull the handle until the bubble level reads 90 degrees.");

            AddHint(task, "make-offset", 0, "Now for the offset bend. What angle do you need?");
            AddHint(task, "make-offset", 1, "A standard offset uses two bends at 30 degrees. Calculate the distance between bends.");
            AddHint(task, "make-offset", 2, "For a 30-degree offset: multiply the offset depth by 2 to get the distance between bends. Bend first angle, slide conduit, bend the reverse angle.");

            AddHint(task, "check-fit", 0, "Does your conduit run fit properly?");
            AddHint(task, "check-fit", 1, "Hold the conduit up to the wall and check alignment with the boxes.");
            AddHint(task, "check-fit", 2, "The conduit should enter each box straight and be supported within 3 feet of each box per NEC 358.30. Total bends must not exceed 360 degrees.");
        }

        // --- 7. Troubleshooting ---
        private static void PopulateTroubleshootingHints()
        {
            string task = "troubleshooting-residential";

            AddHint(task, "", 0, "Start with the customer. What questions should you ask?");
            AddHint(task, "", 1, "Ask when the problem started, what was happening, and whether anything changed recently.");
            AddHint(task, "", 2, "Systematic troubleshooting: 1) Interview customer, 2) Check panel for tripped breakers, 3) Test outlets with multimeter, 4) Isolate the fault.");

            AddHint(task, "interview-customer", 0, "Talk to the customer first. What would you ask?");
            AddHint(task, "interview-customer", 1, "Ask about the symptoms, when it started, and any recent changes (new appliances, storms, etc.).");
            AddHint(task, "interview-customer", 2, "Key questions: 'When did it start?', 'Which outlets are affected?', 'Did you add any new loads?', 'Was there a storm recently?' Each answer narrows the diagnosis.");

            AddHint(task, "check-panel", 0, "Next step: go to the source. Where should you look?");
            AddHint(task, "check-panel", 1, "Check the electrical panel. Look for tripped breakers.");
            AddHint(task, "check-panel", 2, "Open the panel and look for breakers in the middle (tripped) position. A tripped GFCI breaker may indicate a ground fault. Reset and observe.");

            AddHint(task, "test-outlets", 0, "Use your tools to gather more data.");
            AddHint(task, "test-outlets", 1, "Test voltage at the affected outlets using your multimeter.");
            AddHint(task, "test-outlets", 2, "Set multimeter to AC voltage. Test hot-neutral (should read ~120V), hot-ground, and neutral-ground. Low or no voltage indicates the fault location.");

            AddHint(task, "identify-fault", 0, "Based on your tests, what's the problem?");
            AddHint(task, "identify-fault", 1, "Look at the pattern. Which circuit is affected? Is it all outlets or just some?");
            AddHint(task, "identify-fault", 2, "If voltage is present at the panel but not at the outlet, the fault is in the wire run. Check for loose connections, damaged wire, or a bad splice in a junction box.");

            AddHint(task, "repair-fault", 0, "You found the problem. How will you fix it?");
            AddHint(task, "repair-fault", 1, "De-energize the circuit first, then address the specific fault you identified.");
            AddHint(task, "repair-fault", 2, "Turn off the breaker. If it's a loose connection, strip fresh wire and re-terminate. If it's a bad splice, use proper wire nuts per NEC 110.14. Test after repair.");
        }

        // =====================================================================
        // CORRECTION POPULATION
        // =====================================================================

        private static void PopulateAllCorrections()
        {
            // Panel Inspection
            AddCorrection("panel-inspection-residential", "check-double-tap",
                "Each breaker terminal should have only one conductor unless the breaker is rated for multiple wires (NEC 408.41).");
            AddCorrection("panel-inspection-residential", "check-knockouts",
                "All unused knockout openings must be sealed with appropriate filler plates (NEC 408.7).");
            AddCorrection("panel-inspection-residential", "check-wire-gauge",
                "Wire gauge must match breaker amperage: 14 AWG for 15A, 12 AWG for 20A, 10 AWG for 30A (NEC 310.16).");
            AddCorrection("panel-inspection-residential", "check-directory",
                "A legible circuit directory must be present identifying each circuit (NEC 408.4).");
            AddCorrection("panel-inspection-residential", "check-grounding",
                "The grounding electrode system must be complete and properly connected (NEC 250.50).");
            AddCorrection("panel-inspection-residential", "",
                "Review all components: breakers, wiring, grounding, enclosure, and labeling for NEC compliance.");

            // Circuit Wiring
            AddCorrection("circuit-wiring-20a", "select-wire",
                "A 20A circuit requires 12 AWG wire minimum per NEC Table 310.16.");
            AddCorrection("circuit-wiring-20a", "connect-breaker",
                "The hot (black) wire must be securely connected to the breaker terminal with proper torque (NEC 110.14).");
            AddCorrection("circuit-wiring-20a", "connect-neutral",
                "The neutral (white) wire goes to the neutral bus bar, one wire per terminal (NEC 408.41).");
            AddCorrection("circuit-wiring-20a", "connect-ground",
                "The ground wire connects to the grounding bus bar (NEC 250.119).");
            AddCorrection("circuit-wiring-20a", "",
                "For a 20A circuit: 12 AWG wire, proper breaker connection, neutral to bus bar, ground to grounding bar.");

            // Outlet Installation
            AddCorrection("outlet-installation-duplex", "connect-hot",
                "The hot (black) wire connects to the brass screw terminal on the outlet.");
            AddCorrection("outlet-installation-duplex", "connect-neutral",
                "The neutral (white) wire connects to the silver screw terminal (NEC 200.11).");
            AddCorrection("outlet-installation-duplex", "connect-ground",
                "The ground wire connects to the green grounding screw (NEC 250.146).");
            AddCorrection("outlet-installation-duplex", "",
                "Hot to brass, neutral to silver, ground to green. Verify with a voltage tester before and after.");

            // Switch Wiring
            AddCorrection("switch-wiring-3way", "identify-common",
                "The common terminal (dark/black screw) is distinct from the two brass traveler terminals.");
            AddCorrection("switch-wiring-3way", "connect-travelers",
                "Traveler wires connect the same-colored screws on both 3-way switches.");
            AddCorrection("switch-wiring-3way", "connect-common-feed",
                "The incoming hot wire must connect to the common terminal on the first switch.");
            AddCorrection("switch-wiring-3way", "connect-common-load",
                "The switch leg to the fixture connects to the common terminal on the second switch.");
            AddCorrection("switch-wiring-3way", "",
                "3-way wiring: hot to common on switch 1, load to common on switch 2, travelers between both switches.");

            // GFCI Testing
            AddCorrection("gfci-testing-residential", "press-test",
                "Pressing TEST should trip the GFCI and cut power. If it doesn't, the GFCI is faulty.");
            AddCorrection("gfci-testing-residential", "press-reset",
                "RESET restores power. If it won't reset, LINE and LOAD terminals may be reversed.");
            AddCorrection("gfci-testing-residential", "tester-check",
                "A plug-in GFCI tester verifies correct wiring and external trip function.");
            AddCorrection("gfci-testing-residential", "",
                "GFCI outlets must trip on TEST, reset on RESET, and pass a plug-in tester check (NEC 210.8).");

            // Conduit Bending
            AddCorrection("conduit-bending-emt", "measure-stub",
                "Stub-up mark = desired height minus the deduct for your conduit size (6\" for 3/4\" EMT).");
            AddCorrection("conduit-bending-emt", "make-90",
                "Align the bender arrow at the mark and bend until the level indicates 90 degrees.");
            AddCorrection("conduit-bending-emt", "make-offset",
                "For 30-degree offsets, the distance between bends = offset depth x 2.");
            AddCorrection("conduit-bending-emt", "",
                "Measure twice, bend once. Total bends between pull points cannot exceed 360 degrees (NEC 358.24).");

            // Troubleshooting
            AddCorrection("troubleshooting-residential", "interview-customer",
                "Good diagnostic questions help narrow down the fault before opening any boxes.");
            AddCorrection("troubleshooting-residential", "check-panel",
                "Check for tripped breakers (middle position) and listen for buzzing indicating loose connections.");
            AddCorrection("troubleshooting-residential", "test-outlets",
                "Use a multimeter to test voltage at each outlet: hot-neutral, hot-ground, neutral-ground.");
            AddCorrection("troubleshooting-residential", "identify-fault",
                "Follow the circuit from panel to outlet, testing at each junction until voltage drops.");
            AddCorrection("troubleshooting-residential", "repair-fault",
                "De-energize, repair the connection with proper techniques, and verify the fix with your multimeter.");
            AddCorrection("troubleshooting-residential", "",
                "Systematic approach: interview, inspect panel, test outlets, isolate fault, repair, verify.");
        }
    }
}
