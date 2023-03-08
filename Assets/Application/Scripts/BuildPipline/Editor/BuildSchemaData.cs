using UnityEditor;

namespace CustomBuildPipeline
{
    
    /// <summary>
    /// The build schema data class data which is used to create builds
    /// </summary>
    public class BuildSchemaData
    {
        /// <summary>
        /// The build path
        /// </summary>
        public string buildPath;
        /// <summary>
        /// The client type for who the app is built
        /// </summary>
        public ClientType clientType;
        /// <summary>
        /// The build target
        /// </summary>
        public BuildTarget buildTarget;
        /// <summary>
        /// The build options
        /// </summary>
        public BuildOptions buildOptions;
    }
}