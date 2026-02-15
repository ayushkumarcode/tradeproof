using UnityEngine;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Data;

namespace TradeProof.AI
{
    /// <summary>
    /// Analyzes PlayerProgress to produce per-task DifficultyProfile settings.
    /// New players get easier settings; experienced players get harder ones.
    /// </summary>
    public class AdaptiveDifficulty : MonoBehaviour
    {
        private static AdaptiveDifficulty _instance;
        public static AdaptiveDifficulty Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AdaptiveDifficulty");
                    _instance = go.AddComponent<AdaptiveDifficulty>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Difficulty profile that governs hint timing, timer scaling,
        /// violation count, and fault complexity for a given task.
        /// </summary>
        [System.Serializable]
        public class DifficultyProfile
        {
            /// <summary>Multiplier for hint delay thresholds. Higher = slower hints.</summary>
            public float hintDelayMultiplier;

            /// <summary>Multiplier for the task timer. Lower = less time.</summary>
            public float timerMultiplier;

            /// <summary>Number of violations / faults to place in the task.</summary>
            public int violationCount;

            /// <summary>Maximum complexity level for faults (0=simple, 1=medium, 2=complex).</summary>
            public int maxFaultComplexity;

            /// <summary>Readable difficulty tier name.</summary>
            public string tierName;

            public override string ToString()
            {
                return $"[{tierName}] hintDelay={hintDelayMultiplier:F1}x, timer={timerMultiplier:F1}x, violations={violationCount}, faultComplexity={maxFaultComplexity}";
            }
        }

        // Cache so we don't recalculate every frame
        private Dictionary<string, DifficultyProfile> profileCache = new Dictionary<string, DifficultyProfile>();
        private float cacheTimestamp;
        private const float CACHE_LIFETIME = 30f; // Recalculate every 30 seconds

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
        /// Returns the DifficultyProfile for the given task based on the player's history.
        /// </summary>
        public DifficultyProfile GetProfile(string taskId)
        {
            // Check cache validity
            if (Time.time - cacheTimestamp > CACHE_LIFETIME)
            {
                profileCache.Clear();
                cacheTimestamp = Time.time;
            }

            if (profileCache.TryGetValue(taskId, out DifficultyProfile cached))
            {
                return cached;
            }

            DifficultyProfile profile = BuildProfile(taskId);
            profileCache[taskId] = profile;

            Debug.Log($"[AdaptiveDifficulty] Profile for '{taskId}': {profile}");
            return profile;
        }

        /// <summary>
        /// Invalidates the cache so profiles are recalculated on next request.
        /// Call this after a task completion or significant progress change.
        /// </summary>
        public void InvalidateCache()
        {
            profileCache.Clear();
            cacheTimestamp = 0f;
        }

        private DifficultyProfile BuildProfile(string taskId)
        {
            PlayerProgress progress = GameManager.Instance.Progress;
            if (progress == null)
            {
                return BuildEasyProfile();
            }

            // Total attempts across all modes for this task
            int totalAttempts = GetTotalAttempts(progress, taskId);
            float bestPassRate = GetBestPassRate(progress, taskId);

            // Determine tier
            // Experienced: 6+ attempts OR >85% pass rate
            if (totalAttempts >= 6 || bestPassRate > 85f)
            {
                return BuildHardProfile();
            }
            // Intermediate: 3-5 attempts
            else if (totalAttempts >= 3)
            {
                return BuildNormalProfile();
            }
            // New player: 0-2 attempts
            else
            {
                return BuildEasyProfile();
            }
        }

        private int GetTotalAttempts(PlayerProgress progress, string taskId)
        {
            int total = 0;
            foreach (var record in progress.scores)
            {
                if (record.taskId == taskId)
                {
                    total += record.attempts;
                }
            }
            return total;
        }

        private float GetBestPassRate(PlayerProgress progress, string taskId)
        {
            float bestScore = 0f;
            foreach (var record in progress.scores)
            {
                if (record.taskId == taskId && record.bestScore > bestScore)
                {
                    bestScore = record.bestScore;
                }
            }
            return bestScore;
        }

        private DifficultyProfile BuildEasyProfile()
        {
            return new DifficultyProfile
            {
                hintDelayMultiplier = 1f,
                timerMultiplier = 1.2f,      // 20% more time
                violationCount = 3,           // Fewer violations
                maxFaultComplexity = 0,       // Simple faults only
                tierName = "Easy"
            };
        }

        private DifficultyProfile BuildNormalProfile()
        {
            return new DifficultyProfile
            {
                hintDelayMultiplier = 1f,
                timerMultiplier = 1f,         // Normal time
                violationCount = 5,           // Standard violation count
                maxFaultComplexity = 1,       // Medium complexity
                tierName = "Normal"
            };
        }

        private DifficultyProfile BuildHardProfile()
        {
            return new DifficultyProfile
            {
                hintDelayMultiplier = 2f,     // Hints come half as frequently
                timerMultiplier = 0.8f,       // 20% less time
                violationCount = 7,           // More violations
                maxFaultComplexity = 2,       // Complex faults
                tierName = "Hard"
            };
        }

        /// <summary>
        /// Returns the adjusted time limit for a task given its base time limit.
        /// </summary>
        public float GetAdjustedTimeLimit(string taskId, float baseTimeLimit)
        {
            DifficultyProfile profile = GetProfile(taskId);
            return baseTimeLimit * profile.timerMultiplier;
        }

        /// <summary>
        /// Returns the adjusted hint delay multiplier for a task.
        /// </summary>
        public float GetHintDelayMultiplier(string taskId)
        {
            DifficultyProfile profile = GetProfile(taskId);
            return profile.hintDelayMultiplier;
        }

        /// <summary>
        /// Returns how many violations/faults should be placed for this task.
        /// </summary>
        public int GetViolationCount(string taskId)
        {
            DifficultyProfile profile = GetProfile(taskId);
            return profile.violationCount;
        }

        /// <summary>
        /// Returns the maximum fault complexity for this task (0=simple, 1=medium, 2=complex).
        /// </summary>
        public int GetMaxFaultComplexity(string taskId)
        {
            DifficultyProfile profile = GetProfile(taskId);
            return profile.maxFaultComplexity;
        }
    }
}
