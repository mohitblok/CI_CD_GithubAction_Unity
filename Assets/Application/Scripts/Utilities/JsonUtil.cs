using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Unity.VisualScripting.FullSerializer;
using Utilities;

/// <summary>
/// json util class used to read and write json files
/// </summary>
public static class JsonUtil
{
    private static Queue<Action> ParseQueue = new Queue<Action>();
    private static int activeRequests = 0;
    
    //TODO this need to be async underline system at risk of data overlap
    private const int MAX_ACTIVE_REQUESTS = 1; //_uncomputedAotCompilations static list prevents async running

    /// <summary> The ReadFromFile function reads a file and returns the contents as a string.</summary>
    /// <param name="filePath"> The path to the file.</param>
    public static T ReadFromFile<T>(string filePath) where T : class
    {
        return ReadFromText<T>(ReadFile(filePath));
    }

    private static string ReadFile(string filePath) => IO.ReadFromFile(AppendExtensionIfNeeded(filePath));

    /// <summary> The ReadFromText function reads a JSON string and returns an object of type T.</summary>
    /// <param name="jsonText"> The json text to deserialise</param>
    /// <returns> An object of type t.</returns>
    public static T ReadFromText<T>(string jsonText)
    {
        if (false == string.IsNullOrEmpty(jsonText))
        {
            return (T)Deserialise(jsonText, default(T), typeof(T));
        }

        return default(T);
    }

