using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Sets up the TrainingScene with all required VR objects and configures XR Management.
/// Run via: Unity -executeMethod VRSceneSetup.SetupEverything
/// </summary>
public class VRSceneSetup
{
    [MenuItem("Build/Setup VR Scene")]
    public static void SetupEverything()
    {
        Debug.Log("[VRSceneSetup] Starting full VR setup...");

        ConfigureURPPipeline();
        ConfigureXRManagement();
        SetupScene();

        Debug.Log("[VRSceneSetup] Setup complete!");
    }

    [MenuItem("Build/Fix URP Pipeline")]
    static void ConfigureURPPipeline()
    {
        Debug.Log("[VRSceneSetup] Configuring URP Pipeline...");

        // Find UniversalRenderPipelineAsset type
        System.Type urpAssetType = null;
        System.Type rendererDataType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (urpAssetType == null)
                urpAssetType = assembly.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset");
            if (rendererDataType == null)
                rendererDataType = assembly.GetType("UnityEngine.Rendering.Universal.UniversalRendererData");
            if (urpAssetType != null && rendererDataType != null) break;
        }

        if (urpAssetType == null)
        {
            Debug.LogError("[VRSceneSetup] UniversalRenderPipelineAsset type not found!");
            return;
        }

        // Check if pipeline asset already exists
        string pipelinePath = "Assets/Settings";
        if (!AssetDatabase.IsValidFolder(pipelinePath))
            AssetDatabase.CreateFolder("Assets", "Settings");

        // Create renderer data first
        string rendererPath = "Assets/Settings/Quest2_Renderer.asset";
        ScriptableObject rendererData = null;
        if (rendererDataType != null)
        {
            rendererData = ScriptableObject.CreateInstance(rendererDataType);
            AssetDatabase.CreateAsset(rendererData, rendererPath);
            Debug.Log("[VRSceneSetup] Created URP Renderer Data");
        }

        // Create URP pipeline asset using the Create method
        string urpAssetPath = "Assets/Settings/Quest2_URPAsset.asset";

        // Use UniversalRenderPipelineAsset.Create() if available
        var createMethod = urpAssetType.GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        ScriptableObject urpAsset = null;

        if (createMethod != null)
        {
            try
            {
                if (rendererData != null)
                    urpAsset = createMethod.Invoke(null, new object[] { rendererData }) as ScriptableObject;
                else
                    urpAsset = createMethod.Invoke(null, null) as ScriptableObject;
            }
            catch
            {
                // Try without parameters
                try { urpAsset = createMethod.Invoke(null, new object[0]) as ScriptableObject; } catch { }
            }
        }

        if (urpAsset == null)
        {
            // Fallback: create instance directly
            urpAsset = ScriptableObject.CreateInstance(urpAssetType);
        }

