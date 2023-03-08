using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public enum ListGet
{
    Wrap,
    Clamp,
    Nothing,
}

public static class Extensions
{
    public const string CsvDelimiter = "|";

    public static T AddFlag<T>(this Enum mask, T flag) where T : struct
    {
        return (T)(object)(Convert.ToInt32(mask) | (int)(object)flag);
    }

    public static bool IsEmpty<T>(this List<T> list)
    {
        return (list == null || list.Count == 0);
    }

    public static List<T> List<T>(this T item) => new List<T> { item };

    public static List<T> PrepareList<T>(this IList list) where T : class
    {
        int count = list.Count;
        var newList = new List<T>(count);
        for (int i = 0; i < count; ++i)
        {
            newList.Add(null);
        }
        return newList;
    }

    public static List<T> NewList<T>(this List<T> list) where T : new()
    {
        int count = list.Count;
        var newList = new List<T>(count);
        for (int i = 0; i < count; ++i)
        {
            newList.Add(new T());
        }
        return newList;
    }

    public static float Maximum<T>(this List<T> list, Func<T, float> selector)
    {
        if (list.Count == 0) { return 0; }
        return list.Max(selector);
    }

    public static float Minimum<T>(this List<T> list, Func<T, float> selector)
    {
        if (list.Count == 0) { return 0; }
        return list.Min(selector);
    }

    public static List<T> ToList<T>(this T[] array)
    {
        if (array != null)
        {
            return new List<T>(array);
        }
        return null;
    }

