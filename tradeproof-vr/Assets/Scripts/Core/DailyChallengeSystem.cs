using UnityEngine;
using System;
using TradeProof.Data;

namespace TradeProof.Core
{
    /// <summary>
    /// Generates one daily challenge per day using the date as a seed.
    /// Tracks progress toward the daily goal and awards bonus XP on completion.
    /// </summary>
    public class DailyChallengeSystem : MonoBehaviour
    {
        private static DailyChallengeSystem _instance;
        public static DailyChallengeSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DailyChallengeSystem");
                    _instance = go.AddComponent<DailyChallengeSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Represents a single daily challenge with progress tracking.
        /// </summary>
        [Serializable]
        public class DailyChallenge
        {
            public string challengeId;
            public string description;
            public int targetValue;
            public int currentProgress;
            public int xpBonus;
            public bool isComplete;
            public int challengeType; // 0-4 corresponding to challenge categories
            public string dateGenerated;
        }

        private DailyChallenge cachedChallenge;
        private string cachedDate;

        // Tracking fields for the current day
        private int tasksWithoutHints;
        private float minScoreToday = 100f;
        private bool hasCompletedTaskToday;
        private float fastestTimeToday = float.MaxValue;
        private int dailyXPAccumulated;
        private int uniqueTaskTypesCompleted;
        private System.Collections.Generic.HashSet<string> completedTaskTypes = new System.Collections.Generic.HashSet<string>();

        // Events
        public event Action<DailyChallenge> OnChallengeCompleted;
        public event Action<DailyChallenge> OnChallengeProgressUpdated;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Generates or returns the cached daily challenge for today.
        /// Uses dayOfYear as the seed so the same challenge appears all day.
        /// </summary>
        public DailyChallenge GetTodaysChallenge()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Return cached if still the same day
            if (cachedChallenge != null && cachedDate == today)
            {
                return cachedChallenge;
            }

            // Check if we saved today's challenge already
            string savedJson = PlayerPrefs.GetString("TradeProof_DailyChallenge", "");
            if (!string.IsNullOrEmpty(savedJson))
            {
                DailyChallenge saved = JsonUtility.FromJson<DailyChallenge>(savedJson);
                if (saved != null && saved.dateGenerated == today)
                {
                    cachedChallenge = saved;
                    cachedDate = today;
                    return cachedChallenge;
                }
            }

            // Generate new challenge
            cachedChallenge = GenerateChallenge(today);
            cachedDate = today;

            // Reset daily tracking
            ResetDailyTracking();

            SaveChallenge();

            Debug.Log($"[DailyChallenge] Generated: {cachedChallenge.description} (target: {cachedChallenge.targetValue}, bonus: {cachedChallenge.xpBonus} XP)");

            return cachedChallenge;
        }

        /// <summary>
        /// Tracks progress based on a completed task result.
        /// </summary>
        public void TrackProgress(TaskResult result)
        {
            DailyChallenge challenge = GetTodaysChallenge();
            if (challenge.isComplete) return;

            // Update tracking based on result
            hasCompletedTaskToday = true;

            // Track min score
            if (result.score < minScoreToday)
            {
                minScoreToday = result.score;
            }

            // Track task types
            if (!string.IsNullOrEmpty(result.taskId))
            {
                completedTaskTypes.Add(result.taskId);
                uniqueTaskTypesCompleted = completedTaskTypes.Count;
            }

            // Track fastest time
            if (result.breakdown != null && result.breakdown.timeUsed > 0f && result.breakdown.timeUsed < fastestTimeToday)
            {
                fastestTimeToday = result.breakdown.timeUsed;
            }

            // Track XP
            dailyXPAccumulated += result.xpEarned;

            // Update challenge progress based on type
            switch (challenge.challengeType)
            {
                case 0: // Complete N tasks without hints
                    UpdateNoHintsChallenge(challenge, result);
                    break;
                case 1: // Score 90%+ on all tasks
                    UpdateHighScoreChallenge(challenge, result);
                    break;
                case 2: // Complete a task in under 2 minutes
                    UpdateSpeedChallenge(challenge, result);
                    break;
                case 3: // Earn N XP in one day
                    UpdateXPChallenge(challenge, result);
                    break;
                case 4: // Complete N different task types
                    UpdateVarietyChallenge(challenge, result);
                    break;
            }

            OnChallengeProgressUpdated?.Invoke(challenge);
            SaveChallenge();
        }

