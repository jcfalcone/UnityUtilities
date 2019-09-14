using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Falcone.BuildTool
{
    public static class FileUtility
    {
        public static T[] GetAssetsByType<T>() where T : UnityEngine.Object
        {
            string[] settingsGUID = AssetDatabase.FindAssets("t:BuildScriptWindowSettings");

            if (settingsGUID.Length == 0)
            {
                Debug.Log("No file found!");
                return null;
            }

            T[] assets = new T[settingsGUID.Length];

            for (int count = 0; count < settingsGUID.Length; count++)
            {
                assets[count] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(settingsGUID[count]));
            }

            return assets;
        }

        public static T GetAssetByType<T>() where T : UnityEngine.Object
        {
            string[] settingsGUID = AssetDatabase.FindAssets("t:" + typeof(T).ToString().Replace("UnityEngine.", ""));

            if (settingsGUID.Length == 0)
            {
                Debug.Log("No file found!");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(settingsGUID[0]));
        }
    }
}
