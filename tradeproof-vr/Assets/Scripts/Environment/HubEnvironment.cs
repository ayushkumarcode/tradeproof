using UnityEngine;
using System.Collections.Generic;
using TradeProof.Core;
using TradeProof.Interaction;
using TradeProof.UI;

namespace TradeProof.Environment
{
    /// <summary>
    /// Creates the player's home base workshop between jobs.
    /// Procedurally builds: workshop room, workbench with pegboard, job board,
    /// van/truck, badge wall, and overhead fluorescent lights.
    /// </summary>
    public class HubEnvironment : MonoBehaviour
    {
        [Header("Room Dimensions")]
        [SerializeField] private float roomWidth = 8f;
        [SerializeField] private float roomLength = 10f;
        [SerializeField] private float roomHeight = 3.5f;

        [Header("Colors")]
        [SerializeField] private Color floorColor = new Color(0.35f, 0.35f, 0.38f);    // Gray concrete
        [SerializeField] private Color wallColor = new Color(0.88f, 0.88f, 0.85f);      // Off-white walls
        [SerializeField] private Color ceilingColor = new Color(0.8f, 0.8f, 0.8f);       // Lighter ceiling

        // Generated objects
        private GameObject roomObject;
        private Workbench workbench;
        private GameObject jobBoard;
        private GameObject van;
        private GameObject badgeWall;
        private List<GameObject> fluorescents = new List<GameObject>();

        // References
        private RoomBuilder roomBuilder;

        private void Awake()
        {
            BuildHub();
        }

        /// <summary>
        /// Procedurally generates the entire hub environment.
        /// </summary>
        public void BuildHub()
        {
            // Clean up any existing hub
            ClearHub();

            BuildRoom();
            BuildWorkbench();
            BuildJobBoard();
            BuildVan();
            BuildBadgeWall();
            BuildFluorescentLights();

            Debug.Log("[HubEnvironment] Workshop hub built successfully.");
        }

        /// <summary>
        /// Removes all generated hub objects.
        /// </summary>
        public void ClearHub()
        {
            if (roomObject != null) Destroy(roomObject);
            if (jobBoard != null) Destroy(jobBoard);
            if (van != null) Destroy(van);
            if (badgeWall != null) Destroy(badgeWall);
            foreach (var light in fluorescents)
            {
                if (light != null) Destroy(light);
            }
            fluorescents.Clear();
        }

        // --- Room Construction ---

        private void BuildRoom()
        {
            // Use RoomBuilder if available, otherwise build inline
            roomBuilder = GetComponent<RoomBuilder>();
            if (roomBuilder == null)
            {
                roomBuilder = gameObject.AddComponent<RoomBuilder>();
            }

            RoomDefinition def = new RoomDefinition
            {
                width = roomWidth,
                length = roomLength,
                height = roomHeight,
                wallColor = wallColor,
                floorColor = floorColor,
                ceilingColor = ceilingColor,
                panelPosition = Vector3.zero,
                outletPositions = new Vector3[0],
                switchPositions = new Vector3[0]
            };

            roomObject = roomBuilder.BuildRoom(def);
            roomObject.name = "Workshop_Room";
            roomObject.transform.SetParent(transform, false);
        }

        // --- Workbench ---

        private void BuildWorkbench()
        {
            // Place workbench against the north wall
            GameObject workbenchObj = new GameObject("Workbench");
            workbenchObj.transform.SetParent(transform, false);
            workbenchObj.transform.localPosition = new Vector3(0f, 0f, roomLength / 2f - 0.5f);

            workbench = workbenchObj.AddComponent<Workbench>();
        }

        // --- Job Board ---