        /// <summary>
        /// Notifies the system that the player used a hint.
        /// </summary>
        public void OnHintUsed()
        {
            // Reset the no-hints counter
            tasksWithoutHints = 0;

            DailyChallenge challenge = GetTodaysChallenge();
            if (challenge.challengeType == 0)
            {
                // The no-hints challenge progress resets when a hint is used
                challenge.currentProgress = 0;
                SaveChallenge();
            }
        }

        /// <summary>
        /// Returns whether today's challenge is complete.
        /// </summary>
        public bool IsComplete()
        {
            DailyChallenge challenge = GetTodaysChallenge();
            return challenge.isComplete;
        }

        /// <summary>
        /// Returns human-readable progress text like "2/3 tasks completed".
        /// </summary>
        public string GetProgressText()
        {
            DailyChallenge challenge = GetTodaysChallenge();

            if (challenge.isComplete)
            {
                return "COMPLETE! +" + challenge.xpBonus + " XP";
            }

            switch (challenge.challengeType)
            {
                case 0:
                    return $"{challenge.currentProgress}/{challenge.targetValue} tasks without hints";
                case 1:
                    if (!hasCompletedTaskToday)
                        return "Complete tasks with 90%+ to progress";
                    return minScoreToday >= 90f ? $"All scores 90%+ ({challenge.currentProgress} tasks)" : $"Lowest score: {minScoreToday:F0}% (need 90%+)";
                case 2:
                    if (fastestTimeToday >= float.MaxValue)
                        return "Complete a task in under 2:00";
                    float best = fastestTimeToday;
                    int m = Mathf.FloorToInt(best / 60f);
                    int s = Mathf.FloorToInt(best % 60f);
                    return $"Best time: {m}:{s:D2} (need under 2:00)";
                case 3:
                    return $"{challenge.currentProgress}/{challenge.targetValue} XP earned";
                case 4:
                    return $"{challenge.currentProgress}/{challenge.targetValue} different task types";
                default:
                    return $"{challenge.currentProgress}/{challenge.targetValue}";
            }
        }

        // --- Challenge Generation ---

        private DailyChallenge GenerateChallenge(string dateString)
        {
            // Use day of year as seed for deterministic daily challenge
            int dayOfYear = DateTime.Now.DayOfYear;
            int year = DateTime.Now.Year;
            int seed = dayOfYear + year * 366;

            // Deterministic random based on date
            System.Random rng = new System.Random(seed);
            int challengeType = dayOfYear % 5;

            DailyChallenge challenge = new DailyChallenge();
            challenge.dateGenerated = dateString;
            challenge.challengeType = challengeType;
            challenge.currentProgress = 0;
            challenge.isComplete = false;

            switch (challengeType)
            {
                case 0: // Complete N tasks without hints
                {
                    int count = 2 + (rng.Next(0, 3)); // 2-4
                    challenge.challengeId = $"no-hints-{count}-{dateString}";
                    challenge.description = $"Complete {count} tasks without using any hints";
                    challenge.targetValue = count;
                    challenge.xpBonus = 50 + count * 15;
                    break;
                }

                case 1: // Score 90%+ on all tasks
                {
                    challenge.challengeId = $"high-score-{dateString}";
                    challenge.description = "Score 90% or higher on every task today";
                    challenge.targetValue = 1; // Binary: achieved or not
                    challenge.xpBonus = 100;
                    break;
                }

                case 2: // Complete a task in under 2 minutes
                {
                    challenge.challengeId = $"speed-run-{dateString}";
                    challenge.description = "Complete any task in under 2 minutes";
                    challenge.targetValue = 120; // 120 seconds
                    challenge.xpBonus = 75;
                    break;
                }

                case 3: // Earn N XP in one day
                {
                    int xpTarget = 100 + (rng.Next(0, 5)) * 25; // 100-200 in increments of 25
                    challenge.challengeId = $"xp-grind-{xpTarget}-{dateString}";
                    challenge.description = $"Earn {xpTarget} XP in one day";
                    challenge.targetValue = xpTarget;
                    challenge.xpBonus = 60;
                    break;
                }

                case 4: // Complete N different task types
                {
                    int typeCount = 2 + (rng.Next(0, 2)); // 2-3
                    challenge.challengeId = $"variety-{typeCount}-{dateString}";
                    challenge.description = $"Complete {typeCount} different task types today";
                    challenge.targetValue = typeCount;
                    challenge.xpBonus = 55 + typeCount * 20;
                    break;
                }
            }

            return challenge;
        }

