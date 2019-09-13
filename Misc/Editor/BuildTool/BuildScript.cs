using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace Falcone.BuildTool
{
    public class BuildScript
    {
        #region Declares
        public class ExtraSettings
        {
            public List<string> targets = new List<string>();
            public List<string> tags = new List<string>();
            public bool forceDev;
            public bool noAlerts;

            public int forceSequence = -1;

            public static ExtraSettings Dev()
            {
                ExtraSettings extra = new ExtraSettings();
                extra.forceDev = true;

                return extra;
            }
        }

        public delegate void OnStep();
        #endregion

        #region Events
        public static OnStep onStep;
        public static OnStep onComplete;
        #endregion

        #region Progress
        public static int totalParts = 0;
        public static int currParts = 0;

        public static float Progress
        {
            get
            {
                if(totalParts == 0)
                {
                    return 0f;
                }

                return Mathf.Clamp01(currParts / totalParts);
            }
        }

        public static bool isBuilding = false;
        public static string currStep = "";
        #endregion

        #region Settings
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        static int tempSequence = -1;
        #endregion

        #region Premade Builds
        #region IOS Build
        [MenuItem("Build/IOs/Development", false, 1)]
        static void BuildiOSDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.iOS));

            BuildAll(null, extra);
        }

        [MenuItem("Build/IOs/Release", false, 2)]
        static void BuildiOSRelease()
        {
            ExtraSettings extra = new ExtraSettings();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.iOS));

            BuildAll(null, extra);
        }

        [MenuItem("Build/IOs/All", false, 14)]
        static void BuildiOSAll()
        {
            BuildiOSDev();
            BuildiOSRelease();
        }
        #endregion

        #region Android Build
        [MenuItem("Build/Android/Development", false, 1)]
        static void BuildAndroidDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.Android));

            BuildAll(null, extra);
        }

        [MenuItem("Build/Android/Release", false, 2)]
        static void BuildAndroidRelease()
        {
            ExtraSettings extra = new ExtraSettings();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.Android));

            BuildAll(null, extra);
        }

        [MenuItem("Build/Android/All", false, 14)]
        static void BuildAndroidAll()
        {
            BuildAndroidDev();
            BuildAndroidRelease();
        }
        #endregion

        #region WebGL Build
        [MenuItem("Build/WebGL/Development", false, 1)]
        static void BuildWebDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.WebGL));

            BuildAll(null, extra);
        }

        [MenuItem("Build/WebGL/Release", false, 2)]
        static void BuildWebRelease()
        {
            ExtraSettings extra = new ExtraSettings();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.WebGL));

            BuildAll(null, extra);
        }

        [MenuItem("Build/WebGL/All", false, 14)]
        static void BuildWebAll()
        {
            BuildWebDev();
            BuildWebRelease();
        }
        #endregion

        #region PC Build
        [MenuItem("Build/PC/Development", false, 1)]
        static void BuildPCDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();

            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneOSX));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneLinux64));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows64));

            BuildAll(null, extra);
        }

        [MenuItem("Build/PC/Release", false, 2)]
        static void BuildPCRelease()
        {
            ExtraSettings extra = new ExtraSettings();

            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneOSX));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneLinux64));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows64));

            BuildAll(null, extra);
        }

        [MenuItem("Build/PC/All", false, 14)]
        static void BuildPCAll()
        {
            BuildPCDev();
            BuildPCRelease();
        }
        #endregion

        #region All
        [MenuItem("Build/Build All", false, 14)]
        static void BuildAll()
        {
            BuildAll();
        }

        public static void BuildAll(BuildEditorSettings _setting = null, ExtraSettings _extra = null)
        {
            if (_setting == null)
            {
                BuildEditorSettings[] settings = Resources.FindObjectsOfTypeAll<BuildEditorSettings>();

                if (settings.Length == 0)
                {
                    BuildScriptUtilities.LogError("No build setting found!");
                    return;
                }

                _setting = settings[0];
            }

            BuildSettings(_setting, _extra);
        }
        #endregion

        #region CLI
        [RuntimeInitializeOnLoadMethod]
        static void BuildCLI()
        {
            BuildScriptUtilities.terminal = false;

            ExtraSettings extra = new ExtraSettings();
            extra.noAlerts = true;

            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-targets")
                {
                    i++;

                    while (i < args.Length && !args[i].Contains("-"))
                    {
                        extra.targets.Add(args[i++]);
                    }

                    if (extra.targets.Count == 0)
                    {
                        Console.WriteLine("ERROR: no valid targets found!");
                        return;
                    }
                }
                else if (args[i] == "-dev")
                {
                    extra.forceDev = true;
                }
                else if (args[i] == "-sequence")
                {
                    int newSequence = -1;

                    if(int.TryParse(args[++i], out newSequence))
                    {
                        extra.forceSequence = newSequence;
                    }
                }
                else if (args[i] == "-terminal")
                {
                    BuildScriptUtilities.terminal = true;
                }
                else if (args[i] == "-tag")
                {
                    i++;

                    while (i < args.Length && !args[i].Contains("-"))
                    {
                        extra.tags.Add(args[i++]);
                    }

                    if (extra.tags.Count == 0)
                    {
                        Console.WriteLine("ERROR: no valid tags found!");
                        return;
                    }
                }
            }

            BuildAll(null, extra);
        }
        #endregion
        #endregion

        #region Functions
        public static string ParseString(string _base)
        {
            foreach (var key in dictionary)
            {
                _base = _base.Replace(key.Key, key.Value);
            }

            return _base;
        }

        static void CreateDictionary()
        {
            AddToDictionary("Project_Name", PlayerSettings.productName);
            AddToDictionary("Company_Name", PlayerSettings.companyName);
            AddToDictionary("Asset_Folder", Application.dataPath+"/");
            AddToDictionary("Root_Folder",  Application.dataPath.Replace("/Assets", "/"));

            DateTime saveUtcNow = DateTime.UtcNow;

            AddToDictionary("Date",           saveUtcNow.ToShortDateString()+" "+ saveUtcNow.ToShortTimeString());

            AddToDictionary("Date.ShortFull", saveUtcNow.ToShortDateString() + " " + saveUtcNow.ToShortTimeString());
            AddToDictionary("Date.LongFull",  saveUtcNow.ToLongDateString() + " " + saveUtcNow.ToLongTimeString());

            AddToDictionary("Date.Path",      DateTime.Now.ToString("yyyyMMddHHmmss"));
            AddToDictionary("Date.Path.Date", DateTime.Now.ToString("yyyyMMdd"));
            AddToDictionary("Date.Path.Time", DateTime.Now.ToString("HHmmss"));

            AddToDictionary("Date.ShortDate", saveUtcNow.ToShortDateString());
            AddToDictionary("Date.LongDate",  saveUtcNow.ToLongDateString());
            AddToDictionary("Date.ShortTime", saveUtcNow.ToShortTimeString());
            AddToDictionary("Date.LongTime",  saveUtcNow.ToLongTimeString());

            AddToDictionary("Date.Day",       saveUtcNow.Day.ToString());
            AddToDictionary("Date.Month",     saveUtcNow.Month.ToString());
            AddToDictionary("Date.Year",      saveUtcNow.Year.ToString());
        }

        static void AddToDictionary(string _key, string _value)
        {
            dictionary["{" + _key + "}"] = _value;
        }

        public static void ZipBuild(string _build, string _path, string _file)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(_build, _path + _file);
        }

        static void CalcBuildProgress(BuildEditorSettings _settings)
        {
            currParts = 0;

            totalParts = _settings.Steps.Length;

            for (int count = 0; count < _settings.Steps.Length; count++)
            {
                totalParts += _settings.Steps[count].preBuildActions.Length;
                totalParts += _settings.Steps[count].postBuildActions.Length;

                if (_settings.Steps[count].zipBuild)
                {
                    totalParts++;
                }

                if (_settings.Steps[count].overwriteScenes)
                {
                    totalParts++;
                }
            }
        }

        static void SetStep(string _step, bool _countStep = true)
        {
            currStep = _step;

            if (_countStep)
            {
                currParts++;
            }

            Debug.Log(_step);

            if(BuildScriptUtilities.terminal)
            {
                Console.WriteLine(_step);
            }

            if(onStep != null) onStep();
        }
        #endregion

        #region Build
        static bool PreBuild(BuildEditorSettings _settings, ExtraSettings _extra = null, bool _changeVersion = true)
        {
            CreateDictionary();

            if (_settings.controlVersion)
            {
                if (BuildScriptUtilities.IsNewSequence(_settings))
                {
                    _settings.sequence++;
                }
                else
                {
                    _settings.sequence = 0;
                }

                tempSequence = _settings.sequence;

                if (_extra != null)
                {
                    if(_extra.forceSequence != -1)
                    {
                        tempSequence = _extra.forceSequence;
                    }
                }

                AddToDictionary("Sequence", tempSequence.ToString());

                string version = ParseString(_settings.Version);

                AddToDictionary("Version", version);

                PlayerSettings.bundleVersion = version;
            }

            return true;
        }

        static void BuildSettings(BuildEditorSettings _settings, ExtraSettings _extra = null)
        {
            isBuilding = true;

            PreBuild(_settings);

            //Execute Pre Actions
            for (int count = 0; count < _settings.preBuildActions.Length; count++)
            {
                SetStep("Building Step: Executing Generic post action " + _settings.preBuildActions[count].GetName());

                if (!_settings.preBuildActions[count].Exec(_settings, null, _extra, null, null))
                {
                    if (!string.IsNullOrEmpty(_settings.preBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Post Step " + _settings.preBuildActions[count].GetName() +
                                                      " returned a error - " + _settings.preBuildActions[count].GetError());
                    }
                }
            }

            for (int count = 0; count < _settings.Steps.Length; count++)
            {
                BuildStep(_settings, _settings.Steps[count], _extra);
            }

            //Execute Post Actions
            for (int count = 0; count < _settings.postBuildActions.Length; count++)
            {
                SetStep("Building Step: Executing Generic post action " + _settings.postBuildActions[count].GetName());

                if (!_settings.postBuildActions[count].Exec(_settings, null, _extra, null, null))
                {
                    if (!string.IsNullOrEmpty(_settings.postBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Post Step " + _settings.postBuildActions[count].GetName() +
                                                      " returned a error - " + _settings.postBuildActions[count].GetError());
                    }
                }
            }

            isBuilding = false;
            if (onComplete != null) onComplete();
        }

        static void BuildStep(BuildEditorSettings _settings, BuildEditorSettings.Step _step, ExtraSettings _extra = null)
        {
            SetStep("Building Step: " + _step.Target);

            AddToDictionary("Short_Target", BuildScriptUtilities.GetShortTargetName(_step.Target));
            AddToDictionary("Long_Target", _step.Target.ToString());
            AddToDictionary("Extension", BuildScriptUtilities.GetTargetExtension(_step.Target));

            string buildPath = ParseString(_settings.Path);
            string filePath = ParseString(_settings.File);

            //Replace step if necessary
            if (_step.overwritePath)
            {
                buildPath = ParseString(_step.path);
            }

            //Check if a custom build procress is set
            if (_step.overwriteStep != null)
            {
                _step.overwriteStep.Exec(_settings, _step, _extra, buildPath, filePath);
                return;
            }

            //Filter by tag and platform
            if (_extra != null)
            {
                if(_extra.targets.Count > 0)
                {
                    if(!_extra.targets.Contains(BuildScriptUtilities.GetShortTargetName(_step.Target)))
                    {
                        BuildScriptUtilities.Log("Skipping "+ _step.Name+ " : Missing target "+ String.Join(", ", _extra.targets.ToArray()));
                        return;
                    }
                }

                if (_extra.tags.Count > 0)
                {
                    string[] labels = _step.Labels.Split(' ');
                    bool found = false;

                    for (int count = 0; count < labels.Length; count++)
                    {
                        if (_extra.tags.Contains(labels[count]))
                        {
                            found = true;
                            break;
                        }
                    }

                    if(!found)
                    {
                        BuildScriptUtilities.Log("Skipping " + _step.Name + " : Missing label " + String.Join(", ", _extra.tags.ToArray()));
                        return;
                    }
                }
            }

            //Check if platform is different
            if (!_extra.noAlerts)
            {
                if(_step.Target != EditorUserBuildSettings.activeBuildTarget)
                {
                    if (EditorUtility.DisplayDialog("Switch Targets?", "Do you want to switch platforms? It can take several minutes", "Cancel", "Switch"))
                    {
                        BuildScriptUtilities.Log("Stopping Step "+ _step.Name);
                        return;
                    }
                }
            }

            //Switching platforms
            SetStep("Building Step: " + _step.Name + " - Switching Platform to "+ _step.Target);

            bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildScriptUtilities.GetTargetGroup(_step.Target), _step.Target);

            if (!switchResult)
            {
                BuildScriptUtilities.Log("Unable to change Build Target to: " + _step.Target.ToString());
                return;
            }

            //Get Scenes from overwrite or the editor
            string[] scenes;

            SetStep("Building Step: " + _step.Name +" - Getting Scenes");

            if (_step.overwriteScenes)
            {
                scenes = BuildScriptUtilities.GetSceneList(_step);
            }
            else
            {
                scenes = BuildScriptUtilities.GetSceneLevels();
            }

            SetStep("Building Step: " + _step.Name + " - Creating Directory");
            Directory.CreateDirectory(buildPath);

            //Execute Pre Actions
            for (int count = 0; count < _step.preBuildActions.Length; count++)
            {
                SetStep("Building Step: " + _step.Name + " - Executing pre action "+ _step.preBuildActions[count].name);

                if (!_step.preBuildActions[count].Exec(_settings, _step, _extra, buildPath, filePath))
                {
                    if (!string.IsNullOrEmpty(_step.postBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Pre Step " + _step.postBuildActions[count].GetName() +
                                                      " returned a error - " + _step.postBuildActions[count].GetError());
                    }
                }
            }

            SetStep("Building Step: " + _step.Name + " - Building Project");
            //Create the Build and Zip

            BuildOptions options = _step.Option;

            if (_extra != null && _extra.forceDev)
            {
                BuildScriptUtilities.Log("Forcing Dev Mode");
                options |= BuildOptions.Development;
            }

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, buildPath + "/" + filePath, _step.Target, options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if(summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                BuildScriptUtilities.LogError("Building Step: Build Failed - Target: "+ _step.Name + " Time:" + summary.totalTime + " Total Errors: " + summary.totalErrors);
                return;
            }

            if (_step.zipBuild)
            {
                SetStep("Building Step: " + _step.Name + " - Zipping Project");

                string parentFolder = System.IO.Directory.GetParent(buildPath).ToString();
                ZipBuild(buildPath, parentFolder+".zip", "");
            }

            //Execute Post Actions
            for (int count = 0; count < _step.postBuildActions.Length; count++)
            {
                SetStep("Building Step: " + _step.Name + " - Executing post action " + _step.postBuildActions[count].GetName());

                if (!_step.postBuildActions[count].Exec(_settings, _step, _extra, buildPath, filePath))
                {
                    if (!string.IsNullOrEmpty(_step.postBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Post Step "+ _step.postBuildActions[count].GetName()+
                                                      " returned a error - "+ _step.postBuildActions[count].GetError());
                    }
                }
            }
        }
        #endregion
    }
}