using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WebSocketSharp;

namespace Utilities
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class InspectorButtonEditor : Editor
    {
        private List<AttributeMethod<InspectorButtonAttribute>> m_Methods = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_Methods == null)
            {
                m_Methods = GetMethods<InspectorButtonAttribute>(target.GetType());
            }

            if (m_Methods.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            foreach (var m in m_Methods)
            {
                using (new GuiEnabledScope(EditorApplication.isPlaying || !m.Attribute.PlayModeOnly))
                {
                    if (DrawButton(m) == true)
                    {
                        foreach (var obj in targets)
                        {
                            var paramCount = m.Method.GetParameters().Length;
                            m.Method.Invoke(obj, new object[paramCount]);
                        }
                        Repaint();
                    }
                }
            }
        }

        private static bool DrawButton(AttributeMethod<InspectorButtonAttribute> m)
        {
            string text = m.Attribute.TextStr.IsNullOrEmpty() ? $"{m.Method.Name}()" : $"{m.Attribute.TextStr}()";
            if (m.Attribute.IsHoldButton == true)
            {
                return GUILayout.RepeatButton(text);
            }
            else
            {
                return GUILayout.Button(text);
            }
        }

        private List<AttributeMethod<T>> GetMethods<T>(Type type) where T : Attribute
        {
            var allMethods = type.GetMethods(Reflection.AnyFlags()).ToList();
            allMethods.RemoveAll(m => m.IsAbstract == true);

            var attributes = allMethods.Extract(m => new AttributeMethod<T> { Method = m });
            attributes.ForEach(a => a.Attribute = a.Method.GetCustomAttribute<T>());
            attributes.RemoveAll(a => a.Attribute == null);

            if (type.BaseType != null && type.BaseType.IsInUserAssembly() == true)
            {
                var baseMethods = GetMethods<T>(type.BaseType);
                var methodHash = attributes.Extract(a => a.Method.Name).Hash();
                baseMethods.RemoveAll(b => methodHash.Contains(b.Method.Name));
                attributes.InsertRange(0, baseMethods);
            }
            return attributes;
        }


        private class AttributeMethod<T> where T : Attribute
        {
            public MethodInfo Method;
            public T Attribute;
        }
    }
}