using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data factory which attaches modules to a specified item
/// </summary>
public static class ModuleFactory
{
    private static Dictionary<Type, IAttacher> DataFactory = new Dictionary<Type, IAttacher>()
    {
        { typeof(OffsetModuleData), new ModuleAttacher<OffsetModule>() },
        { typeof(TextureModuleData), new ModuleAttacher<TextureModule>() },
        { typeof(AudioModuleData), new ModuleAttacher<AudioModule>() },
        { typeof(ModelModuleData), new ModuleAttacher<ModelModule>() },
        { typeof(VideoModuleData), new ModuleAttacher<VideoModule>() },
        { typeof(ScaleModuleData), new ModuleAttacher<ScaleModule>() },
        { typeof(SpawnPointModuleData), new ModuleAttacher<SpawnPointModule>() },
        { typeof(PriceTrackerModuleData), new ModuleAttacher<PriceTrackerModule>() },
    };
    
    /// <summary>
    /// Get keys from data factory dict and returns a list
    /// </summary>
    /// <returns>the Types</returns>
    public static List<Type> GetTypesList()
    {
        List<Type> types = new List<Type>();

        foreach (var type in DataFactory.Keys)
        {
            types.Add(type);
        }

        return types;
    }
    
    private interface IAttacher
    {
        public BaseModule Attach(GameObject holder);
    }

    private class ModuleAttacher<T> : IAttacher where T : BaseModule
    {
        public BaseModule Attach(GameObject holder)
        {
            T module;

            try
            {
                module = holder.AddComponent<T>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError($"Unable to attach module: {typeof(T)}");
                return null;
            }

            return module;
        }
    }

    /// <summary>
    /// Attaches dataType to the specified holder object
    /// </summary>
    /// <param name="holder"></param>
    /// <param name="dataType"></param>
    /// <returns>Of type BaseModule</returns>
    public static BaseModule AttachModule(GameObject holder, Type dataType)
    {
        BaseModule newModule = null;

        if (dataType != null && DataFactory.ContainsKey(dataType))
        {
            newModule = DataFactory[dataType].Attach(holder);
        }
        else
        {
            Debug.LogError($"Factory does not contain a module of type {dataType.Name}", holder);
        }

        return newModule;
    }

    /// <summary>
    /// Get current data from the moduleData from the data factory
    /// </summary>
    /// <param name="moduleData"></param>
    /// <returns>the Type</returns>
    public static Type GetModuleFromModuleData(Type moduleData)
    {
        if (DataFactory.TryGetValue(moduleData, out var moduleType))
        {
            return moduleType.GetType();
        }

        Debug.LogError($"No Type in DataFactory for Type = {moduleData}");
        return default;
    }
}