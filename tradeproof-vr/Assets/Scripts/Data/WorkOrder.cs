using UnityEngine;
using System;
using System.Collections.Generic;

namespace TradeProof.Data
{
    public enum WorkOrderPriority
    {
        Low,
        Normal,
        Urgent
    }

    [Serializable]
    public class WorkOrder
    {
        public string orderId;
        public string customerName;
        public string address;
        public string description;
        public string taskId;
        public string jobSiteType;
        public WorkOrderPriority priority;
        public int xpReward;
        public float bonusMultiplier;
        public string[] requiredTools;
    }

    public static class WorkOrderGenerator
    {
        private static readonly string[] FirstNames = {
            "Sarah", "Mike", "Jennifer", "David", "Lisa", "Robert", "Emily", "James",
            "Maria", "Thomas", "Karen", "Brian", "Amanda", "Kevin", "Patricia"
        };

        private static readonly string[] LastNames = {
            "Johnson", "Williams", "Brown", "Garcia", "Miller", "Davis", "Rodriguez",
            "Martinez", "Anderson", "Taylor", "Thomas", "Moore", "Jackson", "White"
        };

        private static readonly string[] Streets = {
            "Oak St", "Maple Ave", "Cedar Ln", "Pine Dr", "Elm Blvd",
            "Birch Rd", "Walnut Ct", "Cherry Way", "Hickory Pl", "Spruce Ter"
        };

        private static readonly WorkOrderTemplate[] ApprenticeTemplates = {
            new WorkOrderTemplate {
                taskId = "panel-inspection-residential",
                descriptions = new[] {
                    "Home buyer wants a panel inspection before closing",
                    "Annual safety inspection requested for insurance",
                    "Homeowner noticed flickering lights, wants panel checked"
                },
                jobSiteType = "residential-garage",
                xpReward = 50
            },
            new WorkOrderTemplate {
                taskId = "circuit-wiring-20a",
                descriptions = new[] {
                    "New kitchen needs a dedicated 20A circuit for dishwasher",
                    "Adding a 20A outlet in the garage workshop",
                    "Wiring a new 20A circuit for bathroom GFCI outlets"
                },
                jobSiteType = "residential-kitchen",
                xpReward = 60
            },
            new WorkOrderTemplate {
                taskId = "outlet-installation-duplex",
                descriptions = new[] {
                    "Replace old outlet in the living room",
                    "Install a new outlet behind the TV wall mount",
                    "Add an outlet in the home office"
                },
                jobSiteType = "residential-kitchen",
                xpReward = 40
            }
        };

        private static readonly WorkOrderTemplate[] JourneymanTemplates = {
            new WorkOrderTemplate {
                taskId = "switch-wiring-3way",
                descriptions = new[] {
                    "Install 3-way switches for the hallway lights",
                    "Replace single switch with 3-way setup for stairwell",
                    "Wire 3-way switches for the living room ceiling fan"
                },
                jobSiteType = "residential-kitchen",
                xpReward = 80
            },
            new WorkOrderTemplate {
                taskId = "gfci-testing-residential",
                descriptions = new[] {
                    "GFCI outlets in bathroom stopped working",
                    "Kitchen outlet won't reset after power surge",
                    "Test all GFCI outlets per rental inspection requirements"
                },
                jobSiteType = "residential-bathroom",
                xpReward = 70
            },
            new WorkOrderTemplate {
                taskId = "conduit-bending-emt",
                descriptions = new[] {
                    "Run EMT conduit for new garage subpanel feed",
                    "Install conduit run for outdoor lighting circuit",
                    "Route conduit through basement for workshop power"
                },
                jobSiteType = "residential-garage",
                xpReward = 85
            }
        };

        private static readonly WorkOrderTemplate[] MasterTemplates = {
            new WorkOrderTemplate {
                taskId = "troubleshooting-residential",
                descriptions = new[] {
                    "Half the outlets in the house went dead overnight",
                    "Customer hears buzzing from the panel, intermittent outages",
                    "Home office circuit keeps tripping -- customer says nothing changed"
                },
                jobSiteType = "residential-kitchen",
                xpReward = 120
            }
        };

        public static List<WorkOrder> Generate(int count, Core.CareerLevel level)
        {
            List<WorkOrder> orders = new List<WorkOrder>();
            List<WorkOrderTemplate> availableTemplates = new List<WorkOrderTemplate>();

            // Add templates based on career level
            availableTemplates.AddRange(ApprenticeTemplates);
            if (level >= Core.CareerLevel.Journeyman)
                availableTemplates.AddRange(JourneymanTemplates);
            if (level >= Core.CareerLevel.Master)
                availableTemplates.AddRange(MasterTemplates);

            for (int i = 0; i < count; i++)
            {
                var template = availableTemplates[UnityEngine.Random.Range(0, availableTemplates.Count)];
                var order = new WorkOrder();

                order.orderId = $"WO-{DateTime.Now:yyyyMMdd}-{i + 1:D3}";
                order.customerName = $"{FirstNames[UnityEngine.Random.Range(0, FirstNames.Length)]} {LastNames[UnityEngine.Random.Range(0, LastNames.Length)]}";
                order.address = $"{UnityEngine.Random.Range(100, 9999)} {Streets[UnityEngine.Random.Range(0, Streets.Length)]}";
                order.description = template.descriptions[UnityEngine.Random.Range(0, template.descriptions.Length)];
                order.taskId = template.taskId;
                order.jobSiteType = template.jobSiteType;
                order.xpReward = template.xpReward;

                // Assign priority (urgent jobs get bonus multiplier)
                float priorityRoll = UnityEngine.Random.value;
                if (priorityRoll < 0.15f)
                {
                    order.priority = WorkOrderPriority.Urgent;
                    order.bonusMultiplier = 1.5f;
                }
                else if (priorityRoll < 0.5f)
                {
                    order.priority = WorkOrderPriority.Normal;
                    order.bonusMultiplier = 1.0f;
                }
                else
                {
                    order.priority = WorkOrderPriority.Low;
                    order.bonusMultiplier = 0.8f;
                }

                orders.Add(order);
            }

            return orders;
        }

        private class WorkOrderTemplate
        {
            public string taskId;
            public string[] descriptions;
            public string jobSiteType;
            public int xpReward;
        }
    }
}