        private void BuildJobBoard()
        {
            // Flat cube on the east wall, 1.2m x 0.8m
            jobBoard = new GameObject("JobBoard");
            jobBoard.transform.SetParent(transform, false);
            jobBoard.transform.localPosition = new Vector3(roomWidth / 2f - 0.06f, 1.4f, -1f);
            jobBoard.transform.localRotation = Quaternion.Euler(0f, -90f, 0f); // Face inward

            // Board backing
            GameObject boardBacking = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardBacking.name = "BoardBacking";
            boardBacking.transform.SetParent(jobBoard.transform, false);
            boardBacking.transform.localPosition = Vector3.zero;
            boardBacking.transform.localScale = new Vector3(1.2f, 0.8f, 0.03f);
            Renderer backingRend = boardBacking.GetComponent<Renderer>();
            Material corkMat = new Material(Shader.Find("Standard"));
            corkMat.color = new Color(0.6f, 0.45f, 0.25f); // Cork board color
            backingRend.material = corkMat;

            // Frame (4 thin cubes)
            Color frameColor = new Color(0.3f, 0.2f, 0.1f); // Dark wood
            CreateFramePiece(jobBoard, "FrameTop", new Vector3(0f, 0.41f, 0f), new Vector3(1.24f, 0.03f, 0.04f), frameColor);
            CreateFramePiece(jobBoard, "FrameBottom", new Vector3(0f, -0.41f, 0f), new Vector3(1.24f, 0.03f, 0.04f), frameColor);
            CreateFramePiece(jobBoard, "FrameLeft", new Vector3(-0.61f, 0f, 0f), new Vector3(0.03f, 0.85f, 0.04f), frameColor);
            CreateFramePiece(jobBoard, "FrameRight", new Vector3(0.61f, 0f, 0f), new Vector3(0.03f, 0.85f, 0.04f), frameColor);

            // Header text label
            GameObject headerObj = new GameObject("HeaderLabel");
            headerObj.transform.SetParent(jobBoard.transform, false);
            headerObj.transform.localPosition = new Vector3(0f, 0.5f, -0.02f);
            headerObj.transform.localScale = Vector3.one * 0.15f;
            FloatingLabel headerLabel = headerObj.AddComponent<FloatingLabel>();
            headerLabel.SetText("JOB BOARD");
            headerLabel.SetFontSize(4f);
            headerLabel.SetColor(new Color(0.9f, 0.7f, 0.1f));
            headerLabel.SetBillboard(false);

            // WorkOrderBoardUI can mount onto this object
            // Leave a transform marker for it
            GameObject uiMount = new GameObject("WorkOrderBoardUI_Mount");
            uiMount.transform.SetParent(jobBoard.transform, false);
            uiMount.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        }

        private void CreateFramePiece(GameObject parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent.transform, false);
            piece.transform.localPosition = localPos;
            piece.transform.localScale = scale;
            Renderer rend = piece.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            rend.material = mat;

            // Remove collider from decorative elements
            Collider col = piece.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        // --- Van / Truck ---

