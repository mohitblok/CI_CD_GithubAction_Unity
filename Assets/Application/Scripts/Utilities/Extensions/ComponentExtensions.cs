using System.Collections.Generic;
using UnityEngine;

public static class ComponentExtensions
{
    public static void SetActive<T>(this T component, bool state) where T : Component
    {
        var go = component.gameObject;
        if (go.activeSelf != state)
        {
            go.SetActive(state);
        }
    }

    public static GameObject FindGameObject(this Transform transform, string name)
    {
        return transform?.Find(name)?.gameObject;
    }

    public static RectTransform FindRect(this Transform transform, string name)
    {
        return transform?.Find(name) as RectTransform;
    }

    public static T GetComponentInSibling<T>(this Transform transform, bool includeInactive = false) where T : Component
    {
        return transform.parent?.GetComponentInChildren<T>(includeInactive);
    }

    public static List<T> GetComponentsInSibling<T>(this Transform transform) where T : Component
    {
        return transform.parent?.GetComponentsInChildren<T>().ToList();
    }

    public static T FindComponent<T>(this List<GameObject> objs) where T : Component
    {
        foreach (var go in objs)
        {
            var component = go.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
        }
        return null;
    }

    public static T FindComponent<T>(this Transform transform, string name) where T : Component
    {
        return transform?.Find(name)?.GetComponent<T>();
    }

    public static List<T> FindComponents<T>(this Transform transform, string name) where T : Component
    {
        var child = transform?.Find(name);
        if (child != null)
        {
            return child.FindComponents<T>();
        }
        return new List<T>();
    }

    public static List<T> FindComponents<T>(this Transform transform) where T : Component
    {
        var list = new List<T>();
        foreach (Transform child in transform)
        {
            list.AddIfNotNull(child.GetComponent<T>());
        }
        return list;
    }

    public static T FindComponentInSibling<T>(this Transform transform, string name) where T : Component
    {
        return transform.parent.FindComponent<T>(name);
    }

    public static List<T> FindComponentsInSibling<T>(this Transform transform, string name) where T : Component
    {
        return transform.parent.FindComponents<T>(name);
    }

    public static List<T> FindComponentsInChildren<T>(this Transform transform) where T : Component
    {
        return transform.GetComponentsInChildren<T>(true).ToList();
    }

    public static List<T> FindComponentsInChildren<T>(this Transform transform, string name) where T : Component
    {
        return transform.Find(name)?.FindComponentsInChildren<T>();
    }

    public static T SearchComponent<T>(this Transform transform, string name) where T : Component
    {
        return transform.Search(name)?.GetComponent<T>();
    }

    private static Transform SearchInternal(this Transform transform, string name)
    {
        var child = transform.Find(name);
        if (child != null)
        {
            return child;
        }
        else
        {
            foreach (Transform newChild in transform)
            {
                child = newChild.Search(name);
                if (child != null)
                {
                    return child;
                }
            }
        }
        return null;
    }

    public static Transform Search(this Transform transform, string name, bool displayErrorMessage = false)
    {
        Transform returnTransform = transform.SearchInternal(name);
        if ((null == returnTransform) && (true == displayErrorMessage))
        {
            Debug.LogError($"Cannot find {name} in gameObject {transform.gameObject.GetGameObjectPath()}", transform);
        }
        return returnTransform;
    }


    public static List<Transform> SearchAll(this Transform transform, string name)
    {
        var children = transform.GetDirectChildren();
        var found = children.FindAll(t => t.name == name);
        var foundChildren = children.Extract(t => t.SearchAll(name));
        found.AddRange(foundChildren.Flatten());
        return found;
    }

    public static Transform FindSibling(this Transform transform, string name)
    {
        return transform.parent.Find(name);
    }

    public static List<Transform> GetDirectChildren(this Transform transform)
    {
        var children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        return children;
    }

    public static List<T> GetComponentDirectChildren<T>(this Transform transform) where T : Behaviour
    {
        var components = new List<T>();

        transform.GetDirectChildren().ForEach(c =>
        {
            var baseObject = c.GetComponent<T>();
            if (baseObject)
            {
                components.Add(baseObject);
            }
        });

        return components;
    }

