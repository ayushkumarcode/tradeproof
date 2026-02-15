using UnityEngine;
using System;
using System.Collections.Generic;

namespace TradeProof.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public List<TaskCompletionRecord> completedTasks = new List<TaskCompletionRecord>();
        public List<TaskScoreRecord> scores = new List<TaskScoreRecord>();
        public List<string> earnedBadgeIds = new List<string>();
        public float totalPracticeTimeSeconds;
        public int totalTasksAttempted;
        public int totalTasksPassed;
        public string lastPlayDate;

        // Career progression
        public int totalXP;
        public int currentCareerLevel; // 0=Apprentice, 1=Journeyman, 2=Master
        public int daysCompleted;
        public List<string> unlockedTaskIds = new List<string>();
        public List<string> completedChallenges = new List<string>();
        public int currentDayStreak;
        public string lastDayPlayedDate;

        // XP thresholds
        public static readonly int[] CareerXPThresholds = { 0, 500, 2000 };

        public void RecordTaskCompletion(TaskResult result)
        {
            totalTasksAttempted++;

            if (result.passed)
            {
                totalTasksPassed++;
            }

            TaskCompletionRecord record = new TaskCompletionRecord();
            record.taskId = result.taskId;
            record.mode = result.mode.ToString();
            record.score = result.score;
            record.passed = result.passed;
            record.completedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            completedTasks.Add(record);

            TaskScoreRecord existingScore = null;
            foreach (var s in scores)
            {
                if (s.taskId == result.taskId && s.mode == result.mode.ToString())
                {
                    existingScore = s;
                    break;
                }
            }

            if (existingScore != null)
            {
                existingScore.attempts++;
                if (result.score > existingScore.bestScore)
                {
                    existingScore.bestScore = result.score;
                    existingScore.bestDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                existingScore.lastScore = result.score;
                existingScore.lastDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                TaskScoreRecord newScore = new TaskScoreRecord();
                newScore.taskId = result.taskId;
                newScore.mode = result.mode.ToString();
                newScore.bestScore = result.score;
                newScore.lastScore = result.score;
                newScore.attempts = 1;
                newScore.bestDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                newScore.lastDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                scores.Add(newScore);
            }

            if (result.passed && !string.IsNullOrEmpty(result.badgeId))
            {
                if (!earnedBadgeIds.Contains(result.badgeId))
                {
                    earnedBadgeIds.Add(result.badgeId);
                }
            }

            // Track day streak
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (lastDayPlayedDate != today)
            {
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                if (lastDayPlayedDate == yesterday)
                    currentDayStreak++;
                else
                    currentDayStreak = 1;
                lastDayPlayedDate = today;
            }

            lastPlayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void AddPracticeTime(float seconds)
        {
            totalPracticeTimeSeconds += seconds;
        }

        public void AddXP(int amount)
        {
            totalXP += amount;

            // Check for career level up
            int newLevel = 0;
            for (int i = CareerXPThresholds.Length - 1; i >= 0; i--)
            {
                if (totalXP >= CareerXPThresholds[i])
                {
                    newLevel = i;
                    break;
                }
            }

            if (newLevel > currentCareerLevel)
            {
                currentCareerLevel = newLevel;
                Debug.Log($"[PlayerProgress] Career level up! Now: {GetCareerLevel()} (XP: {totalXP})");
            }
        }

        public Core.CareerLevel GetCareerLevel()
        {
            return (Core.CareerLevel)currentCareerLevel;
        }

        public int GetXPForNextLevel()
        {
            if (currentCareerLevel >= CareerXPThresholds.Length - 1)
                return 0; // Already max level
            return CareerXPThresholds[currentCareerLevel + 1] - totalXP;
        }

        public float GetXPProgressToNextLevel()
        {
            if (currentCareerLevel >= CareerXPThresholds.Length - 1)
                return 1f;
            int currentThreshold = CareerXPThresholds[currentCareerLevel];
            int nextThreshold = CareerXPThresholds[currentCareerLevel + 1];
            return (float)(totalXP - currentThreshold) / (nextThreshold - currentThreshold);
        }

        public bool IsTaskUnlocked(string taskId)
        {
            // All beginner tasks are always unlocked
            if (taskId == "panel-inspection-residential" || taskId == "circuit-wiring-20a" || taskId == "outlet-installation-duplex")
                return true;

            // Check if explicitly unlocked
            if (unlockedTaskIds.Contains(taskId))
                return true;

            // Journeyman tasks unlock at Journeyman level
            if (currentCareerLevel >= 1 &&
                (taskId == "switch-wiring-3way" || taskId == "gfci-testing-residential" || taskId == "conduit-bending-emt"))
                return true;

            // Master tasks unlock at Master level
            if (currentCareerLevel >= 2 && taskId == "troubleshooting-residential")
                return true;

            return false;
        }

        public void UnlockTask(string taskId)
        {
            if (!unlockedTaskIds.Contains(taskId))
                unlockedTaskIds.Add(taskId);
        }

        public float GetBestScore(string taskId, string mode)
        {
            foreach (var s in scores)
            {
                if (s.taskId == taskId && s.mode == mode)
                {
                    return s.bestScore;
                }
            }
            return 0f;
        }

        public int GetAttemptCount(string taskId, string mode)
        {
            foreach (var s in scores)
            {
                if (s.taskId == taskId && s.mode == mode)
                {
                    return s.attempts;
                }
            }
            return 0;
        }

        public bool HasCompletedTask(string taskId, string mode)
        {
            foreach (var record in completedTasks)
            {
                if (record.taskId == taskId && record.mode == mode && record.passed)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasEarnedBadge(string badgeId)
        {
            return earnedBadgeIds.Contains(badgeId);
        }

        public string GetFormattedPracticeTime()
        {
            TimeSpan ts = TimeSpan.FromSeconds(totalPracticeTimeSeconds);
            if (ts.TotalHours >= 1)
            {
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            }
            return $"{ts.Minutes}m {ts.Seconds}s";
        }

        public float GetOverallPassRate()
        {
            if (totalTasksAttempted == 0) return 0f;
            return (float)totalTasksPassed / totalTasksAttempted * 100f;
        }
    }

    [Serializable]
    public class TaskCompletionRecord
    {
        public string taskId;
        public string mode;
        public float score;
        public bool passed;
        public string completedDate;
    }

    [Serializable]
    public class TaskScoreRecord
    {
        public string taskId;
        public string mode;
        public float bestScore;
        public float lastScore;
        public int attempts;
        public string bestDate;
        public string lastDate;
    }
}