        private void BuildVan()
        {
            // Simple box shape near the "door" area (south wall)
            van = new GameObject("WorkVan");
            van.transform.SetParent(transform, false);
            van.transform.localPosition = new Vector3(2f, 0f, -roomLength / 2f + 2f);

            // Van body: 2m x 1.8m x 3m
            GameObject vanBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vanBody.name = "VanBody";
            vanBody.transform.SetParent(van.transform, false);
            vanBody.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            vanBody.transform.localScale = new Vector3(2f, 1.8f, 3f);
            Renderer vanRend = vanBody.GetComponent<Renderer>();
            Material vanMat = new Material(Shader.Find("Standard"));
            vanMat.color = new Color(0.9f, 0.9f, 0.95f); // White van
            vanRend.material = vanMat;

            // Cab section (slightly lower, in front)
            GameObject cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cab.name = "VanCab";
            cab.transform.SetParent(van.transform, false);
            cab.transform.localPosition = new Vector3(0f, 0.7f, -1.8f);
            cab.transform.localScale = new Vector3(1.8f, 1.4f, 1.2f);
            Renderer cabRend = cab.GetComponent<Renderer>();
            cabRend.material = vanMat;

            // Windshield
            GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windshield.name = "Windshield";
            windshield.transform.SetParent(van.transform, false);
            windshield.transform.localPosition = new Vector3(0f, 1.0f, -2.35f);
            windshield.transform.localScale = new Vector3(1.6f, 0.7f, 0.02f);
            Renderer wsRend = windshield.GetComponent<Renderer>();
            Material glassMat = new Material(Shader.Find("Standard"));
            glassMat.color = new Color(0.4f, 0.5f, 0.6f, 0.5f);
            glassMat.SetFloat("_Mode", 3);
            glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            glassMat.SetInt("_ZWrite", 0);
            glassMat.EnableKeyword("_ALPHABLEND_ON");
            glassMat.renderQueue = 3000;
            wsRend.material = glassMat;
            Collider wsCol = windshield.GetComponent<Collider>();
            if (wsCol != null) Destroy(wsCol);

            // Wheels (4 cylinders)
            CreateWheel(van, "WheelFL", new Vector3(-0.85f, 0.2f, -1.2f));
            CreateWheel(van, "WheelFR", new Vector3(0.85f, 0.2f, -1.2f));
            CreateWheel(van, "WheelRL", new Vector3(-0.85f, 0.2f, 0.8f));
            CreateWheel(van, "WheelRR", new Vector3(0.85f, 0.2f, 0.8f));

            // Company label
            GameObject labelObj = new GameObject("VanLabel");
            labelObj.transform.SetParent(van.transform, false);
            labelObj.transform.localPosition = new Vector3(1.01f, 1.2f, 0f);
            labelObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            labelObj.transform.localScale = Vector3.one * 0.15f;
            FloatingLabel vanLabel = labelObj.AddComponent<FloatingLabel>();
            vanLabel.SetText("TradeProof\nElectrical");
            vanLabel.SetFontSize(4f);
            vanLabel.SetColor(new Color(0.1f, 0.3f, 0.7f));
            vanLabel.SetBillboard(false);

            // Travel trigger (invisible collider near the van door area)
            GameObject travelTrigger = new GameObject("TravelTrigger");
            travelTrigger.transform.SetParent(van.transform, false);
            travelTrigger.transform.localPosition = new Vector3(-1.2f, 0.5f, 0f);
            BoxCollider triggerCol = travelTrigger.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(1f, 2f, 1.5f);
        }