        // --- Progress Update Methods ---

        private void UpdateNoHintsChallenge(DailyChallenge challenge, TaskResult result)
        {
            // Only count if no hints were used this session
            DayManager dayManager = FindObjectOfType<DayManager>();
            bool hintsUsed = dayManager != null && dayManager.UsedHintsToday;

            if (!hintsUsed)
            {
                tasksWithoutHints++;
                challenge.currentProgress = tasksWithoutHints;
            }
            else
            {
                challenge.currentProgress = 0;
                tasksWithoutHints = 0;
            }

            CheckCompletion(challenge);
        }

        private void UpdateHighScoreChallenge(DailyChallenge challenge, TaskResult result)
        {
            // Track how many tasks have been completed with 90%+
            if (minScoreToday >= 90f && hasCompletedTaskToday)
            {
                challenge.currentProgress = 1;
                CheckCompletion(challenge);
            }
            else
            {
                challenge.currentProgress = 0;
            }
        }

        private void UpdateSpeedChallenge(DailyChallenge challenge, TaskResult result)
        {
            if (result.breakdown != null && result.breakdown.timeUsed > 0f)
            {
                if (result.breakdown.timeUsed < challenge.targetValue)
                {
                    challenge.currentProgress = challenge.targetValue; // Complete!
                    CheckCompletion(challenge);
                }
                else
                {
                    challenge.currentProgress = Mathf.FloorToInt(result.breakdown.timeUsed);
                }
            }
        }

        private void UpdateXPChallenge(DailyChallenge challenge, TaskResult result)
        {
            challenge.currentProgress = dailyXPAccumulated;
            CheckCompletion(challenge);
        }

        private void UpdateVarietyChallenge(DailyChallenge challenge, TaskResult result)
        {
            challenge.currentProgress = uniqueTaskTypesCompleted;
            CheckCompletion(challenge);
        }

        private void CheckCompletion(DailyChallenge challenge)
        {
            if (challenge.isComplete) return;

            if (challenge.currentProgress >= challenge.targetValue)
            {
                challenge.isComplete = true;

                // Award bonus XP
                PlayerProgress progress = GameManager.Instance.Progress;
                if (progress != null)
                {
                    progress.AddXP(challenge.xpBonus);

                    // Record challenge completion
                    if (!progress.completedChallenges.Contains(challenge.challengeId))
                    {
                        progress.completedChallenges.Add(challenge.challengeId);
                    }

                    GameManager.Instance.SavePlayerProgress();
                }

                Debug.Log($"[DailyChallenge] COMPLETED: {challenge.description} -- +{challenge.xpBonus} XP bonus!");

                AudioManager.Instance.PlayBadgeEarnedSound();
                OnChallengeCompleted?.Invoke(challenge);
            }
        }

        private void ResetDailyTracking()
        {
            tasksWithoutHints = 0;
            minScoreToday = 100f;
            hasCompletedTaskToday = false;
            fastestTimeToday = float.MaxValue;
            dailyXPAccumulated = 0;
            uniqueTaskTypesCompleted = 0;
            completedTaskTypes.Clear();
        }

        private void SaveChallenge()
        {
            if (cachedChallenge != null)
            {
                string json = JsonUtility.ToJson(cachedChallenge);
                PlayerPrefs.SetString("TradeProof_DailyChallenge", json);
                PlayerPrefs.Save();
            }
        }
    }
}
