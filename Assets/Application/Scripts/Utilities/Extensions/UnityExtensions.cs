using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;
using Object = UnityEngine.Object;

public static class UnityExtensions
{
    public static void DestroyObject(this Object obj)
    {
        if (obj != null)
        {
            Object.Destroy(obj);
        }
    }

    public static void DestroyGameObject<T>(this T component) where T : Component
    {
        if (component != null)
        {
            Object.Destroy(component.gameObject);
        }
    }

    public static void DestroyObjectImmediate(this Object obj, bool allowDestroyAssets = false)
    {
        if (obj != null)
        {
            Object.DestroyImmediate(obj, allowDestroyAssets);
        }
    }

    public static void RebuildLayout(this RectTransform rect)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    public static RectTransform ToRect(this Transform transform)
    {
        return transform as RectTransform;
    }

    public static RectTransform GetRect<T>(this T component) where T : Component
    {
        return component.transform.ToRect();
    }

    public static Vector2 GetCentre(this RectTransform transform)
    {
        var corners = new Vector3[4];
        transform.GetLocalCorners(corners);

        var total = Vector2.zero;
        for (int i = 0; i < corners.Length; ++i)
        {
            total += (Vector2)corners[i];
        }
        var average = (total / corners.Length);
        return transform.anchoredPosition + average;
    }