        private void CreateWheel(GameObject parent, string name, Vector3 localPos)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent.transform, false);
            wheel.transform.localPosition = localPos;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.4f, 0.15f, 0.4f);
            Renderer wheelRend = wheel.GetComponent<Renderer>();
            Material tireMat = new Material(Shader.Find("Standard"));
            tireMat.color = new Color(0.15f, 0.15f, 0.15f); // Black tire
            wheelRend.material = tireMat;

            Collider wheelCol = wheel.GetComponent<Collider>();
            if (wheelCol != null) Destroy(wheelCol);
        }

        // --- Badge Wall ---

        private void BuildBadgeWall()
        {
            // Display area on the west wall for earned badges
            badgeWall = new GameObject("BadgeWall");
            badgeWall.transform.SetParent(transform, false);
            badgeWall.transform.localPosition = new Vector3(-roomWidth / 2f + 0.06f, 1.5f, 1f);
            badgeWall.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Backing panel
            GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backing.name = "BadgeWallBacking";
            backing.transform.SetParent(badgeWall.transform, false);
            backing.transform.localPosition = Vector3.zero;
            backing.transform.localScale = new Vector3(2f, 1.2f, 0.02f);
            Renderer backRend = backing.GetComponent<Renderer>();
            Material backMat = new Material(Shader.Find("Standard"));
            backMat.color = new Color(0.15f, 0.12f, 0.1f); // Dark wood
            backRend.material = backMat;

            // Header
            GameObject headerObj = new GameObject("BadgeWallHeader");
            headerObj.transform.SetParent(badgeWall.transform, false);
            headerObj.transform.localPosition = new Vector3(0f, 0.7f, -0.02f);
            headerObj.transform.localScale = Vector3.one * 0.12f;
            FloatingLabel headerLabel = headerObj.AddComponent<FloatingLabel>();
            headerLabel.SetText("ACHIEVEMENTS");
            headerLabel.SetFontSize(4f);
            headerLabel.SetColor(new Color(0.9f, 0.75f, 0.1f));
            headerLabel.SetBillboard(false);

            // Create placeholder badge slots (3x3 grid)
            float slotSpacing = 0.4f;
            float startX = -slotSpacing;
            float startY = -0.1f;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float x = startX + col * slotSpacing;
                    float y = startY - row * slotSpacing;

                    GameObject slot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    slot.name = $"BadgeSlot_{row}_{col}";
                    slot.transform.SetParent(badgeWall.transform, false);
                    slot.transform.localPosition = new Vector3(x, y, -0.015f);
                    slot.transform.localScale = new Vector3(0.25f, 0.25f, 0.01f);
                    Renderer slotRend = slot.GetComponent<Renderer>();
                    Material slotMat = new Material(Shader.Find("Standard"));
                    slotMat.color = new Color(0.2f, 0.18f, 0.15f); // Slightly lighter than backing
                    slotRend.material = slotMat;

                    Collider slotCol = slot.GetComponent<Collider>();
                    if (slotCol != null) Destroy(slotCol);
                }
            }
        }

        // --- Fluorescent Lights ---

        private void BuildFluorescentLights()
        {
            // 2 long overhead fluorescent light fixtures
            float lightY = roomHeight - 0.1f;
            float lightWidth = 0.12f;
            float lightLength = roomLength * 0.7f;

            for (int i = 0; i < 2; i++)
            {
                float xPos = (i == 0) ? -roomWidth / 4f : roomWidth / 4f;

                GameObject lightFixture = new GameObject($"FluorescentLight_{i}");
                lightFixture.transform.SetParent(transform, false);
                lightFixture.transform.localPosition = new Vector3(xPos, lightY, 0f);

                // Light housing (thin cube)
                GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                housing.name = "Housing";
                housing.transform.SetParent(lightFixture.transform, false);
                housing.transform.localPosition = Vector3.zero;
                housing.transform.localScale = new Vector3(lightWidth, 0.03f, lightLength);
                Renderer housingRend = housing.GetComponent<Renderer>();
                Material housingMat = new Material(Shader.Find("Standard"));
                housingMat.color = new Color(0.7f, 0.7f, 0.7f); // Light gray housing
                housingRend.material = housingMat;
                Collider housingCol = housing.GetComponent<Collider>();
                if (housingCol != null) Destroy(housingCol);

                // Light tube (emissive)
                GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tube.name = "LightTube";
                tube.transform.SetParent(lightFixture.transform, false);
                tube.transform.localPosition = new Vector3(0f, -0.02f, 0f);
                tube.transform.localScale = new Vector3(lightWidth * 0.8f, 0.015f, lightLength * 0.95f);
                Renderer tubeRend = tube.GetComponent<Renderer>();
                Material tubeMat = new Material(Shader.Find("Standard"));
                tubeMat.color = new Color(0.95f, 0.97f, 1f);
                tubeMat.EnableKeyword("_EMISSION");
                tubeMat.SetColor("_EmissionColor", new Color(0.95f, 0.97f, 1f) * 2f);
                tubeRend.material = tubeMat;
                Collider tubeCol = tube.GetComponent<Collider>();
                if (tubeCol != null) Destroy(tubeCol);

                // Actual Unity light component
                GameObject lightObj = new GameObject("PointLight");
                lightObj.transform.SetParent(lightFixture.transform, false);
                lightObj.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                Light pointLight = lightObj.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = 8f;
                pointLight.intensity = 1.2f;
                pointLight.color = new Color(0.95f, 0.95f, 1f); // Slightly cool white

                fluorescents.Add(lightFixture);
            }
        }

        // --- Public API ---

        /// <summary>
        /// Returns the Workbench component for tool storage interaction.
        /// </summary>
        public Workbench GetWorkbench()
        {
            return workbench;
        }

        /// <summary>
        /// Returns the job board transform for mounting the WorkOrderBoardUI.
        /// </summary>
        public Transform GetJobBoardMount()
        {
            if (jobBoard == null) return null;
            Transform mount = jobBoard.transform.Find("WorkOrderBoardUI_Mount");
            return mount != null ? mount : jobBoard.transform;
        }

        /// <summary>
        /// Returns the van/truck travel trigger transform.
        /// </summary>
        public Transform GetTravelTrigger()
        {
            if (van == null) return null;
            Transform trigger = van.transform.Find("TravelTrigger");
            return trigger;
        }
    }
}
