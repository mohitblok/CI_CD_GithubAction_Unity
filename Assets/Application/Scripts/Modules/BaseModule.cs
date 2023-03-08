using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This abstract class defines a standardized way to initialize and deinitialize.
/// It has a public field to store module-specific data and two abstract methods to initialize and deinitialize the module.
/// The "Init" method takes a "BaseModuleData" object and a callback function, while the "Deinit" method has no arguments.
/// </summary>
public abstract class BaseModule : MonoBehaviour
{
    public BaseModuleData data;
    
    /// <summary>
    /// This abstract method is used to initialize a module and takes a "BaseModuleData" object and a callback function as arguments.
    /// It is implemented in derived classes to provide module-specific initialization logic.
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public abstract void Init(BaseModuleData baseModuleData, Action callback);

    /// <summary>
    /// This abstract method is used to deinitialize a module and has no arguments.
    /// It is implemented in derived classes to provide module-specific cleanup logic.
    /// </summary>
    public abstract void Deinit();
}

/// <summary>
/// This serializable class provides a way to store module-specific data. It has a virtual method named "GetDependencyData"
/// which returns a list of strings representing the module's dependencies. This method can be overridden in derived classes
/// to provide module-specific dependency data. By default, it returns an empty list.
/// </summary>
[Serializable]
public class BaseModuleData
{
    /// <summary>
    /// This virtual method is used to retrieve a list of string dependencies for a module.
    /// It returns an empty list by default and can be overridden in derived classes to provide module-specific dependency data.
    /// Dependency data is data that needs to be downloaded for a specific module to function correctly.
    /// </summary>
    /// <returns>Type of list<string></returns>
    public virtual List<string> GetDependencyData()
    {
        return new List<string>();
    }
}