    public static List<T> Clone<T>(this List<T> list)
    {
        if (list != null)
        {
            return new List<T>(list);
        }
        return null;
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        var clone = list.Clone();
        int n = clone.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            var item = clone[k];
            clone[k] = clone[n];
            clone[n] = item;
        }
        return clone;
    }

    public static List<T> MoveUp<T>(this List<T> list, int index)
    {
        T item = list[index];
        list.RemoveAt(index);
        list.Insert(index - 1, item);
        return list;
    }

    public static List<T> MoveDown<T>(this List<T> list, int index)
    {
        T item = list[index];
        list.RemoveAt(index);
        list.Insert(index + 1, item);
        return list;
    }

    public static bool CanMoveUp<T>(this List<T> list, int index)
    {
        return (index > 0);
    }

    public static bool CanMoveDown<T>(this List<T> list, int index)
    {
        return (list.Count < (index - 2));
    }

    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary != null)
        {
            return new Dictionary<TKey, TValue>(dictionary);
        }
        return null;
    }

    public static T Pop<T>(this List<T> list)
    {
        return list.PopAt(0);
    }

    public static T PopBack<T>(this List<T> list)
    {
        return list.PopAt(list.Count - 1);
    }

    public static T PopAt<T>(this List<T> list, int index)
    {
        if (index >= 0 && index < list.Count)
        {
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }
        return default(T);
    }

    public static void RemoveAfter<T>(this List<T> list, T target) where T : class
    {
        int i = list.FindIndex(item => item == target);
        if (i >= 0)
        {
            list.RemoveRange(i, list.Count - i);
        }
    }

    public static T First<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            return list[0];
        }
        return default(T);
    }

    public static T Last<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            return list[list.Count - 1];
        }
        return default(T);
    }

    public static T Get<T>(this List<T> list, T item, int delta, ListGet getType = ListGet.Nothing)
    {
        int index = list.IndexOf(item);
        if (index >= 0)
        {
            return list.Get(index + delta, getType);
        }
        return default(T);
    }

    public static T Get<T>(this List<T> list, int index, ListGet getType = ListGet.Clamp)
    {
        if (getType == ListGet.Wrap)
        {
            index = index.Wrap(list);
        }
        else if (getType == ListGet.Clamp)
        {
            index = index.Clamp(list);
        }
        else if (getType == ListGet.Nothing)
        {
            if (index < 0 || index >= list.Count)
            {
                return default(T);
            }
        }
        return list[index];
    }

    public static void AddToList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key) == false)
        {
            dictionary[key] = new List<TValue>();
        }
        dictionary[key].Add(value);
    }

    public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class
    {
        if (dictionary.ContainsKey(key) == true)
        {
            return dictionary[key];
        }
        return null;
    }

    public static TValue? TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : struct
    {
        if (dictionary.ContainsKey(key) == true)
        {
            return dictionary[key];
        }
        return null;
    }

    public static T IndexOrDefault<T>(this List<T> list, int index)
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        return default(T);
    }

    public static T IndexOrDefault<T>(this List<T> list, int index, Func<T> getDefault)
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        return getDefault();
    }

    public static List<T> GetDuplicates<T>(this List<T> list)
    {
        return list.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
    }

    public static List<T> Intersect<T>(this List<List<T>> listOfLists)
    {
        if (listOfLists.Count > 0)
        {
            var list = listOfLists[0].Clone();
            for (int i = 0; i < listOfLists.Count; ++i)
            {
                list.Intersect(listOfLists[i]);
            }
            return list;
        }
        return listOfLists.Flatten();
    }

    public static HashSet<T> Hash<T>(this IEnumerable<T> list) => new HashSet<T>(list);

    public static Dictionary<TKey, TValue> ExtractAsValues<TKey, TValue>(this List<TValue> list, Func<TValue, TKey> selector)
    {
        return list.Extract(item => selector(item), item => item);
    }

    public static Dictionary<TKey, TValue> ExtractAsKeys<TKey, TValue>(this List<TKey> list, Func<TKey, TValue> selector)
    {
        return list.Extract(item => item, item => selector(item));
    }

    public static Dictionary<TKey, TValue> Extract<T, TKey, TValue>(this List<T> list, Func<T, TKey> key, Func<T, TValue> value)
    {
        var dict = new Dictionary<TKey, TValue>(list.Count);
        foreach (var item in list)
        {
            try
            {
                dict.Add(key(item), value(item));
            }
            catch (ArgumentException ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
        return dict;
    }

    public static void RemoveByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue someValue)
    {
        List<TKey> itemsToRemove = new List<TKey>();

        foreach (var pair in dictionary)
        {
            if (pair.Value.Equals(someValue))
                itemsToRemove.Add(pair.Key);
        }

        foreach (TKey item in itemsToRemove)
        {
            dictionary.Remove(item);
        }
    }

    public static Dictionary<TValue, TKey> Inverse<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        var inverse = new Dictionary<TValue, TKey>(dictionary.Count);
        foreach (var key in dictionary.Keys)
        {
            inverse.Add(dictionary[key], key);
        }
        return inverse;
    }

    public static List<T> DistinctList<T>(this List<T> list) => list.Distinct().ToList();

    public static List<T> Modify<T>(this List<T> list, Func<T, T> modifier)
    {
        if (list == null) { return null; }

        int count = list.Count;
        for (int i = 0; i < count; ++i)
        {
            list[i] = modifier(list[i]);
        }
        return list;
    }

    public static List<TKey> Extract<T, TKey>(this List<T> list, Func<T, TKey> selector)
    {
        if (list == null) { return null; }

        var keys = new List<TKey>(list.Count);
        foreach (var item in list)
        {
            keys.Add(selector(item));
        }
        return keys;
    }

    public static List<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
    {
        if (enumerable == null)
        {
            return null;
        }

        var all = new List<T>();
        foreach (var inner in enumerable)
        {
            foreach (var item in inner)
            {
                all.Add(item);
            }
        }
        return all;
    }

    public static List<T> Merge<T>(this List<T> list, params List<T>[] lists)
    {
        var all = lists.ToList();
        all.Insert(0, list);
        return all.Flatten();
    }

    public static void SortOrIgnore<T>(this List<T> list, Comparison<T> comparison)
    {
        var dict = new Dictionary<T, int>(list.Count);
        for (int i = 0; i < list.Count; ++i)
        {
            dict.Add(list[i], i);
        }
        list.Sort((x, y) =>
        {
            int result = comparison(x, y);
            if (result != 0)
            {
                return result;
            }
            return dict[x].CompareTo(dict[y]);
        });
    }

    public static T GetRandom<T>(this List<T> list)
    {
        if (list.Count == 1)
        {
            return list[0];
        }
        else if (list.Count > 0)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        return default(T);
    }

    public static int GetRandomIndex<T>(this List<T> self)
    {
        return UnityEngine.Random.Range(0, self.Count);
    }

    public static List<TOutput> To<TOutput>(this IList list) where TOutput : class
    {
        var output = new List<TOutput>(list.Count);
        for (int i = 0; i < list.Count; ++i)
        {
            output.Add(list[i] as TOutput);
        }
        return output;
    }

    public static void AddIfNotNull<T>(this List<T> list, T obj) where T : class
    {
        if (IsNull(obj) == false)
        {
            list.Add(obj);
        }
    }

    private static bool IsNull<T>(T obj) where T : class
    {
        return obj == null || ((obj is UnityEngine.Object) && (obj as UnityEngine.Object) == null);
    }

    public static string Stringify<T>(this List<T> list) => list.Stringify(s => s.ToString());
    public static string Stringify<T>(this List<T> list, Func<T, string> log) => list.Stringify(log, "\n");
    public static string Stringify<T>(this List<T> list, Func<T, string> log, string separator)
    {
        if (list.Count > 0)
        {
            var str = new StringBuilder();
            int sepLen = separator.Length;
            list.ForEach(e => str.Append(log(e) + separator));
            return str.Remove(str.Length - sepLen, sepLen).ToString();
        }
        return string.Empty;
    }

    public static List<string> SplitList(this string str, params string[] delimiters) => str.SplitList(false, delimiters);

    public static List<string> SplitList(this string str, bool removeEmpties, params string[] delimiters)
    {
        if (delimiters.Length == 0)
        {
            delimiters = new string[] { CsvDelimiter };
        }
        var options = removeEmpties ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
        return str.Split(delimiters, options).ToList();
    }

    public static string Substring(this string str, string from, string to)
    {
        int i = str.IndexOf(from);
        if (i >= 0)
        {
            i += from.Length;

            int j = str.IndexOf(to, i);
            if (j >= 0)
            {
                return str.Substring(i, j - i);
            }
        }

        return str;
    }

    public static string SubstringToEnd(this string str, string from)
    {
        int i = str.IndexOf(from);
        if (i >= 0)
        {
            i += from.Length;
            return str.Substring(i, str.Length - i);
        }

        return str;
    }

    public static string SubstringFromStart(this string str, string to)
    {
        int j = str.IndexOf(to);
        if (j >= 0)
        {
            return str.Substring(0, j);
        }

        return str;
    }

    public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        return text.IndexOf(value, stringComparison) >= 0;
    }

    public static string Strip(this string str, params string[] toRemove)
    {
        for (int i = 0; i < toRemove.Length; ++i)
        {
            str = str?.Replace(toRemove[i], "");
        }
        return str;
    }


    public static byte[] GetBytes(this string str) => Encoding.UTF8.GetBytes(str);
    public static string GetString(this byte[] bytes) => Encoding.UTF8.GetString(bytes);

    public static string EncodeToBase64(this string str) => Convert.ToBase64String(str.GetBytes());
    public static string DecodeFromBase64(this string str) => Convert.FromBase64String(str).GetString();

    public static string ShaHash(this string str)
    {
        using (var hash = SHA256.Create())
        {
            byte[] hashed = hash.ComputeHash(str.GetBytes());

            var sb = new StringBuilder();
            for (int i = 0; i < hashed.Length; ++i)
            {
                sb.Append(hashed[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public static string ToLowerUnderscored(this string str)
    {
        return string.Concat(str.Select((x, i) => (i > 0 && char.IsUpper(x)) ? "_" + x.ToString() : x.ToString()).ToArray()).ToLower();
    }

    public static string SanitiseSlashes(this string str) => str?.Replace('\\', '/');
    public static string DesanitiseSlashes(this string str) => str.DesanitiseSlashes(System.IO.Path.DirectorySeparatorChar);
    public static string DesanitiseSlashes(this string str, char pathChar) => str?.Replace('/', pathChar);

    public static bool IsEnum<T>(this string str, bool ignoreCase = false)
    {
        return Enum.IsDefined(typeof(T), str);
    }

    public static T ParseEnum<T>(this string str, bool ignoreCase = false)
    {
        return (T)Enum.Parse(typeof(T), str, ignoreCase);
    }

    public static Transform FindChildRecursive(this Transform parent, string nameToFind)
    {
        foreach (Transform child in parent)
        {
            if (child.name == nameToFind)
                return child;
            var result = FindChildRecursive(child, nameToFind);
            if (result != null)
                return result;
        }
        return null;
    }

    public static GameObject CreateChildObject<T>(this GameObject go, string name, bool worldPosStays)
    {
        var createdGo = new GameObject(name, typeof(T));
        createdGo.transform.SetParent(go.transform, worldPosStays);
        createdGo.transform.localScale = Vector3.one;

        return createdGo;
    }
}
