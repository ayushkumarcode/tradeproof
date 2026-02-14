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

        public void RecordTaskCompletion(TaskResult result)
        {
            totalTasksAttempted++;

            if (result.passed)
            {
                totalTasksPassed++;
            }

            // Record completion
            TaskCompletionRecord record = new TaskCompletionRecord();
            record.taskId = result.taskId;
            record.mode = result.mode.ToString();
            record.score = result.score;
            record.passed = result.passed;
            record.completedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            completedTasks.Add(record);

            // Update or add score record
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

            // Track badge
            if (result.passed && !string.IsNullOrEmpty(result.badgeId))
            {
                if (!earnedBadgeIds.Contains(result.badgeId))
                {
                    earnedBadgeIds.Add(result.badgeId);
                }
            }

            lastPlayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void AddPracticeTime(float seconds)
        {
            totalPracticeTimeSeconds += seconds;
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
