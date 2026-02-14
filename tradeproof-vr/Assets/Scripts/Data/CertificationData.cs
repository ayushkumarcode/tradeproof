using System;
using System.Collections.Generic;

namespace TradeProof.Data
{
    [Serializable]
    public class CertificationData
    {
        public string certificationId;
        public string certificationName;
        public string description;
        public string issuingBody;
        public List<CertificationRequirement> requirements;
        public string difficulty;
        public float estimatedStudyHours;
    }

    [Serializable]
    public class CertificationRequirement
    {
        public string taskId;
        public string mode;
        public float minimumScore;
        public bool required;
    }

    [Serializable]
    public class CertificationProgress
    {
        public string certificationId;
        public List<RequirementStatus> requirementStatuses;
        public bool isComplete;
        public string completionDate;
    }

    [Serializable]
    public class RequirementStatus
    {
        public string taskId;
        public string mode;
        public float currentScore;
        public bool met;
    }

    public static class CertificationDatabase
    {
        private static List<CertificationData> certifications;

        public static void Initialize()
        {
            certifications = new List<CertificationData>();

            // Residential Electrical Basics
            CertificationData residentialBasics = new CertificationData();
            residentialBasics.certificationId = "cert-residential-basics";
            residentialBasics.certificationName = "Residential Electrical Basics";
            residentialBasics.description = "Fundamental skills for residential electrical work including panel inspection and basic circuit wiring.";
            residentialBasics.issuingBody = "TradeProof VR Training";
            residentialBasics.difficulty = "beginner";
            residentialBasics.estimatedStudyHours = 4f;
            residentialBasics.requirements = new List<CertificationRequirement>
            {
                new CertificationRequirement
                {
                    taskId = "panel-inspection-residential",
                    mode = "Test",
                    minimumScore = 80f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "circuit-wiring-20a",
                    mode = "Test",
                    minimumScore = 80f,
                    required = true
                }
            };
            certifications.Add(residentialBasics);

            // NEC Code Compliance
            CertificationData necCompliance = new CertificationData();
            necCompliance.certificationId = "cert-nec-compliance";
            necCompliance.certificationName = "NEC Code Compliance Specialist";
            necCompliance.description = "Advanced understanding of NEC code requirements for residential electrical installations.";
            necCompliance.issuingBody = "TradeProof VR Training";
            necCompliance.difficulty = "intermediate";
            necCompliance.estimatedStudyHours = 8f;
            necCompliance.requirements = new List<CertificationRequirement>
            {
                new CertificationRequirement
                {
                    taskId = "panel-inspection-residential",
                    mode = "Test",
                    minimumScore = 95f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "circuit-wiring-20a",
                    mode = "Test",
                    minimumScore = 90f,
                    required = true
                }
            };
            certifications.Add(necCompliance);
        }

        public static List<CertificationData> GetAllCertifications()
        {
            if (certifications == null)
                Initialize();
            return new List<CertificationData>(certifications);
        }

        public static CertificationData GetCertification(string certId)
        {
            if (certifications == null)
                Initialize();

            foreach (var cert in certifications)
            {
                if (cert.certificationId == certId)
                    return cert;
            }
            return null;
        }

        public static CertificationProgress CheckProgress(string certId, PlayerProgress playerProgress)
        {
            CertificationData cert = GetCertification(certId);
            if (cert == null) return null;

            CertificationProgress progress = new CertificationProgress();
            progress.certificationId = certId;
            progress.requirementStatuses = new List<RequirementStatus>();
            progress.isComplete = true;

            foreach (var req in cert.requirements)
            {
                RequirementStatus status = new RequirementStatus();
                status.taskId = req.taskId;
                status.mode = req.mode;
                status.currentScore = playerProgress.GetBestScore(req.taskId, req.mode);
                status.met = status.currentScore >= req.minimumScore;

                if (req.required && !status.met)
                {
                    progress.isComplete = false;
                }

                progress.requirementStatuses.Add(status);
            }

            if (progress.isComplete)
            {
                progress.completionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return progress;
        }
    }
}
