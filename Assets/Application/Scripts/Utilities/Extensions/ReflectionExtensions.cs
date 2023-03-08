using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilities;

public static class ReflectionExtensions
{
    public static bool IsInUserAssembly(this Type type) => type.Assembly.IsUserAssembly();

    public static bool IsUserAssembly(this Assembly assembly)
    {
        return assembly.FullName.StartsWith("Assembly-CSharp");
    }

    public static bool ImplementsInterface<TInterface>(this Type type) => typeof(TInterface).IsAssignableFrom(type);

    public static bool ImplementsInterface(this Type type, Type interfaceType) => interfaceType.IsAssignableFrom(type);

    public static List<Type> GetChildTypes(this Type type)
    {
        return Reflection.CollectUserTypes(t => t.IsSubclassOf(type));
    }

    public static Type GetGenericBaseType(this Type type)
    {
        if (type.IsGenericType == false)
        {
            if (type.BaseType != null)
            {
                return GetGenericBaseType(type.BaseType);
            }
            return null;
        }
        return type;
    }

    public static bool IsOfType<T>(this Type type) => type.IsOfType(typeof(T));

    public static bool IsOfType(this Type type, Type targetType)
    {
        if (type == targetType)
        {
            return true;
        }
        if (targetType.IsInterface == true)
        {
            return type.ImplementsInterface(targetType);
        }
        if (type.BaseType != null)
        {
            return type.BaseType.IsOfType(targetType);
        }
        return false;
    }

    public static List<Type> GetBaseTypes(this Type type, Type stopAt = null)
    {
        var types = new List<Type> { type };
        if (type.BaseType != null && type.BaseType != stopAt)
        {
            types.InsertRange(0, type.BaseType.GetBaseTypes(stopAt));
        }
        return types;
    }

    public static T GetAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttributes(false).OfType<T>().SingleOrDefault();
    }

    public static (T, FieldInfo) GetComponentType<T, C>(int targetIndex, C classHolder) where C : class where T : Attribute
    {
        int fieldIndex = 0;
        FieldInfo[] props = classHolder.GetType().GetFields(); //TODO: needs testing
        foreach (FieldInfo prop in props)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                T castAttr = attr as T;
                if (castAttr != null)
                {
                    if (fieldIndex == targetIndex)
                    {
                        return (castAttr, prop);
                    }

                    fieldIndex++;
                }
            }
        }

        return default;
    }
}