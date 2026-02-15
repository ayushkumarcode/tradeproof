using UnityEngine;
using System.Collections.Generic;

namespace TradeProof.Environment
{
    public class JobSiteManager : MonoBehaviour
    {
        private static JobSiteManager _instance;
        public static JobSiteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("JobSiteManager");
                    _instance = go.AddComponent<JobSiteManager>();
                }
                return _instance;
            }
        }

        private GameObject currentJobSite;
        private RoomBuilder roomBuilder;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            roomBuilder = gameObject.AddComponent<RoomBuilder>();
        }

        public void LoadJobSite(string jobSiteType)
        {
            ClearJobSite();

            RoomDefinition room = GetRoomDefinition(jobSiteType);
            if (room != null)
            {
                currentJobSite = roomBuilder.BuildRoom(room);
                Debug.Log($"[JobSiteManager] Loaded job site: {jobSiteType}");
            }
        }

        public void ClearJobSite()
        {
            if (currentJobSite != null)
            {
                Destroy(currentJobSite);
                currentJobSite = null;
            }
        }

        private RoomDefinition GetRoomDefinition(string jobSiteType)
        {
            switch (jobSiteType)
            {
                case "residential-kitchen":
                    return new RoomDefinition
                    {
                        width = 4f, length = 5f, height = 2.7f,
                        wallColor = new Color(0.85f, 0.82f, 0.75f),
                        floorColor = new Color(0.6f, 0.5f, 0.4f),
                        ceilingColor = new Color(0.95f, 0.95f, 0.95f),
                        panelPosition = new Vector3(-1.9f, 1.2f, -2.4f),
                        outletPositions = new[] {
                            new Vector3(1.9f, 0.35f, -1f),
                            new Vector3(1.9f, 0.35f, 1f),
                            new Vector3(-1f, 1.1f, -2.4f)
                        },
                        switchPositions = new[] {
                            new Vector3(-1.9f, 1.2f, 0f)
                        }
                    };

                case "residential-garage":
                    return new RoomDefinition
                    {
                        width = 6f, length = 7f, height = 3f,
                        wallColor = new Color(0.7f, 0.7f, 0.7f),
                        floorColor = new Color(0.4f, 0.4f, 0.4f),
                        ceilingColor = new Color(0.6f, 0.6f, 0.6f),
                        panelPosition = new Vector3(-2.9f, 1.5f, -3.4f),
                        outletPositions = new[] {
                            new Vector3(2.9f, 0.35f, -2f),
                            new Vector3(2.9f, 0.35f, 0f),
                            new Vector3(2.9f, 0.35f, 2f)
                        },
                        switchPositions = new[] {
                            new Vector3(-2.9f, 1.2f, 3f)
                        }
                    };

                case "residential-bathroom":
                    return new RoomDefinition
                    {
                        width = 2.5f, length = 3f, height = 2.7f,
                        wallColor = new Color(0.9f, 0.9f, 0.95f),
                        floorColor = new Color(0.8f, 0.8f, 0.85f),
                        ceilingColor = new Color(0.95f, 0.95f, 0.95f),
                        panelPosition = Vector3.zero, // No panel in bathroom
                        outletPositions = new[] {
                            new Vector3(1.15f, 1.0f, -1f)
                        },
                        switchPositions = new[] {
                            new Vector3(-1.15f, 1.2f, 1.3f)
                        }
                    };

                case "commercial-office":
                    return new RoomDefinition
                    {
                        width = 8f, length = 10f, height = 3.5f,
                        wallColor = new Color(0.88f, 0.88f, 0.85f),
                        floorColor = new Color(0.35f, 0.35f, 0.38f),
                        ceilingColor = new Color(0.9f, 0.9f, 0.9f),
                        panelPosition = new Vector3(-3.9f, 1.5f, -4.9f),
                        outletPositions = new[] {
                            new Vector3(3.9f, 0.35f, -3f),
                            new Vector3(3.9f, 0.35f, 0f),
                            new Vector3(3.9f, 0.35f, 3f),
                            new Vector3(-3.9f, 0.35f, -1f),
                            new Vector3(-3.9f, 0.35f, 2f)
                        },
                        switchPositions = new[] {
                            new Vector3(-3.9f, 1.2f, 4.5f),
                            new Vector3(3.9f, 1.2f, 4.5f)
                        }
                    };

                default:
                    Debug.LogWarning($"[JobSiteManager] Unknown job site type: {jobSiteType}");
                    return new RoomDefinition
                    {
                        width = 4f, length = 5f, height = 2.7f,
                        wallColor = new Color(0.8f, 0.8f, 0.8f),
                        floorColor = new Color(0.5f, 0.5f, 0.5f),
                        ceilingColor = new Color(0.9f, 0.9f, 0.9f),
                        panelPosition = new Vector3(-1.9f, 1.5f, -2.4f),
                        outletPositions = new Vector3[0],
                        switchPositions = new Vector3[0]
                    };
            }
        }
    }

    [System.Serializable]
    public class RoomDefinition
    {
        public float width;
        public float length;
        public float height;
        public Color wallColor;
        public Color floorColor;
        public Color ceilingColor;
        public Vector3 panelPosition;
        public Vector3[] outletPositions;
        public Vector3[] switchPositions;
    }
}
