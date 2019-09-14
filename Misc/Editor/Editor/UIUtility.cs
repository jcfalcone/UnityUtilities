using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    }
}