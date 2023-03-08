using System.Collections.Generic;
using Utilities;

/// <summary>
/// This abstract class extends the "BaseModule" class and provides a base implementation for content-based modules.
/// It has protected fields for a playlist and current index, which can be accessed by derived classes.
/// </summary>
/// <see cref="BaseModule"/>
public abstract class ContentModule : BaseModule
{
    protected System.Object[] playlist;
    protected int currentIndex = 0;

    /// <summary>
    /// This method overrides the "Deinit" method in the "BaseModule" class to clean up the module-specific data.
    /// It sets the "playlist" field to null, resets the "currentIndex" field to 0, and sets the "data" field to null.
    /// </summary>
    public override void Deinit()
    {
        playlist = null;
        currentIndex = 0;
        data = null;
    }

    protected abstract void UpdateContent();
    
    [InspectorButton]
    protected virtual void GoToNextContent()
    {
        currentIndex++;
        if (currentIndex >= playlist.Length)
        {
            currentIndex = 0;
        }

        UpdateContent();
    }
    
    [InspectorButton]
    protected virtual void GoToPrevContent()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = playlist.Length - 1;
        }

        UpdateContent();
    }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="BaseModuleData"/>
public class ContentModuleData : BaseModuleData
{
    /// <summary>
    /// The media guids
    /// </summary>
    public List<string> mediaGuids;

    /// <summary>
    /// This virtual method is used to retrieve a list of string dependencies for a module.
    /// It returns an empty list by default and can be overridden in derived classes to provide module-specific dependency data.
    /// Dependency data is data that needs to be downloaded for a specific module to function correctly.
    /// </summary>
    /// <returns>Type of list<string></returns>
    public override List<string> GetDependencyData()
    {
        return mediaGuids;
    }
}
