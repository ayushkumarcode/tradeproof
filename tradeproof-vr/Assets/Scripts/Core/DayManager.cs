using UnityEngine;
using System;
using System.Collections.Generic;
using TradeProof.Data;

namespace TradeProof.Core
{
    /// <summary>
    /// Day phase enumeration for the day-in-the-life game flow.
    /// </summary>
    public enum DayPhase
    {
        MorningBriefing,
        OnSite,
        BetweenJobs,
        EndOfDay
    }

    /// <summary>
    /// Manages the full day-in-the-life game loop.
    /// Generates work orders, tracks completions, and produces daily summaries.
    /// </summary>
    public class DayManager : MonoBehaviour
    {
        private static DayManager _instance;
        public static DayManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DayManager");
                    _instance = go.AddComponent<DayManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Day State")]
        [SerializeField] private int currentDay;
        [SerializeField] private DayPhase currentPhase = DayPhase.MorningBriefing;

        [Header("Work Orders")]
        private List<WorkOrder> todaysWorkOrders = new List<WorkOrder>();
        private List<WorkOrder> completedOrders = new List<WorkOrder>();
        private WorkOrder activeOrder;

        // Daily tracking
        private int dailyXPEarned;
        private float bestScoreToday;
        private int jobsCompletedToday;
        private float fastestTimeToday = float.MaxValue;
        private HashSet<string> uniqueTaskTypesToday = new HashSet<string>();
        private bool usedHintsToday;

        // Events
        public event Action<DayPhase> OnDayPhaseChanged;

        // Properties
        public int CurrentDay => currentDay;
        public DayPhase CurrentPhase => currentPhase;
        public List<WorkOrder> TodaysWorkOrders => todaysWorkOrders;
        public List<WorkOrder> CompletedOrders => completedOrders;
        public WorkOrder ActiveOrder => activeOrder;
        public int DailyXPEarned => dailyXPEarned;
        public int JobsCompletedToday => jobsCompletedToday;
        public float BestScoreToday => bestScoreToday;
        public bool UsedHintsToday => usedHintsToday;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadDayState();
        }

        /// <summary>
        /// Starts a new day. Increments the day counter, generates work orders,
        /// and transitions to the MorningBriefing phase.
        /// </summary>
        public void StartNewDay()
        {
            currentDay++;
            dailyXPEarned = 0;
            bestScoreToday = 0f;
            jobsCompletedToday = 0;
            fastestTimeToday = float.MaxValue;
            uniqueTaskTypesToday.Clear();
            usedHintsToday = false;

            completedOrders.Clear();
            activeOrder = null;

            // Determine order count based on career level
            CareerLevel level = GameManager.Instance.CurrentCareerLevel;
            int orderCount;
            switch (level)
            {
                case CareerLevel.Apprentice:
                    orderCount = 3;
                    break;
                case CareerLevel.Journeyman:
                    orderCount = 4;
                    break;
                case CareerLevel.Master:
                    orderCount = 5;
                    break;
                default:
                    orderCount = 3;
                    break;
            }

            todaysWorkOrders = WorkOrderGenerator.Generate(orderCount, level);

            TransitionToPhase(DayPhase.MorningBriefing);

            Debug.Log($"[DayManager] Day {currentDay} started with {todaysWorkOrders.Count} work orders. Career level: {level}");
        }

        /// <summary>
        /// Accepts a work order, sets it as the active order, and tells GameManager to start the task.
        /// </summary>
        public void AcceptWorkOrder(WorkOrder order)
        {
            if (order == null)
            {
                Debug.LogError("[DayManager] Cannot accept null work order.");
                return;
            }

            activeOrder = order;
            GameManager.Instance.CurrentWorkOrder = order;

            TransitionToPhase(DayPhase.OnSite);

            // Start the task via GameManager
            GameManager.Instance.StartWorkOrder(order, Training.TaskMode.Practice);

            Debug.Log($"[DayManager] Accepted work order: {order.orderId} ({order.taskId}) for {order.customerName}");
        }

