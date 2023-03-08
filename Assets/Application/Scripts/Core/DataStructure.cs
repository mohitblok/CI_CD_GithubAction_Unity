using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// The media type enum
/// </summary>
public enum Media
{
    /// <summary>
    /// None
    /// </summary>
    None,
    /// <summary>
    /// The scene media
    /// </summary>
    Scene,
    /// <summary>
    /// The model media
    /// </summary>
    Model,
    /// <summary>
    /// The video media
    /// </summary>
    Video,
    /// <summary>
    /// The audio media
    /// </summary>
    Audio,
    /// <summary>
    /// The image media
    /// </summary>
    Image,
}

/// <summary>
/// Data for correctly acquiring the asset (what to get and the hash)
/// </summary>
public class AssetData
{
    /// <summary>
    /// The guid or url of the media
    /// </summary>
    public string downloadData;
    /// <summary>
    /// The dictionary of hashes per platform
    /// </summary>
    public Dictionary<string, string> assetHashDict = new Dictionary<string, string>();
}

/// <summary>
/// Base type for all Content Data
/// </summary>
public class ContentDataEntry
{
    /// <summary>
    /// The guid
    /// </summary>
    public string guid;
    /// <summary>
    /// The name
    /// </summary>
    public string name;
    /// <summary>
    /// The description
    /// </summary>
    public string description;
    /// <summary>
    /// Creation/last updated time
    /// </summary>
    public string creation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
    /// <summary>
    /// The unity version data was uploaded from
    /// </summary>
    public string unityVersion = Application.unityVersion;
    /// <summary>
    /// The expiration
    /// </summary>
    public string expiration;
}

/// <summary>
/// The media entry class, required data for one of the Media enums
/// </summary>
/// <seealso cref="ContentDataEntry"/>
public class MediaEntry : ContentDataEntry
{
    /// <summary>
    /// The none
    /// </summary>
    /// <seealso cref="Media"/>
    public Media media = Media.None;
    /// <summary>
    /// The asset data
    /// </summary>
    /// <seealso cref="AssetData"/>
    public AssetData assetData = new AssetData();
    /// <summary>
    /// The original source
    /// </summary>
    public string originalSource;
    /// <summary>
    /// The extension
    /// </summary>
    public string extension = ".unity3d";
}

/// <summary>
/// The template data entry class, this combines a source media entry with modules
/// </summary>
/// <seealso cref="ContentDataEntry"/>
[Template]
public class TemplateDataEntry : ContentDataEntry
{
    /// <summary>
    /// The source media entry entry guid
    /// </summary>
    /// <seealso cref="MediaEntry"/>
    public string sourceContentDataEntryGuid;
    /// <summary>
    /// The list of dependencies required
    /// </summary>
    public List<string> dependencyList = new List<string>();
    /// <summary>
    /// The base module data, when sent to the backend this is converted
    /// to a string via JsonUtil.ModuleDataConverter
    /// </summary>
    /// /// <seealso cref="JsonUtil.ModuleDataConverter"/>
    [fsProperty(Converter = typeof(JsonUtil.ModuleDataConverter))]
    public List<BaseModuleData> moduleData = new List<BaseModuleData>();
}

/// <summary>
/// The template attribute class, so we can tag the TemplateDataEntry class for module data list to string conversion
/// </summary>
/// <seealso cref="Attribute"/>
/// <seealso cref="TemplateDataEntry"/>
public class TemplateAttribute : Attribute
{
}

/// <summary>
/// The scene graph entry class is all the non-scene assets in an environment
/// </summary>
/// <seealso cref="ContentDataEntry"/>
public class SceneGraphEntry : ContentDataEntry
{
    /// <summary>
    /// The configured item entry list
    /// </summary>
    public List<ConfiguredItemEntry> sceneItems = new List<ConfiguredItemEntry>();
}

/// <summary>
/// The configured item entry class ties together a content (template or media entry)
/// with the in scene position and rotation
/// </summary>
public class ConfiguredItemEntry
{
    /// <summary>
    /// The content guid (template or media entry)
    /// </summary>
    public string contentGuid;
    /// <summary>
    /// The position
    /// </summary>
    public Vector3 position;
    /// <summary>
    /// The rotation
    /// </summary>
    public Quaternion rotation;
}

/// <summary>
/// The location data entry class contains a list of all the media entries that are a Media of scene for an environment
/// </summary>
/// <seealso cref="ContentDataEntry"/>
public class LocationDataEntry : ContentDataEntry
{
    /// <summary>
    /// The list of media entry guids (must be Media.Scene)
    /// </summary>
    public List<string> scenes = new List<string>();
}

/// <summary>
/// The environment data entry class defines the scenes and objects within those scenes that make a space
/// </summary>
/// <seealso cref="ContentDataEntry"/>
public class EnvironmentDataEntry : ContentDataEntry
{
    /// <summary>
    /// The scene graph guid
    /// </summary>
    /// <seealso cref="SceneGraphEntry"/>
    public string sceneGraph;
    /// <summary>
    /// The location guid
    /// </summary>
    /// <seealso cref="LocationDataEntry"/>
    public string location;
    /// <summary>
    /// The network settings for this enviornment
    /// </summary>
    public NetworkData networkData = new NetworkData();
}

/// <summary>
/// The network data class
/// </summary>
public class NetworkData
{
    /// <summary>
    /// The instancing type enum
    /// </summary>
    public enum InstancingType
    {
        /// <summary>
        /// None/Offline mode
        /// </summary>
        None, //offline mode
        /// <summary>
        /// Single room only
        /// </summary>
        Single, //One room only
        /// <summary>
        /// The multi instancing type
        /// </summary>
        Multi, //room instance
        /// <summary>
        /// Fixed number of room instance
        /// </summary>
        Fixed, // fixed number of room instance
    }

    /// <summary>
    /// The fixed instance class
    /// </summary>
    public class FixedInstance
    {
        /// <summary>
        /// The room names
        /// </summary>
        public List<string> roomNames = new List<string>();
    }

    /// <summary>
    /// The instancing type
    /// </summary>
    public InstancingType instancingType = InstancingType.Single;
    /// <summary>
    /// The player count
    /// </summary>
    public int playerCount = 50;
    /// <summary>
    /// The fixed instance
    /// </summary>
    public FixedInstance fixedInstance;
}
