using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Fusion;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using Assert = UnityEngine.Assertions.Assert;
using Debug = UnityEngine.Debug;

namespace CustomBuildPipeline
{
    /// <summary>
    /// The jenkins build class
    /// </summary>
    public static class JenkinsBuild
    {
        /// <summary>
        /// The builds folder
        /// </summary>
        public const string BUILDS_FOLDER = "Builds";
        
        private static void Build(BuildSchemaData data)
        {
            if (EditorUserBuildSettings.activeBuildTarget != data.buildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(data.buildTarget);
            }

            BuildPlayer(data);
        }
        
        [MenuItem("Build/VR/Windows", false, 0)]
        private static void BuildWin64()
        {
            Build(PreBuiltSchemas.Windows64);
        }
        
        [MenuItem("Build/VR/Android", false, 0)]
        private static void BuildAndroidVR()
        {
            Build(PreBuiltSchemas.AndroidVR);
        }

        /// <summary>
        /// Builds the from command for Jenkins build machine
        /// </summary>
        public static void BuildFromCommand()
        {
            var schema = SchemaFromArgs(Environment.GetCommandLineArgs());

            BuildPlayer(schema);
        }
        
        private static BuildSchemaData SchemaFromArgs(string[] args)
        {
            var schema = new BuildSchemaData();
            var cmdArgs = args;

            schema.buildOptions = BuildOptions.None;
            schema.buildTarget = BuildTarget.NoTarget;
            schema.clientType = ClientType.User;

            var audioPlugin = string.Empty;
            var isVRBuild = false;

            bool containsVersionNumber = false;
            for (int i = 0; i < cmdArgs.Length; i++)
            {
                switch (cmdArgs[i])
                {
                    case "-buildPath":
                        Assert.IsTrue(i + 1 < cmdArgs.Length, "Expected argument after -buildPath");
                        schema.buildPath = cmdArgs[++i];
                        break;
                    case "-buildPlatform":
                        Assert.IsTrue(i + 1 < cmdArgs.Length, "Expected argument after -buildPlatform");
                        schema.buildTarget = StringToBuildTarget(cmdArgs[++i]);
                        break;
                    case "-buildOption":
                        Assert.IsTrue(i + 1 < cmdArgs.Length, "Expected argument after -buildOption");
                        schema.buildOptions |= StringToBuildOption(cmdArgs[++i]);
                        break;
                    case "-clientType":
                        Assert.IsTrue(i + 1 < cmdArgs.Length, "Expected argument after -clientType");
                        schema.clientType = ClientTypeToString(cmdArgs[++i]);
                        break;
                    case "-debug":
                        schema.buildOptions |= BuildOptions.Development;
                        break;
                    case "-oculus":
                        audioPlugin = "OculusSpatializer";
                        isVRBuild = true;
                        break;
                }
            }
            
            AudioSettings.SetSpatializerPluginName(audioPlugin);

            if (isVRBuild)
            {
                AssignXRDeviceLoader("Unity.XR.Oculus.OculusLoader");
            }
            else
            {
                RemoveXRDeviceLoader("Unity.XR.Oculus.OculusLoader");
            }

            return schema;
        }
        
        private static BuildTarget StringToBuildTarget(string platform)
        {
            switch (platform)
            {
                case "iOS":
                    return BuildTarget.iOS;
                case "Android":
                    return BuildTarget.Android;
                case "Windows":
                case "Windows64":
                    return BuildTarget.StandaloneWindows64;
                case "macOS":
                case "OSX":
                    return BuildTarget.StandaloneOSX;
                case "WebGL":
                    return BuildTarget.WebGL;
                default:
                    if (Enum.TryParse(platform, out BuildTarget target))
                        return target;
                    throw new ArgumentException($"Unknown build target {platform}");
            }
        }
        
        private static BuildOptions StringToBuildOption(string str)
        {
            switch (str)
            {
                default:
                    if (Enum.TryParse(str, out BuildOptions option))
                        return option;
                    throw new ArgumentException($"Unknown build option {str}");
            }
        }
        
        private static ClientType ClientTypeToString(string str)
        {
            switch (str)
            {
                default:
                    if (Enum.TryParse(str, out ClientType type))
                        return type;
                    throw new ArgumentException($"Unknown build option {str}");
            }
        }
        
        private static string[] GetScenesToBuild()
        {
            return EditorBuildSettings.scenes.Select(x => x.path).ToArray();
        }
        
        private static void BuildPlayer(BuildSchemaData schemaData)
        {
            var stopwatch = Stopwatch.StartNew();

            PlayerSettings.SplashScreen.show = false;

            var scenes = GetScenesToBuild();

            CheckBuildsFolder();

            BuildPipeline.BuildPlayer(scenes, schemaData.buildPath, schemaData.buildTarget, schemaData.buildOptions);

            Debug.Log(
                $"Build completed after {stopwatch.Elapsed:hh\\:mm\\:ss}\nBuild location: {schemaData.buildPath}");

            if (Environment.CommandLine.Contains("-csv"))
                EditorApplication.Exit(0);
        }
        
        private static void CheckBuildsFolder()
        {
            if (!Directory.Exists($"{BUILDS_FOLDER}/Windows"))
            {
                Directory.CreateDirectory($"{BUILDS_FOLDER}/Windows");
            }
        }
        
        private static void RemoveXRDeviceLoader(string loading)
        {
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            XRPackageMetadataStore.RemoveLoader(settings.Manager, loading, BuildTargetGroup.Android);
        }

        private static void AssignXRDeviceLoader(string loading)
        {
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            XRPackageMetadataStore.AssignLoader(settings.Manager, loading, BuildTargetGroup.Android);
        }
    }
}