        if (urpAsset != null)
        {
            AssetDatabase.CreateAsset(urpAsset, urpAssetPath);
            Debug.Log("[VRSceneSetup] Created URP Pipeline Asset");

            // Assign to Graphics Settings
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = urpAsset as UnityEngine.Rendering.RenderPipelineAsset;
            Debug.Log("[VRSceneSetup] Assigned URP to Graphics Settings");

            // Also assign to current quality level
            QualitySettings.renderPipeline = urpAsset as UnityEngine.Rendering.RenderPipelineAsset;
            Debug.Log("[VRSceneSetup] Assigned URP to Quality Settings");

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(urpAsset);
        }
        else
        {
            Debug.LogError("[VRSceneSetup] Failed to create URP Pipeline Asset!");
        }
    }

    static void ConfigureXRManagement()
    {
        Debug.Log("[VRSceneSetup] Configuring XR Management...");

        // Create XR settings directories
        string xrSettingsPath = "Assets/XR";
        if (!AssetDatabase.IsValidFolder(xrSettingsPath))
            AssetDatabase.CreateFolder("Assets", "XR");

        // Use XR Management API to configure Oculus loader
        // We need to create the settings assets programmatically
        var xrGeneralSettings = ScriptableObject.CreateInstance<UnityEngine.XR.Management.XRGeneralSettings>();
        var managerSettings = ScriptableObject.CreateInstance<UnityEngine.XR.Management.XRManagerSettings>();

        // Try to create Oculus loader
        var loaders = new System.Collections.Generic.List<UnityEngine.XR.Management.XRLoader>();

        // Find OculusLoader type
        System.Type oculusLoaderType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            oculusLoaderType = assembly.GetType("Unity.XR.Oculus.OculusLoader");
            if (oculusLoaderType != null) break;
        }

        if (oculusLoaderType != null)
        {
            var loader = ScriptableObject.CreateInstance(oculusLoaderType) as UnityEngine.XR.Management.XRLoader;
            if (loader != null)
            {
                AssetDatabase.CreateAsset(loader, "Assets/XR/OculusLoader.asset");
                loaders.Add(loader);
                Debug.Log("[VRSceneSetup] Created OculusLoader");
            }
        }
        else
        {
            Debug.LogWarning("[VRSceneSetup] OculusLoader type not found! XR may not work.");
        }

        // Configure manager
        var serializedManager = new SerializedObject(managerSettings);
        var loadersProp = serializedManager.FindProperty("m_Loaders");
        // Use reflection to set loaders since the API is protected
        var loadersField = typeof(UnityEngine.XR.Management.XRManagerSettings).GetField("m_Loaders",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (loadersField != null)
        {
            loadersField.SetValue(managerSettings, loaders);
        }
        var autoInitField = typeof(UnityEngine.XR.Management.XRManagerSettings).GetField("m_AutomaticLoading",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (autoInitField != null)
        {
            autoInitField.SetValue(managerSettings, true);
        }
        var autoRunField = typeof(UnityEngine.XR.Management.XRManagerSettings).GetField("m_AutomaticRunning",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (autoRunField != null)
        {
            autoRunField.SetValue(managerSettings, true);
        }

        AssetDatabase.CreateAsset(managerSettings, "Assets/XR/XRManagerSettings.asset");

        xrGeneralSettings.Manager = managerSettings;
        AssetDatabase.CreateAsset(xrGeneralSettings, "Assets/XR/XRGeneralSettings.asset");

        // Now we need to register these with XR General Settings Per Build Target
        // This is stored in EditorBuildSettings
        var xrSettingsPerBuildTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();

        // Use reflection to set the settings map
        var settingsMapField = typeof(XRGeneralSettingsPerBuildTarget).GetField("m_Settings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (settingsMapField != null)
        {
            var settingsMap = new System.Collections.Generic.Dictionary<BuildTargetGroup, UnityEngine.XR.Management.XRGeneralSettings>();
            settingsMap[BuildTargetGroup.Android] = xrGeneralSettings;
            settingsMapField.SetValue(xrSettingsPerBuildTarget, settingsMap);
        }

        AssetDatabase.CreateAsset(xrSettingsPerBuildTarget, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");

        // Register with EditorBuildSettings
        EditorBuildSettings.TryGetConfigObject(UnityEngine.XR.Management.XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget _existing);
        EditorBuildSettings.AddConfigObject(UnityEngine.XR.Management.XRGeneralSettings.k_SettingsKey, xrSettingsPerBuildTarget, true);

        AssetDatabase.SaveAssets();
        Debug.Log("[VRSceneSetup] XR Management configured for Android/Oculus");
    }

    static void SetupScene()
    {
        Debug.Log("[VRSceneSetup] Setting up TrainingScene...");

        string scenePath = "Assets/Scenes/TrainingScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Clear existing objects
        foreach (var go in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(go);
        }

        // --- OVRCameraRig ---
        GameObject cameraRig = new GameObject("OVRCameraRig");
        SceneManager.MoveGameObjectToScene(cameraRig, scene);

        // Add OVRCameraRig component
        System.Type ovrCameraRigType = FindType("OVRCameraRig");
        if (ovrCameraRigType != null)
        {
            cameraRig.AddComponent(ovrCameraRigType);
        }

        // Add OVRManager component
        System.Type ovrManagerType = FindType("OVRManager");
        if (ovrManagerType != null)
        {
            var ovrManager = cameraRig.AddComponent(ovrManagerType);
            // Set tracking origin to floor level via reflection
            var trackingOriginField = ovrManagerType.GetField("trackingOriginType");
            if (trackingOriginField != null)
            {
                // OVRManager.TrackingOrigin.FloorLevel = 1
                trackingOriginField.SetValue(ovrManager, 1);
            }
            // Enable hand tracking
            var handTrackingSupportProp = ovrManagerType.GetProperty("HandTrackingSupport");
            if (handTrackingSupportProp == null)
            {
                var handTrackingField = ovrManagerType.GetField("handTrackingSupport");
                if (handTrackingField != null)
                {
                    // HandsOnly = 1, ControllersAndHands = 2
                    handTrackingField.SetValue(ovrManager, 2);
                }
            }
        }

        // Create tracking space hierarchy
        GameObject trackingSpace = new GameObject("TrackingSpace");
        trackingSpace.transform.SetParent(cameraRig.transform, false);

        // Eye anchors
        GameObject leftEye = new GameObject("LeftEyeAnchor");
        leftEye.transform.SetParent(trackingSpace.transform, false);
        Camera leftCam = leftEye.AddComponent<Camera>();
        leftCam.enabled = false; // Managed by OVRCameraRig

        GameObject centerEye = new GameObject("CenterEyeAnchor");
        centerEye.transform.SetParent(trackingSpace.transform, false);
        centerEye.tag = "MainCamera";
        Camera centerCam = centerEye.AddComponent<Camera>();
        centerCam.nearClipPlane = 0.01f;
        centerCam.farClipPlane = 1000f;
        centerEye.AddComponent<AudioListener>();

        GameObject rightEye = new GameObject("RightEyeAnchor");
        rightEye.transform.SetParent(trackingSpace.transform, false);
        Camera rightCam = rightEye.AddComponent<Camera>();
        rightCam.enabled = false;

        // Hand anchors
        GameObject leftHand = new GameObject("LeftHandAnchor");
        leftHand.transform.SetParent(trackingSpace.transform, false);
        GameObject leftController = new GameObject("LeftControllerAnchor");
        leftController.transform.SetParent(leftHand.transform, false);

        GameObject rightHand = new GameObject("RightHandAnchor");
        rightHand.transform.SetParent(trackingSpace.transform, false);
        GameObject rightController = new GameObject("RightControllerAnchor");
        rightController.transform.SetParent(rightHand.transform, false);

        // Tracker anchor
        GameObject tracker = new GameObject("TrackerAnchor");
        tracker.transform.SetParent(trackingSpace.transform, false);

        // Add OVRHand and OVRSkeleton to hand anchors
        System.Type ovrHandType = FindType("OVRHand");
        System.Type ovrSkeletonType = FindType("OVRSkeleton");

        if (ovrHandType != null)
        {
            var leftOvrHand = leftHand.AddComponent(ovrHandType);
            var rightOvrHand = rightHand.AddComponent(ovrHandType);
            // Set hand types via reflection: HandLeft=0, HandRight=1
            var handTypeField = ovrHandType.GetField("HandType");
            if (handTypeField != null)
            {
                handTypeField.SetValue(leftOvrHand, 0); // HandLeft
                handTypeField.SetValue(rightOvrHand, 1); // HandRight
            }
        }
        if (ovrSkeletonType != null)
        {
            leftHand.AddComponent(ovrSkeletonType);
            rightHand.AddComponent(ovrSkeletonType);
        }

        // Add HandInteraction to hand anchors
        System.Type handInteractionType = FindType("TradeProof.Interaction.HandInteraction");
        if (handInteractionType != null)
        {
            leftHand.AddComponent(handInteractionType);
            rightHand.AddComponent(handInteractionType);
        }

        // --- Directional Light ---
        GameObject light = new GameObject("Directional Light");
        SceneManager.MoveGameObjectToScene(light, scene);
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.color = new Color(1f, 0.957f, 0.839f, 1f);
        lightComp.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.transform.position = new Vector3(0, 3, 0);

        // --- Floor (so you can see something) ---
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        SceneManager.MoveGameObjectToScene(floor, scene);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(5f, 1f, 5f);
        var floorRenderer = floor.GetComponent<MeshRenderer>();
        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.2f, 0.2f, 0.25f);
        floorRenderer.material = floorMat;

        // --- Workshop walls ---
        CreateWall(scene, "BackWall", new Vector3(0, 1.5f, 5f), new Vector3(10f, 3f, 0.1f), new Color(0.3f, 0.3f, 0.35f));
        CreateWall(scene, "LeftWall", new Vector3(-5f, 1.5f, 0), new Vector3(0.1f, 3f, 10f), new Color(0.28f, 0.28f, 0.33f));
        CreateWall(scene, "RightWall", new Vector3(5f, 1.5f, 0), new Vector3(0.1f, 3f, 10f), new Color(0.28f, 0.28f, 0.33f));

        // --- Workbench ---
        GameObject workbench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        workbench.name = "Workbench";
        SceneManager.MoveGameObjectToScene(workbench, scene);
        workbench.transform.position = new Vector3(0, 0.45f, 1.5f);
        workbench.transform.localScale = new Vector3(2f, 0.9f, 0.8f);
        var benchRenderer = workbench.GetComponent<MeshRenderer>();
        Material benchMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        benchMat.color = new Color(0.4f, 0.3f, 0.2f);
        benchRenderer.material = benchMat;

        // --- Managers ---
        CreateManagerObject<TradeProof.Core.GameManager>(scene, "GameManager");
        CreateManagerObject<TradeProof.Core.TaskManager>(scene, "TaskManager");
        CreateManagerObject<TradeProof.Core.ScoreManager>(scene, "ScoreManager");
        CreateManagerObject<TradeProof.Core.AudioManager>(scene, "AudioManager");
        CreateManagerObject<TradeProof.Core.BadgeSystem>(scene, "BadgeSystem");

        // --- UI Objects ---
        GameObject mainMenuUI = CreateManagerObject<TradeProof.UI.MainMenuUI>(scene, "MainMenuUI");
        GameObject hudController = CreateManagerObject<TradeProof.UI.HUDController>(scene, "HUDController");
        GameObject taskSelectionUI = CreateManagerObject<TradeProof.UI.TaskSelectionUI>(scene, "TaskSelectionUI");
        GameObject resultsScreenUI = CreateManagerObject<TradeProof.UI.ResultsScreenUI>(scene, "ResultsScreenUI");
        GameObject badgeDisplayUI = CreateManagerObject<TradeProof.UI.BadgeDisplayUI>(scene, "BadgeDisplayUI");

        // --- Tool Belt ---
        System.Type toolBeltType = FindType("TradeProof.Interaction.ToolBelt");
        if (toolBeltType != null)
        {
            GameObject toolBelt = new GameObject("ToolBelt");
            SceneManager.MoveGameObjectToScene(toolBelt, scene);
            toolBelt.AddComponent(toolBeltType);
            toolBelt.transform.position = new Vector3(0, 0.8f, 0.3f);
        }

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log("[VRSceneSetup] Scene setup complete with VR objects");
    }

    static GameObject CreateManagerObject<T>(Scene scene, string name) where T : Component
    {
        GameObject obj = new GameObject(name);
        SceneManager.MoveGameObjectToScene(obj, scene);
        obj.AddComponent<T>();
        return obj;
    }

    static void CreateWall(Scene scene, string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        SceneManager.MoveGameObjectToScene(wall, scene);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        var renderer = wall.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        renderer.material = mat;
    }

    static System.Type FindType(string fullName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName);
            if (type != null) return type;
        }
        Debug.LogWarning($"[VRSceneSetup] Type not found: {fullName}");
        return null;
    }
}