    /// <summary> The AsyncReadFile function reads a file from the internet asynchronously and returns it to a callback function.</summary>
    /// <param name="filePath"> The path to the file.</param>
    /// <param name="callback"> results of type t</param>
    /// <returns> An object of type t</returns>
    public static IEnumerator AsyncReadFile<T>(string filePath, Action<T> callback) where T : class
    {
        UnityWebRequest www = new UnityWebRequest(filePath);
        www.downloadHandler = new DownloadHandlerBuffer();

        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.InProgress:
                    break;
                case UnityWebRequest.Result.Success:
                    AsyncReadFromText<T>(request.downloadHandler.text, callback);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(request.error);
                    callback.Invoke(default);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary> The AsyncReadFromText function is a helper function that allows you to read from a JSON string asynchronously.
    /// It takes in two parameters: jsonText and callback. The first parameter, jsonText, is the text of the JSON string that you want to parse into an object. The second parameter, callback, is a delegate (a pointer) to another function which will be called when AsyncReadFromText has finished parsing your JSON text.</summary>
    /// <param name="jsonText"> The json text to deserialise</param>
    /// <param name="callback"> results of type t</param>
    /// <returns> An object of type t</returns>
    public static void AsyncReadFromText<T>(string jsonText, Action<T> callback) where T : class
    {
        if (activeRequests >= MAX_ACTIVE_REQUESTS)
        {
            ParseQueue.Enqueue(() => AsyncReadFromText<T>(jsonText, callback));
        }
        else
        {
            activeRequests++;

            Task.Run(() =>
            {
                try
                {
                    var parse = (T)Deserialise(jsonText, default(T), typeof(T));
                    MainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        activeRequests--;

                        if (ParseQueue.Count > 0 && activeRequests < MAX_ACTIVE_REQUESTS)
                        {
                            ParseQueue.Dequeue()?.Invoke();
                        }

                        callback?.Invoke(parse);
                    });
                }
                catch (Exception e)
                {
                    MainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        activeRequests--;
                        if (ParseQueue.Count > 0 && activeRequests < MAX_ACTIVE_REQUESTS)
                        {
                            ParseQueue.Dequeue()?.Invoke();
                        }

                        Debug.LogError(e);
                        callback?.Invoke(null);
                    });
                }
            });
        }
    }

    /// <summary> The WriteToFile function writes the given data to a file at the given path.
    /// If no data is provided, it will not write anything and return.
    /// If an empty string is provided as a path, it will debug exception.</summary>
    /// <param name="data"> The data to write to the file.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <param name="prettyPrint"> if true the file fill be easier to read</param>
    public static void WriteToFile<T>(T data, string filePath, bool prettyPrint = true)
    {
        filePath = AppendExtensionIfNeeded(filePath);
        string jsonText = WriteToText(data, prettyPrint);

        if (data != null && jsonText == null)
        {
            Debug.LogError("None-null data producted an empty output. Aborting file-write to avoid data-loss!");
            return;
        }

        try
        {
            IO.ForceFolder(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, jsonText);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write text to path \"{filePath}\"\n\"{e.Message}\" text: {jsonText}");
        }
    }

    /// <summary> The WriteToText function serialises the given data to a JSON string.</summary>
    /// <param name="data"> The data to serialise.</param>
    /// <param name="bool"> if true, the json will be formatted with indentation and line breaks.</param>
    /// <returns> A string containing the json serialised data.</returns>
    public static string WriteToText<T>(T data, bool prettyPrint)
    {
        try
        {
            return SerialiseData(data, prettyPrint);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to serialise JSON to type {{{typeof(T).Name}}}.\n\"{e.Message}\" from data:\n{data.ToString()}");
        }

        return null;
    }

    private static object Deserialise(string jsonText, object obj, Type type)
    {
        if (obj == null)
        {
            obj = null;
        }

        var serialiser = new fsSerializer();
        var jsonData = fsJsonParser.Parse(jsonText);
        var result = serialiser.TryDeserialize(jsonData, type, ref obj);
        //result.AssertSuccessWithoutWarnings(); //Disabled to allow the use of fragmented data by Andy Speakman
        return obj;
    }

    private static string SerialiseData<T>(T data, bool prettyPrint)
    {
        fsData fsData;
        var serialiser = new fsSerializer();
        var result = serialiser.TrySerialize(data, out fsData);
        result.AssertSuccessWithoutWarnings();

        if (prettyPrint == true)
        {
            return fsJsonPrinter.PrettyJson(fsData);
        }
        else
        {
            return fsJsonPrinter.CompressedJson(fsData);
        }
    }

    private static string AppendExtensionIfNeeded(string filePath)
    {
        string sanePath = filePath.SanitiseSlashes();

        if (Path.GetExtension(sanePath) != ".json")
            return sanePath + ".json";

        return sanePath;
    }

    /// <summary>
    /// A custom converter for the ModuleData class.
    /// </summary>
    public class ModuleDataConverter : fsDirectConverter {
       
        /// <summary>
        /// The type of the model.
        /// </summary>
        /// <returns> The type of the model.</returns>
        public override Type ModelType { get { return typeof(List<BaseModuleData>); } }

        /// <summary> The CreateInstance function is used to create an instance of the class that this fsData object represents. This function is called by the serializer when it needs to create a new object.</summary>
        /// <param name="data"> The data.</param>
        /// <param name="storageType"> type of storage</param>
        public override object CreateInstance(fsData data, Type storageType) {
            return new List<BaseModuleData>();
        }

        /// <summary> The TrySerialize function is used to convert a JSON string into an object of the specified type.
        /// This function is called by the serializer when it needs to convert a JSON string into an object of the specified type.
        /// </summary>
        /// <param name="instance"> The instance to deserialize.</param>
        /// <param name="out"> Where to store the deserialized object.</param>
        /// <param name="Type"> /// the type of the instance to deserialize.</param>
        /// <returns> A fsresult object.</returns>
        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = new fsData(JsonUtil.WriteToText((List<BaseModuleData>)instance,false));
            return fsResult.Success;
        }

        /// <summary>
        /// deserializes the data into the instance
        /// </summary>
        /// <param name="data"> data to deserialize</param>
        /// <param name="instance">object deserialized to</param>
        /// <param name="storageType"> type of storage</param>
        /// <returns></returns>
        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsString == false) return fsResult.Fail("Expected string in " + data);
            var s = data.AsString;
            if (string.IsNullOrEmpty(s))
            {
                instance = new List<BaseModuleData>();
                return fsResult.Success;
            }

            try
            {
                instance = JsonUtil.ReadFromText<List<BaseModuleData>>(s);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                instance = new List<BaseModuleData>();
            }
            return fsResult.Success;
        }
    }
}