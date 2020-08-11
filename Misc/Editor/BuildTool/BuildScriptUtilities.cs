using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace Falcone.BuildTool
{
    public static class BuildScriptUtilities
    {
        public static bool terminal = false;

        #region Sequence
        public static bool IsNewSequence(BuildEditorSettings _settings)
        {
            string verion = BuildScript.ParseString(_settings.Version);
            string[] split = verion.Split(new string[] { "{Sequence}" }, StringSplitOptions.None);

            bool sameVersion = true;

            for (int count = 0; count < split.Length; count++)
            {
                sameVersion = sameVersion && PlayerSettings.bundleVersion.Contains(split[count]);
            }

            return sameVersion;
        }
        #endregion

        #region Scene
        public static string[] GetSceneLevels()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            List<String> scenesPath = new List<String>();//string[scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    scenesPath.Add(scenes[i].path);
                }
            }

            return scenesPath.ToArray();
        }

        public static string[] GetSceneList(BuildEditorSettings.Step _step)
        {
            string[] scenes = new string[_step.scenes.Count];

            for (int count = 0; count < _step.scenes.Count; count++)
            {
                scenes[count] = AssetDatabase.GetAssetPath(_step.scenes[count]); //+ "/" + _step.scenes[count].name;
            }

            return scenes;
        }
        #endregion

        #region Logs
        public static void Log(string _log)
        {
            Debug.Log(_log);

            if (terminal)
            {
                Console.WriteLine(_log);
            }
        }

        public static void LogError(string _error)
        {
            Debug.LogError(_error);

            if (terminal)
            {
                Console.WriteLine(_error);
            }
        }
        #endregion

        #region Targets
        public static string GetShortTargetName(BuildTarget _target)
        {
            switch (_target)
            {
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.StandaloneWindows:
                    return "Win32";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.StandaloneWindows64:
                    return "Win64";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.WSAPlayer:
                    return "WSApp";
                case BuildTarget.StandaloneLinux64:
                    return "Linux";
                case BuildTarget.PS4:
                    return "PS4";
                case BuildTarget.XboxOne:
                    return "XOne";
                case BuildTarget.tvOS:
                    return "tvOS";
                case BuildTarget.Switch:
                    return "Switch";
                case BuildTarget.Lumin:
                    return "Lumin";
                case BuildTarget.Stadia:
                    return "Stadia";
            }

            return "Unknow";
        }

        public static BuildTargetGroup GetTargetGroup(BuildTarget _target)
        {
            switch (_target)
            {
                case BuildTarget.StandaloneOSX:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneWindows:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;
                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;
                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;
                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;
                case BuildTarget.Switch:
                    return BuildTargetGroup.Switch;
                case BuildTarget.Lumin:
                    return BuildTargetGroup.Lumin;
                case BuildTarget.Stadia:
                    return BuildTargetGroup.Stadia;
            }

            return BuildTargetGroup.Unknown;
        }

        public static string GetTargetExtension(BuildTarget _target)
        {
            switch (_target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
            }

            return "";
        }

        public static string[] GetAllShortTargetName()
        {
            string[] targets = new string[14];
            targets[0] = "OSX";
            targets[1] = "Win32";
            targets[2] = "Win64";
            targets[3] = "iOS";
            targets[4] = "Android";
            targets[5] = "WebGL";
            targets[6] = "WSApp";
            targets[7] = "Linux";
            targets[8] = "PS4";
            targets[9] = "XOne";
            targets[10] = "tvOS";
            targets[11] = "Switch";
            targets[12] = "Lumin";
            targets[13] = "Stadia";

            return targets;
        }
        #endregion
    }
}