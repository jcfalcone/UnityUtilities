using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class BuildScript 
{
    static string[] devScene = { "Assets/Scenes/Level1.unity" };
    static string[] relScene = { "Assets/Scenes/Game.unity" };

    static string[] finalDevScene = {};
    static string[] finalRelScene = {};
	// Use this for initialization

    #region IOS Build
    [MenuItem("Build/IOs/Development", false, 1)]
    static void BuildiOSDev () 
    {
        BuildiOS (true);
    }

    [MenuItem("Build/IOs/Release", false, 2)]
    static void BuildiOSRelease () 
    {
        BuildiOS (false);
    }

    [MenuItem("Build/IOs/All", false, 14)]
    static void BuildiOSAll () 
    {
        BuildiOS (true);
        BuildiOS (false);
    }

    static void BuildiOS (bool isDev) 
    {

        BuildOptions options = isDev ? BuildOptions.Development : BuildOptions.None;

        bool updateVersion = true;

        if (EditorUtility.DisplayDialog("Append Project?", "Do you want to append the XCode project or start over?", "Append", "Start Over"))
        {
            options |= BuildOptions.AcceptExternalModificationsToPlayer;
            updateVersion = false;
        }


        if(!PreBuild(updateVersion))
        {
            return;
        }

        string buildPath = Application.dataPath.Replace("/Assets", "/Builds")+"/iOS/";

        if (isDev)
            buildPath += "Dev";
        else
            buildPath += PlayerSettings.productName;

        buildPath += "_"+PlayerSettings.bundleVersion.Replace(".","_");

        options |= BuildOptions.AutoRunPlayer;

        BuildPipeline.BuildPlayer(isDev ? finalDevScene : finalRelScene, buildPath, BuildTarget.iOS, options);
	}
    #endregion

    #region Android Build
    [MenuItem("Build/Android/Development", false, 1)]
    static void BuildAndroidDev () 
    {
        BuildAndroid (true);
    }

    [MenuItem("Build/Android/Release", false, 2)]
    static void BuildAndroidRelease () 
    {//t
        BuildAndroid (false);
    }

    [MenuItem("Build/Android/All", false, 14)]
    static void BuildAndroidAll () 
    {
        BuildAndroid (true);
        BuildAndroid (false);
    }

    static void BuildAndroid (bool isDev) 
    {
        if(!PreBuild())
        {
            return;
        }

        string buildPath = Application.dataPath.Replace("/Assets", "/Builds")+"/Android/";

        buildPath += "_"+PlayerSettings.bundleVersion.Replace(".","_");

        if (isDev)
            buildPath += "Dev";
        else
            buildPath += PlayerSettings.productName;

        BuildPipeline.BuildPlayer(isDev ? finalDevScene : finalRelScene, buildPath, BuildTarget.Android, isDev ? BuildOptions.Development : BuildOptions.None);
    }
    #endregion

    #region WebGL Build
    [MenuItem("Build/WebGL/Development", false, 1)]
    static void BuildWebDev () 
    {
        BuildWeb (true);
    }

    [MenuItem("Build/WebGL/Release", false, 2)]
    static void BuildWebRelease () 
    {
        BuildWeb (false);
    }

    [MenuItem("Build/WebGL/All", false, 14)]
    static void BuildWebAll () 
    {
        BuildWeb (true);
        BuildWeb (false);
    }

    static void BuildWeb (bool isDev) 
    {
        if(!PreBuild())
        {
            return;
        }

        string buildPath = Application.dataPath.Replace("/Assets", "/Builds")+"/WebGL/";

        if (isDev)
            buildPath += "Dev";
        else
            buildPath += PlayerSettings.productName;

        buildPath += "_"+PlayerSettings.bundleVersion.Replace(".","_");

        BuildPipeline.BuildPlayer(isDev ? finalDevScene : finalRelScene, buildPath, BuildTarget.WebGL, isDev ? BuildOptions.Development : BuildOptions.None);
    }
    #endregion

    #region PC Build
    [MenuItem("Build/PC/Development", false, 1)]
    static void BuildPCDev () 
    {
        BuildPC (true);
    }

    [MenuItem("Build/PC/Release", false, 2)]
    static void BuildPCRelease () 
    {
        BuildPC (false);
    }

    [MenuItem("Build/PC/All", false, 14)]
    static void BuildPCAll () 
    {
        BuildPC (true);
        BuildPC (false);
    }

    static void BuildPC (bool isDev) 
    {
        if(!PreBuild())
        {
            return;
        }

        string buildPath = Application.dataPath.Replace("/Assets", "/Builds")+"/PC/";

        if (isDev)
            buildPath += "Dev";
        else
            buildPath += PlayerSettings.productName;


        buildPath += PlayerSettings.bundleVersion.Replace(".","_");

        Directory.CreateDirectory(buildPath);

        BuildPipeline.BuildPlayer(isDev ? finalDevScene : finalRelScene, buildPath+"/"+PlayerSettings.productName+"_Mac", BuildTarget.StandaloneOSX, isDev ? BuildOptions.Development : BuildOptions.None);
        BuildPipeline.BuildPlayer(isDev ? finalDevScene : finalRelScene, buildPath+"/"+PlayerSettings.productName+"_Windows.exe", BuildTarget.StandaloneWindows, isDev ? BuildOptions.Development : BuildOptions.None);
        BuildPipeline.BuildPlayer(isDev ? finalDevScene : finalRelScene, buildPath+"/"+PlayerSettings.productName+"_Linux", BuildTarget.StandaloneLinux, isDev ? BuildOptions.Development : BuildOptions.None);
    }
    #endregion

    #region Functions

    static bool PreBuild(bool _changeVersion = true)
    {
        if(!DefaultScenes())
        {
            return false;
        }
        SetVersion(_changeVersion);

        return true;
    }

    static bool DefaultScenes()
    {
        finalDevScene = devScene;
        finalRelScene = relScene;

        int option = EditorUtility.DisplayDialogComplex("Use Default Scenes?",
                "Do you want to use the default scenes?",
                "Default",
                "Cancel",
                "Script");

        if (option == 0)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            List<String> scenesPath = new List<String>();//string[scenes.Length];

            int levelCount = 0;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    scenesPath.Add(scenes[i].path);
                }
            }

            finalDevScene = scenesPath.ToArray();
            finalRelScene = scenesPath.ToArray();
        }

        if(option == 1)
        {
            return false;
        }

        return true;
    }

    static void SetVersion(bool _changeVersion)
    {
        string[] version = PlayerSettings.bundleVersion.Split('.');
        DateTime saveUtcNow = DateTime.UtcNow;

        int currCount = 0;

        if (version.Length >= 4)
        {
            if (version[0] == saveUtcNow.Year.ToString() && version[1] == saveUtcNow.Month.ToString() && version[2] == saveUtcNow.Day.ToString())
            {
                currCount = Int32.Parse(version[3]) + 1;
            }

            if (!_changeVersion)
            {
                currCount = Int32.Parse(version[3]);
            }
        }

        PlayerSettings.bundleVersion = saveUtcNow.Year.ToString() + "." + saveUtcNow.Month.ToString() + "." + saveUtcNow.Day.ToString() + "." + currCount.ToString();
    }
    #endregion

    [MenuItem("Build/Build All", false, 14)]
    static void BuildAll () 
    {
        BuildiOSAll();
        BuildAndroidAll();
        BuildWebAll();
        BuildPCAll();
    }
}