#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SurvivalMoba.EditorTools
{
    /// <summary>
    /// Headless Android build entry point for CI (GameCI / GitHub Actions).
    /// Generates the content + scenes, configures Android player settings and
    /// produces an installable development APK — no manual Unity interaction.
    ///
    /// Invoked via: -executeMethod SurvivalMoba.EditorTools.AndroidBuilder.PerformBuild
    /// </summary>
    public static class AndroidBuilder
    {
        private const string OutputDir = "Builds/Android";
        private const string ApkName = "ArcaneSurvivors.apk";

        private static readonly string[] Scenes =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GameScene.unity",
        };

        public static void PerformBuild()
        {
            try
            {
                Log("Generating content and scenes...");
                ContentBuilder.BuildContent();
                GameSceneBuilder.BuildScene();
                MainMenuBuilder.BuildMainMenu();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                ConfigurePlayerSettings();

                Directory.CreateDirectory(OutputDir);
                string outputPath = Path.Combine(OutputDir, ApkName);

                var options = new BuildPlayerOptions
                {
                    scenes = Scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    // Development build => Unity signs with the Android debug keystore,
                    // so the APK is installable without a custom keystore.
                    options = BuildOptions.Development,
                };

                Log($"Building APK to {outputPath} ...");
                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Log($"BUILD SUCCEEDED: {summary.totalSize / (1024 * 1024)} MB at {outputPath}");
                    EditorApplication.Exit(0);
                }
                else
                {
                    LogError($"BUILD FAILED: result={summary.result}, errors={summary.totalErrors}");
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception e)
            {
                LogError("BUILD EXCEPTION: " + e);
                EditorApplication.Exit(1);
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "OriginalGames";
            PlayerSettings.productName = "Arcane Survivors";

            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.Android, "com.originalgames.arcanesurvivors");

            // Fast, broadly-installable test config: Mono + ARMv7, debug-signed.
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.useCustomKeystore = false;

            // Landscape HUD layout.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.buildAppBundle = false; // APK, not AAB
        }

        private static void Log(string msg) => Debug.Log($"[AndroidBuilder] {msg}");
        private static void LogError(string msg) => Debug.LogError($"[AndroidBuilder] {msg}");
    }
}
#endif
