using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

/// <summary>
/// Some systems need to run inside and outside of playmode so this util is useful to avoid having to duplicate code to support editor and runtime
/// </summary>
public class CoroutineUtil : MonoSingleton<CoroutineUtil>
{
    /// <summary>
    /// Allows coroutines to run in editor mode and play mode
    /// </summary>
    /// <param name="coroutine">The coroutine that needs to run</param>
    public void RunCoroutine(IEnumerator coroutine)
    {
        if (IsEditorMode())
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutine(coroutine, Instance);
#endif
        }
        else
        {
            StartCoroutine(coroutine);
        }
    }
    /// <summary>
    /// A static method that returns true if the code is running in editor mode and false for play mode
    /// </summary>
    public static bool IsEditorMode()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && Application.isEditor)
        {
            return true;
        }
#endif
        return false;
    }
}
