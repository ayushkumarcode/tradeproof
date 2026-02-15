using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class Quest2BuildScript
{
    [MenuItem("Build/Configure for Quest 2")]
    public static void ConfigureForQuest2()
    {
        Debug.Log("Configuring project for Meta Quest 2...");

        // Set Android platform
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Player Settings for Android
        PlayerSettings.companyName = "TradeProof";
        PlayerSettings.productName = "TradeProof VR";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.tradeproof.vr");

        // Android specific settings — Quest 2 requires API 29+ (Android 10)
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Graphics settings — Linear color space required for Quest
        PlayerSettings.colorSpace = ColorSpace.Linear;
        // OpenGLES3 primary for Quest 2 compatibility, Vulkan as fallback
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] {
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
            UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        });

        // Texture compression — ASTC is optimal for Quest
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

        // VR settings
        PlayerSettings.virtualRealitySupported = true;

        Debug.Log("Quest 2 configuration complete!");
        Debug.Log("Next step: Build > Build and Deploy to Quest 2");
    }

    [MenuItem("Build/Build and Deploy to Quest 2")]
    public static void BuildAndDeployToQuest2()
    {
        Debug.Log("Building APK for Meta Quest 2...");

        // Configure first
        ConfigureForQuest2();

        // Build path
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        string apkPath = Path.Combine(buildPath, "TradeProofVR.apk");

        // Build settings
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/TrainingScene.unity" };
        buildPlayerOptions.locationPathName = apkPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.AutoRunPlayer; // Auto-deploy to connected device

        // Build
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            Debug.Log("APK location: " + apkPath);
            Debug.Log("Deploying to Quest 2...");
        }
        else
        {
            Debug.LogError("Build failed");
        }
    }
}
