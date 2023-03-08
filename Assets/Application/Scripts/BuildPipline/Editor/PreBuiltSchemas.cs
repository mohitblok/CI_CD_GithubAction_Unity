using UnityEditor;

namespace CustomBuildPipeline
{
    /// <summary>
    /// This class defines a static class 'PreBuiltSchemas' to provide details for builds.
    /// The 'PreBuiltSchemas' and 'BuildSchemaData' classes are used for managing various build configurations.
    /// </summary>
    public static class PreBuiltSchemas
    {
        /// <summary>
        /// Standard schema for Windows 64-bit builds, this code makes it easier to automate the build process and maintain consistency across different builds.
        /// </summary>
        public static BuildSchemaData Windows64 { get; } = new BuildSchemaData
        {
            buildTarget = BuildTarget.StandaloneWindows64,
            clientType = ClientType.User,
            buildOptions = BuildOptions.Development,
            buildPath = $"{JenkinsBuild.BUILDS_FOLDER}/Windows/{PlayerSettings.productName}.exe",
        };

        /// <summary>
        /// Standard schema for AndroidVR builds, this code makes it easier to automate the build process and maintain consistency across different builds.
        /// </summary>
        public static BuildSchemaData AndroidVR { get; } = new BuildSchemaData()
        {
            buildTarget = BuildTarget.Android,
            clientType = ClientType.User,
            buildOptions = BuildOptions.Development,
            buildPath = $"{JenkinsBuild.BUILDS_FOLDER}/Android/{PlayerSettings.productName}.apk",
        };
    }
}