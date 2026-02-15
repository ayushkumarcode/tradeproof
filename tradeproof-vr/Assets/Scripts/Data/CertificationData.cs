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

            // GFCI Safety Specialist
            CertificationData gfciSafety = new CertificationData();
            gfciSafety.certificationId = "cert-gfci-safety";
            gfciSafety.certificationName = "GFCI Safety Specialist";
            gfciSafety.description = "Demonstrate expertise in GFCI protection testing, troubleshooting, and replacement per NEC 210.8.";
            gfciSafety.issuingBody = "TradeProof VR Training";
            gfciSafety.difficulty = "intermediate";
            gfciSafety.estimatedStudyHours = 6f;
            gfciSafety.requirements = new List<CertificationRequirement>
            {
                new CertificationRequirement
                {
                    taskId = "gfci-testing-residential",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "outlet-installation-duplex",
                    mode = "Test",
                    minimumScore = 80f,
                    required = true
                }
            };
            certifications.Add(gfciSafety);

            // Conduit Expert
            CertificationData conduitExpert = new CertificationData();
            conduitExpert.certificationId = "cert-conduit-expert";
            conduitExpert.certificationName = "Conduit Bending Expert";
            conduitExpert.description = "Master EMT conduit bending techniques including 90-degree bends, offsets, and saddle bends per NEC 358.";
            conduitExpert.issuingBody = "TradeProof VR Training";
            conduitExpert.difficulty = "intermediate";
            conduitExpert.estimatedStudyHours = 8f;
            conduitExpert.requirements = new List<CertificationRequirement>
            {
                new CertificationRequirement
                {
                    taskId = "conduit-bending-emt",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                }
            };
            certifications.Add(conduitExpert);

            // Troubleshooting Pro
            CertificationData troubleshootingPro = new CertificationData();
            troubleshootingPro.certificationId = "cert-troubleshooting-pro";
            troubleshootingPro.certificationName = "Troubleshooting Professional";
            troubleshootingPro.description = "Proven ability to diagnose and repair residential electrical faults using systematic troubleshooting methodology.";
            troubleshootingPro.issuingBody = "TradeProof VR Training";
            troubleshootingPro.difficulty = "advanced";
            troubleshootingPro.estimatedStudyHours = 12f;
            troubleshootingPro.requirements = new List<CertificationRequirement>
            {
                new CertificationRequirement
                {
                    taskId = "troubleshooting-residential",
                    mode = "Test",
                    minimumScore = 80f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "panel-inspection-residential",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "circuit-wiring-20a",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                }
            };
            certifications.Add(troubleshootingPro);

            // Complete Residential Electrician
            CertificationData residentialComplete = new CertificationData();
            residentialComplete.certificationId = "cert-residential-complete";
            residentialComplete.certificationName = "Complete Residential Electrician";
            residentialComplete.description = "Comprehensive certification covering all residential electrical skills: inspection, wiring, outlets, switches, GFCI, and troubleshooting.";
            residentialComplete.issuingBody = "TradeProof VR Training";
            residentialComplete.difficulty = "advanced";
            residentialComplete.estimatedStudyHours = 20f;
            residentialComplete.requirements = new List<CertificationRequirement>
            {
                new CertificationRequirement
                {
                    taskId = "panel-inspection-residential",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "circuit-wiring-20a",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "outlet-installation-duplex",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "switch-wiring-3way",
                    mode = "Test",
                    minimumScore = 80f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "gfci-testing-residential",
                    mode = "Test",
                    minimumScore = 80f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "troubleshooting-residential",
                    mode = "Test",
                    minimumScore = 75f,
                    required = true
                }
            };
            certifications.Add(residentialComplete);

            // Master Electrician
            CertificationData masterElectrician = new CertificationData();
            masterElectrician.certificationId = "cert-master-electrician";
            masterElectrician.certificationName = "Master Electrician";
            masterElectrician.description = "The highest certification achievable. Requires near-perfect scores on all tasks including conduit bending and advanced troubleshooting.";
            masterElectrician.issuingBody = "TradeProof VR Training";
            masterElectrician.difficulty = "expert";
            masterElectrician.estimatedStudyHours = 40f;
            masterElectrician.requirements = new List<CertificationRequirement>
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
                    minimumScore = 95f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "outlet-installation-duplex",
                    mode = "Test",
                    minimumScore = 90f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "switch-wiring-3way",
                    mode = "Test",
                    minimumScore = 90f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "gfci-testing-residential",
                    mode = "Test",
                    minimumScore = 90f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "conduit-bending-emt",
                    mode = "Test",
                    minimumScore = 90f,
                    required = true
                },
                new CertificationRequirement
                {
                    taskId = "troubleshooting-residential",
                    mode = "Test",
                    minimumScore = 85f,
                    required = true
                }
            };
            certifications.Add(masterElectrician);
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
