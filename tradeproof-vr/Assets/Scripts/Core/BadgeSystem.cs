using UnityEngine;
using System;
using System.Collections.Generic;

namespace TradeProof.Core
{
    [Serializable]
    public class Badge
    {
        public string badgeId;
        public string badgeName;
        public string taskId;
        public float score;
        public string dateEarned;
        public string difficulty;
    }

    [Serializable]
    public class BadgeCollection
    {
        public List<Badge> badges = new List<Badge>();
    }

    public class BadgeSystem : MonoBehaviour
    {
        private static BadgeSystem _instance;
        public static BadgeSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("BadgeSystem");
                    _instance = go.AddComponent<BadgeSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public event Action<Badge> OnBadgeEarned;

        private BadgeCollection badgeCollection;
        private const string PREFS_KEY = "TradeProof_Badges";

        // Badge definitions
        private static readonly Dictionary<string, string> BadgeNames = new Dictionary<string, string>
        {
            { "panel-inspection-level-1", "Panel Inspection - Level 1" },
            { "circuit-wiring-level-1", "Circuit Wiring - Level 1" },
            { "panel-inspection-level-2", "Panel Inspection - Level 2" },
            { "circuit-wiring-level-2", "Circuit Wiring - Level 2" },
            { "speed-demon", "Speed Demon" },
            { "perfectionist", "Perfectionist" },
            { "nec-scholar", "NEC Scholar" }
        };

        private static readonly Dictionary<string, string> BadgeDifficulty = new Dictionary<string, string>
        {
            { "panel-inspection-level-1", "beginner" },
            { "circuit-wiring-level-1", "intermediate" },
            { "panel-inspection-level-2", "intermediate" },
            { "circuit-wiring-level-2", "advanced" },
            { "speed-demon", "special" },
            { "perfectionist", "special" },
            { "nec-scholar", "special" }
        };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadBadges();
        }

        private void LoadBadges()
        {
            string json = PlayerPrefs.GetString(PREFS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                badgeCollection = JsonUtility.FromJson<BadgeCollection>(json);
            }
            else
            {
                badgeCollection = new BadgeCollection();
            }
        }

        private void SaveBadges()
        {
            string json = JsonUtility.ToJson(badgeCollection);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        public bool HasBadge(string badgeId)
        {
            foreach (var badge in badgeCollection.badges)
            {
                if (badge.badgeId == badgeId) return true;
            }
            return false;
        }

        public Badge GetBadge(string badgeId)
        {
            foreach (var badge in badgeCollection.badges)
            {
                if (badge.badgeId == badgeId) return badge;
            }
            return null;
        }

        public List<Badge> GetAllBadges()
        {
            return new List<Badge>(badgeCollection.badges);
        }

        public int GetBadgeCount()
        {
            return badgeCollection.badges.Count;
        }

        public void AwardBadge(string badgeId, string taskId, float score)
        {
            if (string.IsNullOrEmpty(badgeId))
            {
                Debug.LogWarning("[BadgeSystem] Cannot award badge — badgeId is null or empty");
                return;
            }

            // Check if already earned (update if higher score)
            Badge existing = GetBadge(badgeId);
            if (existing != null)
            {
                if (score > existing.score)
                {
                    existing.score = score;
                    existing.dateEarned = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SaveBadges();
                    Debug.Log($"[BadgeSystem] Badge score updated: {badgeId} — new score: {score:F1}%");
                }
                return;
            }

            // Award new badge
            Badge newBadge = new Badge();
            newBadge.badgeId = badgeId;
            newBadge.badgeName = BadgeNames.ContainsKey(badgeId) ? BadgeNames[badgeId] : badgeId;
            newBadge.taskId = taskId;
            newBadge.score = score;
            newBadge.dateEarned = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            newBadge.difficulty = BadgeDifficulty.ContainsKey(badgeId) ? BadgeDifficulty[badgeId] : "unknown";

            badgeCollection.badges.Add(newBadge);
            SaveBadges();

            Debug.Log($"[BadgeSystem] Badge earned: {newBadge.badgeName} (Score: {score:F1}%)");
            OnBadgeEarned?.Invoke(newBadge);

            // Check for special badges
            CheckSpecialBadges(score);
        }

        private void CheckSpecialBadges(float latestScore)
        {
            // Perfectionist: scored 100% on any task
            if (latestScore >= 100f && !HasBadge("perfectionist"))
            {
                Badge perfectBadge = new Badge();
                perfectBadge.badgeId = "perfectionist";
                perfectBadge.badgeName = BadgeNames["perfectionist"];
                perfectBadge.taskId = "any";
                perfectBadge.score = 100f;
                perfectBadge.dateEarned = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                perfectBadge.difficulty = "special";

                badgeCollection.badges.Add(perfectBadge);
                SaveBadges();
                OnBadgeEarned?.Invoke(perfectBadge);
            }

            // NEC Scholar: earned badges for both panel inspection and circuit wiring
            if (HasBadge("panel-inspection-level-1") && HasBadge("circuit-wiring-level-1") && !HasBadge("nec-scholar"))
            {
                Badge scholarBadge = new Badge();
                scholarBadge.badgeId = "nec-scholar";
                scholarBadge.badgeName = BadgeNames["nec-scholar"];
                scholarBadge.taskId = "multiple";
                scholarBadge.score = 100f;
                scholarBadge.dateEarned = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                scholarBadge.difficulty = "special";

                badgeCollection.badges.Add(scholarBadge);
                SaveBadges();
                OnBadgeEarned?.Invoke(scholarBadge);
            }
        }

        public string ExportBadgesAsJson()
        {
            ExportableBadgeProfile profile = new ExportableBadgeProfile();
            profile.platform = "TradeProof VR";
            profile.exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            profile.totalBadges = badgeCollection.badges.Count;
            profile.badges = new List<ExportableBadge>();

            foreach (var badge in badgeCollection.badges)
            {
                ExportableBadge eb = new ExportableBadge();
                eb.name = badge.badgeName;
                eb.category = badge.taskId;
                eb.score = badge.score;
                eb.dateEarned = badge.dateEarned;
                eb.difficulty = badge.difficulty;
                eb.verified = true;
                profile.badges.Add(eb);
            }

            return JsonUtility.ToJson(profile, true);
        }

        public void ClearAllBadges()
        {
            badgeCollection = new BadgeCollection();
            SaveBadges();
            Debug.Log("[BadgeSystem] All badges cleared");
        }
    }

    [Serializable]
    public class ExportableBadgeProfile
    {
        public string platform;
        public string exportDate;
        public int totalBadges;
        public List<ExportableBadge> badges;
    }

    [Serializable]
    public class ExportableBadge
    {
        public string name;
        public string category;
        public float score;
        public string dateEarned;
        public string difficulty;
        public bool verified;
    }
}
