using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to add and remove modules and maintains a list of already loaded modules
/// </summary>
public class RootModule : MonoBehaviour
{
    private List<BaseModule> listOfModules = new List<BaseModule>();
    private Dictionary<Type, List<BaseModule>> moduleDict = new Dictionary<Type, List<BaseModule>>();
    private string guid;
    
    /// <summary>
    /// Adds modules to a list
    /// </summary>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    public void AddModules(List<BaseModuleData> data, Action callback)
    {
        var task = new TaskAction(data.Count, callback);

        for (int i = 0; i < data.Count; i++)
        {
            var moduleData = data[i];

            if (moduleData == null)
            {
                Debug.LogError($"Null module data on {guid}");
                task?.Increment();
                continue;
            }

            var module = ModuleFactory.AttachModule(gameObject, moduleData.GetType());

            if (module != null)
            {
                module.Init(moduleData, () =>
                {
                    var type = module.GetType();

                    if (!moduleDict.ContainsKey(type))
                    {
                        moduleDict.Add(type, new List<BaseModule>());
                    }
                    
                    moduleDict[type].Add(module);
                    listOfModules.Add(module);
                    task.Increment();
                });
            }
            else
            {
                Debug.LogError($"{moduleData.GetType()} is not in the factory!");
                task.Increment();
            }
        }
    }
    
    /// <summary>
    /// Removes a specified module from an object
    /// </summary>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    public void RemoveModule(BaseModuleData data, Action callback)
    {
        var type = ModuleFactory.GetModuleFromModuleData(data.GetType());
        var module = GetModule(type);

        if (module != null)
        {
            listOfModules.Remove(module);
            moduleDict.Remove(type);
            module.Deinit();
            callback?.Invoke();
        }
        else
        {
            Debug.LogError("Failed to remove module");
        }
    }
    
    private BaseModule GetModule(Type type)
    {
        if (moduleDict.ContainsKey(type))
        {
            var data = moduleDict[type];
            if (data.Count > 0)
            {
                return data[0];
            }
        }
    
        return null;
    }
}