    public static Vector3 GetRelativePosition(this Transform target, Transform anchor)
    {
        if (target.IsChildOf(anchor) == false)
        {
            throw new System.Exception("Target must be a child of anchor!");
        }

        if (target.parent != anchor)
        {
            return target.parent.GetRelativePosition(anchor) + target.localPosition;
        }
        else
        {
            return target.localPosition;
        }
    }

    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public static void SetLayerRecursively(this GameObject go, Dictionary<GameObject, int> collection)
    {
        if(collection.ContainsKey(go))
			go.layer = collection[go];
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, collection);
        }
    }

    public static Dictionary<GameObject, int> GetLayerRecursively(this GameObject go)
    {
        Dictionary<GameObject, int> collection = new Dictionary<GameObject, int>();

        return go.GetLayerRecursivelySubLoop(ref collection);
    }
    
    private static Dictionary<GameObject, int> GetLayerRecursivelySubLoop(this GameObject go, ref Dictionary<GameObject, int> collection)
    {
        collection ??= new Dictionary<GameObject, int>();

        collection.Add(go, go.layer);
        foreach (Transform child in go.transform)
        {
	        GetLayerRecursivelySubLoop(child.gameObject, ref collection);
        }

        return collection;
    }

    public static void SetTagRecursively(this GameObject go, string tag)
    {
        go.tag = tag;
        foreach (Transform child in go.transform)
        {
            SetTagRecursively(child.gameObject, tag);
        }
    }

    public static Color ToColour(this Vector3 v)
    {
        return new Color(v.x, v.y, v.z);
    }

    public static Vector3 ToVector3(this Color c)
    {
        return new Vector3(c.r, c.g, c.b);
    }

    public static Vector2 GetFrustumSizeAtDistance(this Camera camera, float distance)
    {
        float fov = (camera.fieldOfView * 0.5f).ToRadians();
        float height = 2.0f * distance * Mathf.Tan(fov);
        float width = height * camera.aspect;
        return new Vector2(width, height);
    }

    public static Vector3 Abs(Vector3 vector)
    {
        vector.x = Mathf.Abs(vector.x);
        vector.y = Mathf.Abs(vector.y);
        vector.z = Mathf.Abs(vector.z);
        return vector;
    }

    public static void Play(this AudioSource audio, MonoBehaviour host, System.Action callback)
    {
        host.StopAllCoroutines();

        float length = audio.clip.length / audio.pitch;
        host.WaitFor(length + 0.5f, callback);
        audio.Play();
    }

    public static void SetEmissionRate(this ParticleSystem particleSystem, float emissionRate)
    {
        var emission = particleSystem.emission;
        emission.rateOverTime = emissionRate;
    }

    public static void SetAlpha(this Graphic graphic, float alpha)
    {
        var colour = graphic.color;
        colour.a = alpha;
        graphic.color = colour;
    }

    public static void SetAlpha(this Material mat, float alpha, string property = null)
    {
        property = property ?? "_Color";
        var colour = mat.GetColor(property);
        colour.a = alpha;
        mat.SetColor(property, colour);
    }

    public static void SetAlpha(this Renderer renderer, float alpha, string property = null)
    {
        renderer.material.SetAlpha(alpha, property);
    }

    public static void SetAlpha(this SpriteRenderer sprite, float alpha)
    {
        var colour = sprite.color;
        colour.a = alpha;
        sprite.color = colour;
    }

    public static void SetTexOffset(this Renderer renderer, float? x = null, float? y = null)
    {
        var m = renderer.material;
        var o = m.mainTextureOffset;
        m.mainTextureOffset = new Vector2(x ?? o.x, y ?? o.y);
    }

    public static void SetTexScale(this Renderer renderer, float? x = null, float? y = null)
    {
        var m = renderer.material;
        var s = m.mainTextureScale;
        m.mainTextureScale = new Vector2(x ?? s.x, y ?? s.y);
    }

    public static void ToggleKeyword(this Material material, string property, bool state)
    {
        if (state == true)
        {
            material.EnableKeyword(property);
        }
        else
        {
            material.DisableKeyword(property);
        }
    }

    public static Material InstantiateMaterial(this Renderer renderer)
    {
        var material = new Material(renderer.sharedMaterial);
        renderer.material = material;
        return material;
    }

    public static void CopyStandardMaterialProperties(this Material destination, Material source)
    {
        foreach (string name in new[] { "_MetallicGlossMap", "_BumpMap", "_EmissionMap", "_MainTex", "_OcclusionMap" })
        {
            if (source.HasProperty(name) && destination.HasProperty(name))
            {
                destination.SetTexture(name, source.GetTexture(name));
                destination.SetTextureScale(name, source.GetTextureScale(name));
                destination.SetTextureOffset(name, source.GetTextureOffset(name));
            }
        }
        foreach (string name in new[] { "_Metallic", "_Emission" })
        {
            if (source.HasProperty(name) && destination.HasProperty(name))
            {
                destination.SetFloat(name, source.GetFloat(name));
            }
        }
        foreach (string name in new[] { "_Color", "_EmissionColor" })
        {
            if (source.HasProperty(name) && destination.HasProperty(name))
            {
                destination.SetColor(name, source.GetColor(name));
            }
        }
    }

    public static Rect GetRect(this Texture2D texture) => new Rect(0, 0, texture.width, texture.height);

    public static void Clear(this RenderTexture renderTexture)
    {
        var currentRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = currentRenderTexture;
    }

    public static void ReadFromRenderTexture(this Texture2D texture, RenderTexture renderTexture)
    {
        var currentRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(texture.GetRect(), 0, 0);
        RenderTexture.active = currentRenderTexture;
    }

    public static Texture2DArray ToTexture2dArray(this List<Texture2D> textures, bool mipmap, bool linear)
    {
        var t = textures[0];
        int count = textures.Count;
        var array = new Texture2DArray(t.width, t.height, count, t.format, mipmap, linear)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        return FillTex2dArray(textures, count, array);
    }

    public static Texture2DArray ToTexture2dArray(this List<Texture2D> textures, Texture2DArray array)
    {
        return FillTex2dArray(textures, textures.Count, array);
    }

    private static Texture2DArray FillTex2dArray(List<Texture2D> textures, int count, Texture2DArray array)
    {
        for (int i = 0; i < count; ++i)
        {
            if (textures[i] != null)
            {
                Graphics.CopyTexture(textures[i], 0, 0, array, i, 0);
            }
        }
        return array;
    }

    public static ComputeBuffer CreateComputeBuffer<T>(this T[] array)
    {
        int stride = array.Length > 0 ? Marshal.SizeOf(array[0]) : Marshal.SizeOf(typeof(T));
        var buffer = new ComputeBuffer(array.Length, stride);
        buffer.SetData(array);
        return buffer;
    }

    public static Transform CreateChild(this Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.Reset(true);
        return go.transform;
    }

    public static Component CreateChild(this Transform parent, System.Type type) => parent.CreateChild(type.Name).gameObject.AddComponent(type);
    public static T CreateChild<T>(this Transform parent, string name) where T : Component => parent.CreateChild(name).AddComponent<T>();
    public static T CreateChild<T>(this Transform parent) where T : Component => parent.CreateChild<T>(typeof(T).Name);

    public static string GetHeirarchyPath<T>(this T component) where T : Component
    {
        Transform transform = component.transform;
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    public static void AddEventTriggerListener(this EventTrigger trigger, EventTriggerType eventType, UnityAction<BaseEventData> callback)
    {
        var triggerCallback = new EventTrigger.TriggerEvent();
        triggerCallback.AddListener(callback);

        trigger.triggers.Add(new EventTrigger.Entry
        {
            eventID = eventType,
            callback = triggerCallback,
        });
    }

    public static void RemoveEventTriggerType(this EventTrigger trigger, EventTriggerType eventType)
    {
        trigger.triggers.RemoveAll(e => e.eventID == eventType);
    }

    public static void SetAnchorAndPivot(this RectTransform transform, float x, float y)
    {
        transform.anchorMin = new Vector2(x, y);
        transform.anchorMax = new Vector2(x, y);
        transform.pivot = new Vector2(x, y);
    }

    public static void SetAnchorMin(this RectTransform transform, float? x = null, float? y = null)
    {
        transform.anchorMin = transform.anchorMin.With(x, y);
    }

    public static void SetAnchorMax(this RectTransform transform, float? x = null, float? y = null)
    {
        transform.anchorMax = transform.anchorMax.With(x, y);
    }

    public static void SetHeight(this RectTransform transform, float height)
    {
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public static void SetWidth(this RectTransform transform, float width)
    {
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public static void SetAnchorPos(this RectTransform transform, float? x = null, float? y = null, float? z = null)
    {
        transform.anchoredPosition3D = transform.anchoredPosition3D.With(x, y, z);
    }

    public static void SetLocalPos(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        transform.localPosition = transform.localPosition.With(x, y, z);
    }

    public static void SetWorldPos(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        transform.position = transform.position.With(x, y, z);
    }

    public static void SetLocalRot(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        transform.localEulerAngles = transform.localEulerAngles.With(x, y, z);
    }

    public static void SetScale(this Transform transform, float? x = null, float? y = null, float? z = null)
    {
        var s = transform.localScale;
        if (s.x != x || s.y != y || s.z != z)
        {
            transform.localScale = s.With(x, y, z);
        }
    }

    public static void SetScale(this Transform transform, float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    public static Vector2 With(this Vector2 original, float? x = null, float? y = null)
    {
        original.x = x ?? original.x;
        original.y = y ?? original.y;
        return original;
    }

    public static Vector3 With(this Vector3 original, float? x = null, float? y = null, float? z = null)
    {
        original.x = x ?? original.x;
        original.y = y ?? original.y;
        original.z = z ?? original.z;
        return original;
    }

    public static GUIContent ToGUI(this string str) => new GUIContent(str);

    public static bool IsValidTransformPath(string sceneName, string path)
    {
	    Scene scene = SceneManager.GetSceneByName(sceneName);
	    GameObject[] sceneGameObject = scene.GetRootGameObjects();
	    string GOName = path.Contains("/") ? string.Concat(path.TakeWhile((x) => x != '/')) : path;

	    string pathFromRoot = path.Contains("/") ? path.Remove(0, GOName.Length + 1) : path;

	    foreach (GameObject sGameObject in sceneGameObject)
	    {
		    if (sGameObject.name != GOName)
			    continue;

		    Transform obj = sGameObject.transform.Find(pathFromRoot);
            
		    return obj != null;
	    }

	    return false;
    }

    public static EventTrigger AddButtonClicksToSlider(this Slider slider, Action<BaseEventData> onDownCallback, Action<BaseEventData> onUpCallback)
    {
	    EventTrigger eventTrigger = slider.gameObject.ForceComponent<EventTrigger>();
	    eventTrigger.triggers.Clear();
	    EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
	    EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();

	    pointerDownEvent.eventID = EventTriggerType.PointerDown;
	    pointerDownEvent.callback.AddListener(data => onDownCallback(data));

	    pointerUpEvent.eventID = EventTriggerType.PointerUp;
	    pointerUpEvent.callback.AddListener((data) => onUpCallback(data));

	    eventTrigger.triggers.Add(pointerDownEvent);
	    eventTrigger.triggers.Add(pointerUpEvent);

	    return eventTrigger;
    }

    public static Bounds CalculateMeshFilterBounds(this GameObject holder, bool logWarnings = true)
    {
	    MeshFilter[] meshFilters = holder.GetComponentsInChildren<MeshFilter>();
	    if (meshFilters != null && meshFilters.Length > 0)
	    { // Check all meshrenderers and make them visible, and also create an encapsulating collider for clickability
		    Bounds calculatedBounds = Application.isPlaying ? meshFilters[0].mesh.bounds : meshFilters[0].sharedMesh.bounds;
		    calculatedBounds.center = meshFilters[0].transform.TransformPoint(calculatedBounds.center);
		    for (int i = 0; i < meshFilters.Length; i++)
		    {
			    Bounds meshBounds = Application.isPlaying ? meshFilters[i].mesh.bounds : meshFilters[i].sharedMesh.bounds;

                meshBounds.center = meshFilters[i].transform.TransformPoint(meshBounds.center);
			    calculatedBounds.Encapsulate(meshBounds);
		    }

		    calculatedBounds.center = holder.transform.InverseTransformPoint(calculatedBounds.center);
		    return calculatedBounds;

	    }

        if(logWarnings)
			Debug.LogWarning("Unable to generate MeshFilter bounds and no meshFilters were found", holder);

        return new Bounds();
    }

    public static Bounds CalculateRendererBounds(this GameObject holder)
    {
        Quaternion currentRotation = holder.transform.rotation;
        holder.transform.rotation = Quaternion.identity;
        Bounds calculatedBounds = new Bounds(holder.transform.position, Vector3.zero);
        MeshRenderer[] meshRenderers = holder.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                calculatedBounds.Encapsulate(meshRenderers[i].bounds);
            }
        }
        Vector3 localCenter = calculatedBounds.center - holder.transform.position;
        calculatedBounds.center = localCenter;
        holder.transform.rotation = currentRotation;
        return calculatedBounds;
    }
    
    public static Vector2 GetNormalisedHitCoords(this RaycastHit hit, Bounds bounds, Transform localSpaceParent, Vector2 pivotOffset)
    {
	    Vector3 localHitPoint = localSpaceParent.InverseTransformPoint(hit.point);

	    localHitPoint -= bounds.center;
	    localHitPoint += bounds.size.Multiply(pivotOffset);

	    float x = 1f - (localHitPoint.x / bounds.size.x);
	    float y = localHitPoint.y / bounds.size.y;

	    return new Vector2(x, y);
    }

    public static void ReplaceCoroutine(this MonoBehaviour scope, IEnumerator newCoroutine, ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            scope.StopCoroutine(coroutine);
        }

        coroutine = newCoroutine != null ? scope.StartCoroutine(newCoroutine) : null;
    }
}