    public static T GetSelfOrDirectChildren<T>(this Transform transform) where T : Behaviour
    {
        T item = transform.GetComponent<T>();
        if (item == null)
        {
            var list = transform.GetDirectChildren();
            foreach (var item1 in list)
            {
                item = item1.GetComponent<T>();
                if (item != null)
                {
                    return item;
                }
            }
        }
        return item;
    }

    public static List<Transform> GetAllChildren(this Transform transform)
    {
        var children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.AddRange(child.GetAllChildren());
            children.Add(child);
        }
        return children;
    }

    public static Dictionary<string, Transform> SearchChildren(this Transform transform)
    {
        var children = new Dictionary<string, Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child.name, child);

            var grandChildren = child.SearchChildren();
            foreach (var grandChild in grandChildren)
            {
                children.Add(grandChild.Key, grandChild.Value);
            }
        }
        return children;
    }

    public static int GetMatchingString(this string[] options, string toFind)
    {
        int index;

        for (index = 0; index < options.Length; index++)
        {
            if (options[index] == toFind)
                break;
        }

        return index;
    }

    public static List<Transform> SearchChildren(this Transform transform, string name)
    {
        var children = GetAllChildren(transform);
        var matching = new List<Transform>();
        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                matching.Add(child);
            }
        }

        return matching;
    }

    public static List<Transform> SearchChildrenWithNameContainsList(this Transform transform, string name)
    {
        var children = GetAllChildren(transform);
        var matching = new List<Transform>();
        foreach (Transform child in children)
        {
            if (true == child.name.CaseInsensitiveContains(name))
            {
                matching.Add(child);
            }
        }

        return matching;
    }

    public static Transform SearchChildrenWithNameContains(this Transform transform, string name)
    {
        if (transform.name.CaseInsensitiveContains(name))
        {
            // this object starts with the string passed in "start":
            // do whatever you want with it...
            return transform;
        }
        // now search in its children, grandchildren etc.
        foreach (Transform newChild in transform)
        {
            Transform returnTransform = SearchChildrenWithNameContains(newChild, name);
            if (null != returnTransform)
            {
                return returnTransform;
            }
        }
        return null;
    }

    public static Transform FindParent(this Transform transform, string name)
    {
        if (transform.parent == null)
        {
            return null;
        }
        else if (transform.parent.name == name)
        {
            return transform.parent;
        }
        else
        {
            return transform.parent.FindParent(name);
        }
    }

    public static void Reset(this Transform transform, bool withScale = true)
    {
        if (transform.localPosition.sqrMagnitude != 0)
        {
            transform.localPosition = Vector3.zero;
        }
        if (transform.localEulerAngles.sqrMagnitude != 0)
        {
            transform.localRotation = Quaternion.identity;
        }

        if (withScale == true)
        {
            if (transform.localScale.sqrMagnitude != 1)
            {
                transform.localScale = Vector3.one;
            }
        }
    }

    public static void OrientTo(this Transform transform, Transform target, bool withScale = true)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;

        if (withScale == true)
        {
            transform.localScale = target.localScale;
        }
    }

    public static void SnapToParent(this Transform transform, Transform parent, bool withScale = true)
    {
        if (transform.parent != parent)
        {
            transform.SetParent(parent, false);
        }
        transform.Reset(withScale);
    }

    public static List<Transform> GetOtherChildren(this Transform transform, params Component[] components)
    {
        var children = transform.FindComponents<Transform>();
        foreach (var component in components)
        {
            children.Remove(component.transform);
        }
        return children;
    }

    public static T ForceComponent<T>(this GameObject go) where T : Component
    {
        var component = go?.GetComponent<T>();
        if (component == null)
        {
            return go?.AddComponent<T>();
        }
        return component;
    }

    public static void DestroyComponent<T>(this Component component) where T : Component
    {
        component.GetComponent<T>().DestroyObject();
    }

    public static void DestroyComponent<T>(this GameObject go) where T : Component
    {
        go.GetComponent<T>().DestroyObject();
    }

    public static T AddComponent<T>(this Component component) where T : Component
    {
        return component.gameObject.AddComponent<T>();
    }

    public static string GetGameObjectPath(this Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    public static string GetGameObjectPath(this GameObject gameObject)
    {
        return GetGameObjectPath(gameObject.transform);
    }

    public static void DestroyChildren(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).DestroyGameObject();
        }
    }

    public static void DestroyChildrenImmediate(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).DestroyObjectImmediate();
        }
    }

    public static void CopyMainSettings(this Camera dest, Camera source)
    {
        dest.backgroundColor = source.backgroundColor;

        dest.useOcclusionCulling = source.useOcclusionCulling;
        dest.depthTextureMode = source.depthTextureMode;
        dest.clearFlags = source.clearFlags;
        dest.cullingMask = source.cullingMask;

        dest.orthographic = source.orthographic;
        dest.orthographicSize = source.orthographicSize;
        dest.allowDynamicResolution = source.allowDynamicResolution;
        dest.allowMSAA = source.allowMSAA;
        dest.allowHDR = source.allowHDR;
        dest.eventMask = source.eventMask;
        dest.rect = source.rect;

        dest.stereoTargetEye = source.stereoTargetEye;
        dest.stereoConvergence = source.stereoConvergence;
        dest.stereoSeparation = source.stereoSeparation;

        dest.pixelRect = source.pixelRect;

        dest.targetDisplay = source.targetDisplay;
        dest.targetTexture = source.targetTexture;

        dest.renderingPath = source.renderingPath;
        dest.farClipPlane = source.farClipPlane;
        dest.nearClipPlane = source.nearClipPlane;

        if (source.stereoTargetEye == StereoTargetEyeMask.None)
        {
            dest.fieldOfView = source.fieldOfView;
        }
    }

    public static void CopySettings(this Camera dest, Camera source)
    {
        dest.clearStencilAfterLightingPass = source.clearStencilAfterLightingPass;

        dest.usePhysicalProperties = source.usePhysicalProperties;
        dest.backgroundColor = source.backgroundColor;
        dest.sensorSize = source.sensorSize;

        dest.lensShift = source.lensShift;
        dest.cullingMatrix = source.cullingMatrix;
        dest.useOcclusionCulling = source.useOcclusionCulling;
        dest.layerCullDistances = source.layerCullDistances;
        dest.cameraType = source.cameraType;
        dest.layerCullSpherical = source.layerCullSpherical;
        dest.depthTextureMode = source.depthTextureMode;
        dest.clearFlags = source.clearFlags;
        dest.cullingMask = source.cullingMask;

        dest.focalLength = source.focalLength;
        dest.aspect = source.aspect;
        dest.depth = source.depth;

        dest.transparencySortAxis = source.transparencySortAxis;
        dest.transparencySortMode = source.transparencySortMode;
        dest.opaqueSortMode = source.opaqueSortMode;
        dest.orthographic = source.orthographic;
        dest.orthographicSize = source.orthographicSize;
        dest.forceIntoRenderTexture = source.forceIntoRenderTexture;
        dest.allowDynamicResolution = source.allowDynamicResolution;
        dest.allowMSAA = source.allowMSAA;
        dest.allowHDR = source.allowHDR;
        dest.eventMask = source.eventMask;
        dest.rect = source.rect;

        dest.stereoTargetEye = source.stereoTargetEye;

        dest.stereoConvergence = source.stereoConvergence;
        dest.stereoSeparation = source.stereoSeparation;
        dest.pixelRect = source.pixelRect;
        dest.useJitteredProjectionMatrixForTransparentRendering = source.useJitteredProjectionMatrixForTransparentRendering;
        dest.nonJitteredProjectionMatrix = source.nonJitteredProjectionMatrix;
        dest.projectionMatrix = source.projectionMatrix;
        dest.worldToCameraMatrix = source.worldToCameraMatrix;
        dest.targetDisplay = source.targetDisplay;
        dest.targetTexture = source.targetTexture;

        dest.scene = source.scene;
        dest.renderingPath = source.renderingPath;
        dest.farClipPlane = source.farClipPlane;
        dest.nearClipPlane = source.nearClipPlane;

        if (source.stereoTargetEye == StereoTargetEyeMask.None)
        {
            dest.fieldOfView = source.fieldOfView;
        }
    }
}
