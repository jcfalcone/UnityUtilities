using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace Falcone.BuildTool
{
    public static class UIUtility
    {
        public static void BeginCenterGroup()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        }

        public static void EndCenterGroup()
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void Space(int _spaces = 1)
        {
            do
            {
                GUILayout.Label("");
            } while (--_spaces > 0);
        }

        public static void SubTitle(string _title, int _size = 70, GUIStyle _style = null)
        {
            if(_style == null)
            {
                _style = EditorStyles.boldLabel;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(_title, _style, GUILayout.MaxWidth(_size));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.EndHorizontal();
        }

        public static void HR()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public static void SetInspectorFile(Object _selected)
        {
            Selection.activeObject = _selected;
        }

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, System.IComparable<T>
        {
            List<T> objects = new List<T>();
            foreach (System.Type type in Assembly.GetAssembly(typeof(T)).GetTypes()
                                                 .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)System.Activator.CreateInstance(type, constructorArgs));
            }
            objects.Sort();
            return objects.Distinct();
        }
    }
}