        /// <summary>
        /// Records a completed work order. Adds XP, updates daily stats,
        /// and checks if more orders remain.
        /// </summary>
        public void CompleteWorkOrder(TaskResult result)
        {
            if (activeOrder == null)
            {
                Debug.LogWarning("[DayManager] CompleteWorkOrder called but no active order.");
                return;
            }

            // Record completion
            completedOrders.Add(activeOrder);
            todaysWorkOrders.Remove(activeOrder);

            // Update daily stats
            jobsCompletedToday++;
            if (result.score > bestScoreToday)
            {
                bestScoreToday = result.score;
            }

            // Calculate and award XP
            int xp = activeOrder.xpReward;
            if (result.passed)
            {
                xp = Mathf.RoundToInt(xp * activeOrder.bonusMultiplier);
            }
            else
            {
                xp = Mathf.RoundToInt(xp * 0.5f); // Half XP for failed jobs
            }
            dailyXPEarned += xp;

            // Track time
            if (result.breakdown != null && result.breakdown.timeUsed < fastestTimeToday)
            {
                fastestTimeToday = result.breakdown.timeUsed;
            }

            // Track unique task types
            if (!string.IsNullOrEmpty(activeOrder.taskId))
            {
                uniqueTaskTypesToday.Add(activeOrder.taskId);
            }

            WorkOrder justCompleted = activeOrder;
            activeOrder = null;

            Debug.Log($"[DayManager] Work order completed: {justCompleted.orderId} | Score: {result.score:F1}% | XP: {xp} | Jobs remaining: {todaysWorkOrders.Count}");

            // Notify daily challenge system
            DailyChallengeSystem challengeSystem = FindObjectOfType<DailyChallengeSystem>();
            if (challengeSystem != null)
            {
                challengeSystem.TrackProgress(result);
            }

            // Check if more orders remain
            if (todaysWorkOrders.Count > 0)
            {
                TransitionToPhase(DayPhase.BetweenJobs);
            }
            else
            {
                EndDay();
            }
        }

        /// <summary>
        /// Ends the current day. Calculates the daily summary, saves progress, and transitions to EndOfDay.
        /// </summary>
        public void EndDay()
        {
            TransitionToPhase(DayPhase.EndOfDay);

            // Update persistent progress
            PlayerProgress progress = GameManager.Instance.Progress;
            if (progress != null)
            {
                progress.daysCompleted++;
                progress.AddXP(dailyXPEarned);
            }

            GameManager.Instance.SavePlayerProgress();
            SaveDayState();

            Debug.Log($"[DayManager] Day {currentDay} ended. Summary:\n{GetDaySummary()}");

            // Transition GameManager to DayResults
            GameManager.Instance.TransitionToState(GameState.DayResults);
        }

        /// <summary>
        /// Returns a formatted string summarizing today's performance.
        /// </summary>
        public string GetDaySummary()
        {
            string summary = $"=== Day {currentDay} Summary ===\n";
            summary += $"Jobs Completed: {jobsCompletedToday}\n";
            summary += $"Total XP Earned: {dailyXPEarned}\n";
            summary += $"Best Score: {bestScoreToday:F1}%\n";

            if (fastestTimeToday < float.MaxValue)
            {
                int minutes = Mathf.FloorToInt(fastestTimeToday / 60f);
                int seconds = Mathf.FloorToInt(fastestTimeToday % 60f);
                summary += $"Fastest Job: {minutes}:{seconds:D2}\n";
            }

            summary += $"Unique Task Types: {uniqueTaskTypesToday.Count}\n";
            summary += $"Used Hints: {(usedHintsToday ? "Yes" : "No")}\n";

            // Completed order details
            if (completedOrders.Count > 0)
            {
                summary += "\nCompleted Jobs:\n";
                foreach (var order in completedOrders)
                {
                    summary += $"  - {order.customerName} ({order.taskId})\n";
                }
            }

            return summary;
        }

        /// <summary>
        /// Marks that the player used hints today.
        /// </summary>
        public void MarkHintUsed()
        {
            usedHintsToday = true;
        }

        /// <summary>
        /// Returns the number of unique task types completed today.
        /// </summary>
        public int GetUniqueTaskTypeCount()
        {
            return uniqueTaskTypesToday.Count;
        }

        /// <summary>
        /// Returns the fastest task completion time today in seconds.
        /// </summary>
        public float GetFastestTime()
        {
            return fastestTimeToday < float.MaxValue ? fastestTimeToday : 0f;
        }

        private void TransitionToPhase(DayPhase newPhase)
        {
            DayPhase previous = currentPhase;
            currentPhase = newPhase;

            Debug.Log($"[DayManager] Phase transition: {previous} -> {newPhase}");

            OnDayPhaseChanged?.Invoke(newPhase);
        }

        // --- Persistence ---

        private void SaveDayState()
        {
            PlayerPrefs.SetInt("TradeProof_CurrentDay", currentDay);
            PlayerPrefs.Save();
        }

        private void LoadDayState()
        {
            currentDay = PlayerPrefs.GetInt("TradeProof_CurrentDay", 0);
        }
    }
}
